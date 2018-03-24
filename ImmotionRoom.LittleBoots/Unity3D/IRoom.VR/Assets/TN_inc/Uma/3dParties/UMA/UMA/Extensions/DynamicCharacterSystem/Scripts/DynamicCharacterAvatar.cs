using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.PoseTools;//so we can set the expression set based on the race
using System.IO;
using UMAAssetBundleManager;


namespace UMACharacterSystem
{
    //TODO when a scene that contains this component is included in an asset bundle it ends up having dependencies on some things that are defined in here
    // namely the race data and the default animator - I think these need to have no refrence to any assets at all so that the scene looses these dependencies
    // because the avatar is supposed to be dynamic and therefore of an undefined race and using an undefined animator. The same is true of the expressions player
    //this need to not have an expressionSet set so again the scene does not end up dependent on the asset that contains it
    public class DynamicCharacterAvatar : UMAAvatarBase
	{
        /// <summary>
        /// Callback event when the character recipe is updated. Use this to tweak the resulting recipe BEFORE the UMA is actually generated
        /// </summary>
        public UMADataEvent RecipeUpdated;
        //because the character might be loaded from an asset bundle, we may want everything required to create it to happen
        //but for it to still not be shown immediately
        [Tooltip("If checked will turn off the SkinnedMeshRenderer after the character has been created to hide it. If not checked will turn it on again.")]
		public bool hide = false;


        /// <summary>
        /// Set this before initialization to determine the active race. This can be set in the inspector
        /// using the activeRace dropdown.
        /// </summary>
        public string RacePreset
        {
            get
            {
                return activeRace.name;
            }
            set
            {
                activeRace.name = value;
            }
        }

        //This will generate itself from a list available Races and set itself to the current value of activeRace.name
        [Tooltip("Selects the race to used. When initialized, the Avatar will use the base recipe from the RaceData selected.")]
        public RaceSetter activeRace = new RaceSetter();

		//bool flagForRebuild = false;
		//bool flagForReload = false;

		public Dictionary<string, UMATextRecipe> WardrobeRecipes = new Dictionary<string, UMATextRecipe>();

		[Tooltip("You can add wardrobe recipes for many races in here and only the ones that apply to the active race will be applied to the Avatar")]
		public WardrobeRecipeList preloadWardrobeRecipes = new WardrobeRecipeList();

		[Tooltip("Add animation controllers here for specific races. If no Controller is found for the active race, the Default Animation Controller is used")]
		public RaceAnimatorList raceAnimationControllers= new RaceAnimatorList();

		[Tooltip("Any colors here are set when the Avatar is first generated and updated as the values are changed using the color sliders")]
		public ColorValueList characterColors = new ColorValueList();

		//in order to preserve the state of the Avatar when switching races (rather than having it loose its wardrobe when switching back and forth) we need to cache its current state before changing to the desired race
		Dictionary<string, CacheStateInfo> cacheStates = new Dictionary<string, CacheStateInfo>();
		class CacheStateInfo
		{
			public Dictionary<string, UMATextRecipe> wardrobeCache = new Dictionary<string, UMATextRecipe>();
			public List<ColorValue> colorCache = new List<ColorValue>();
			public CacheStateInfo(Dictionary<string, UMATextRecipe> _wardrobeCache, List<ColorValue> _colorCache)
			{
				wardrobeCache = _wardrobeCache;
				colorCache = _colorCache;
			}
			public CacheStateInfo(List<ColorValue> _colorCache)
			{
				wardrobeCache = null;
				colorCache = _colorCache;
			}
		}
		//load/save fields
		public enum loadPathTypes { persistentDataPath, streamingAssetsPath, Resources, FileSystem, CharacterSystem, String };
		public loadPathTypes loadPathType;
		public string loadPath;
		public string loadFilename;
        public string loadString;
		public bool loadFileOnStart;
		
		//backwards compatibility for the 'loadOptions' addition - can probably be removed
		public bool waitForBundles {
			get { return loadOptions.waitForBundles; }
			set { loadOptions.waitForBundles = value;  }
		}

		[System.Serializable]
		public class LoadOptions
		{
			[Tooltip("If true, if a loaded recipe requires assetBundles to download, the Avatar will wait until they are downloaded before creating itself. Otherwise a temporary character will be shown.")]
			public bool waitForBundles = true;
			public bool loadRace = true;
			public bool loadDNA = true;
			public bool loadWardrobe = true;
			[Tooltip("If true and the loaded recipe has shared colors that match any of the active race baseRaceRecipe shared colors, these will be loaded")]
			public bool loadBodyColors = true;
			public bool loadWardrobeColors = true;
        }

		public enum savePathTypes { persistentDataPath, streamingAssetsPath, Resources, FileSystem };
		public savePathTypes savePathType;
		public string savePath;
		public string saveFilename;
		public bool makeUnique;
		[Tooltip("The default options for when a character is loaded from an UMATextRecipe asset or a recipe string. Can be overidden when calling 'LoadFromRecipe' or 'LoadFromString' directly.")]
		public LoadOptions loadOptions = new LoadOptions();
		public Vector3 BoundsOffset;

		private HashSet<string> HiddenSlots = new HashSet<string>();
		[HideInInspector]
		public List<string> assetBundlesUsedbyCharacter = new List<string>();
		[HideInInspector]
		public bool AssetBundlesUsedbyCharacterUpToDate = true;

		//a list of downloading assets that the avatar can check the download status of.
		[HideInInspector]
		public List<string> requiredAssetsToCheck = new List<string>();


		//this gets set by UMA when the chracater is loaded, but we need to know the race before hand to we can decide what wardrobe slots to use, so we have a work around above with RaceSetter
		public RaceData RaceData
		{
			get { return base.umaRace; }
			private set { base.umaRace = value; }
		}

        /// <summary>
        /// This returns all the recipes for the current race of the avatar.
        /// </summary>
        public Dictionary<string, List<UMATextRecipe>> AvailableRecipes
        {
            get
            {
                return (context.dynamicCharacterSystem as DynamicCharacterSystem).Recipes[RaceData.raceName];
            }
        }

        public List<string> CurrentWardrobeSlots
        {
            get
            {
                return RaceData.wardrobeSlots;
            }
        }

        public OverlayColorData[] CurrentSharedColors
        {
            get
            {
                return umaData.umaRecipe.sharedColors;
            }
        }

        public List<ColorValue> ActiveColors
        {
            get
            {
                return characterColors.Colors;
            }
        }



        public override void Start()
		{
			//cache the starting colors
			var tempCols = new List<ColorValue>();
			foreach (ColorValue col in characterColors.Colors)
			{
				tempCols.Add(new ColorValue(col));
			}
			var thisCacheStateInfo = new CacheStateInfo(tempCols);
			cacheStates.Add("NULL", thisCacheStateInfo);
			//
			StopAllCoroutines();
			base.Start();
			//if the animator has been set the 'old' way respect that...
			if (raceAnimationControllers.defaultAnimationController == null && animationController != null)
			{
				raceAnimationControllers.defaultAnimationController = animationController;
			}
			if ((loadFilename != "" && loadFileOnStart) || (loadPathType == loadPathTypes.String))
			{
				DoLoad();
			}
			else
			{
				StartCoroutine(StartStartCoroutine());
			}
		}

		public void Update()
		{
			if (hide == true)
			{
				if (umaData != null)
				{
					if (umaData.myRenderer != null)
					{
						umaData.myRenderer.enabled = false;
					}
				}
			}
			else
			{
				if (umaData != null)
				{
					if (umaData.myRenderer != null)
					{
						umaData.myRenderer.enabled = true;
					}
				}
			}
			//This hardly ever happens now since the changeRace/LoadFromString/StartStarCoRoutine methods all yield themselves until asset bundles have been downloaded
			if (requiredAssetsToCheck.Count > 0 && loadOptions.waitForBundles == false)
			{
				if (DynamicAssetLoader.Instance.downloadingAssetsContains(requiredAssetsToCheck) == false)
				{
					Debug.Log("Update did build");
					requiredAssetsToCheck.Clear();
					activeRace.data = context.raceLibrary.GetRace(activeRace.name);
					umaRecipe = activeRace.data.baseRaceRecipe;
					UpdateSetSlots();
					//actually we dont know in this case if we are restoring DNA or not
					//and a placeholder race should only have been used if loadOptions.waitForBundles is false
					BuildCharacter(loadOptions.waitForBundles);
				}
			}
		}

		IEnumerator StartStartCoroutine()
		{
			if(umaRecipe != null)
			{
				StartCoroutine(LoadFromRecipeCO());
				yield break;
			}
			bool mayRequireDownloads = false;
			//calling activeRace.data checks the actual RaceLibrary to see if the raceData is in there since in the editor the asset may still be there even if it shouldn't be (i.e. is in an asset bundle)

			var batchID = DynamicAssetLoader.Instance.GenerateBatchID();
			if (activeRace.data == null)//activeRace.data will get the asset from the library OR add it from Resources so if it's null it means its going to be downloaded from an asset bundle OR is just wrong
			{
				mayRequireDownloads = true;//but we dont know if its wrong until the index is downloaded so we have to wait for that
				if (DynamicAssetLoader.Instance.downloadingAssetsContains(activeRace.name))
				{
					requiredAssetsToCheck.Add(activeRace.name);
				}
			}
			if (mayRequireDownloads)
			{
				while (!DynamicAssetLoader.Instance.isInitialized)
				{
					yield return null;
				}
				//if we are in the editor SimulateOverride will becme true if we go into SimulationMode. This happens when the localAssetBundleServer is off
				if (AssetBundleManager.SimulateOverride == true)
				{
					mayRequireDownloads = false;
				}
			}
			if (umaRecipe == null)
			{
				DynamicAssetLoader.Instance.CurrentBatchID = batchID;
				SetStartingRace();
				//If the UmaRecipe is still null, bail - we cant go any further (and SetStartingRace will have shown an error)
				if (umaRecipe == null)
				{
					yield break;
				}
				if (requiredAssetsToCheck.Count > 0)
					mayRequireDownloads = true;
			}
			if (preloadWardrobeRecipes.loadDefaultRecipes && preloadWardrobeRecipes.recipes.Count > 0 /*&& umaRecipeWasNull*/)
			{
				DynamicAssetLoader.Instance.CurrentBatchID = batchID;
				LoadDefaultWardrobe(true);//this may cause downloads to happen
				if (requiredAssetsToCheck.Count > 0)
					mayRequireDownloads = true;
			}
			if (raceAnimationControllers.animators.Count > 0)
			{
				DynamicAssetLoader.Instance.CurrentBatchID = batchID;
				var animators = raceAnimationControllers.Validate();//this may cause downloads to happen
				if (animators.Count > 0)
				{
					for (int i = 0; i < animators.Count; i++)
					{
						if (!requiredAssetsToCheck.Contains(animators[i]))
						{
							if (DynamicAssetLoader.Instance.downloadingAssetsContains(animators[i]))
								requiredAssetsToCheck.Add(animators[i]);
						}
					}
				}
				if (requiredAssetsToCheck.Count > 0)
					mayRequireDownloads = true;
			}
			if(!loadOptions.waitForBundles && requiredAssetsToCheck.Count > 0)
			{
				BuildCharacter(false);//if we are not waiting for bundles before building, build the placeholder avatar now
			}
			if (requiredAssetsToCheck.Count > 0)
			{
				while (DynamicAssetLoader.Instance.downloadingAssetsContains(requiredAssetsToCheck))
				{
					yield return null;
				}
			}
			requiredAssetsToCheck.Clear();
			activeRace.data = context.raceLibrary.GetRace(activeRace.name);
			umaRecipe = activeRace.data.baseRaceRecipe;
			UpdateSetSlots();
			BuildCharacter(false);
		}

		/// <summary>
		/// Sets the starting race of the avatar based on the value of the 'activeRace' dropdown
		/// </summary>
		void SetStartingRace()
		{
			//calling activeRace.data causes RaceLibrary to gather all racedatas from resources an returns all those along with any temporary assetbundle racedatas that are downloading
			//It will not cause any races to actually download
			if (activeRace.data != null)
			{
				activeRace.name = activeRace.data.raceName;
				umaRecipe = activeRace.data.baseRaceRecipe;
			}
			//otherwise...
			else if (activeRace.name != null)
			{
				//This only happens when the Avatar itself has an active race set to be one that is in an assetbundle
				activeRace.data = context.raceLibrary.GetRace(activeRace.name);// this will trigger a download if the race is in an asset bundle and return a temp asset
				if(activeRace.racedata != null)
					umaRecipe = activeRace.racedata.baseRaceRecipe;
			}
			//Failsafe: if everything else fails we try to do something based on the name of the race that was set
			if (umaRecipe == null)
			{//if its still null just load the first available race- try to match the gender at least
				var availableRaces = context.raceLibrary.GetAllRaces();
				if (availableRaces.Length > 0)
				{
					bool raceFound = false;
					if (activeRace.name.IndexOf("Female") > -1)
					{
						foreach (RaceData race in availableRaces)
						{
							if (race != null)
							{
								if (race.raceName.IndexOf("Female") > -1 || race.raceName.IndexOf("female") > -1)
								{
									activeRace.name = race.raceName;
									activeRace.data = race;
									umaRecipe = activeRace.racedata.baseRaceRecipe;
									raceFound = true;
								}
							}
						}
					}
					if (!raceFound)
					{
						Debug.Log("No base recipe found for race " + activeRace.name);
						activeRace.name = availableRaces[0].raceName;
						activeRace.data = availableRaces[0];
						umaRecipe = activeRace.racedata.baseRaceRecipe;
					}
				}
				else
				{
					Debug.LogError("[DynamicCharacterAvatar] There were no available racedatas to make an UMA from! Please either put your RaceDatas in a Resources folder or add them to (Dynamic)RaceLibrary directly");
				}
			}
			if (DynamicAssetLoader.Instance.downloadingAssetsContains(activeRace.name))
			{
				if (!requiredAssetsToCheck.Contains(activeRace.name))
					requiredAssetsToCheck.Add(activeRace.name);
			}
		}

		/// <summary>
		/// Loads the default wardobe items set in 'defaultWardrobeRecipes' in the CharacterAvatar itself onto the Avatar's base race recipe. Use this to make a naked avatar always have underwear or a set of clothes for example
		/// </summary>
		/// <param name="allowDownloadables">Optionally allow this function to trigger downloads of wardrobe recipes in an asset bundle</param>
		public void LoadDefaultWardrobe(bool allowDownloadables = false)
		{
			List<WardrobeRecipeListItem> validRecipes = preloadWardrobeRecipes.Validate(allowDownloadables, activeRace.name);
			if (validRecipes.Count > 0)
			{
				foreach (WardrobeRecipeListItem recipe in validRecipes)
				{
					if (recipe._recipe != null)
					{
						if (activeRace.name == "")//should never happen TODO: Check if it does
						{
							SetSlot(recipe._recipe);
							if (!requiredAssetsToCheck.Contains(recipe._recipeName))
							{
								requiredAssetsToCheck.Add(recipe._recipeName);
							}
						}
						else if (((recipe._recipe.compatibleRaces.Count == 0 || recipe._recipe.compatibleRaces.Contains(activeRace.name)) || (activeRace.racedata.findBackwardsCompatibleWith(recipe._recipe.compatibleRaces) && activeRace.racedata.wardrobeSlots.Contains(recipe._recipe.wardrobeSlot))))
						{
							//the check activeRace.data.wardrobeSlots.Contains(recipe._recipe.wardrobeSlot) makes sure races that are backwards compatible 
							//with another race but which dont have all of that races wardrobeslots, dont try to load things they dont have wardrobeslots for
							//However we need to make sure that if a slot has already been assigned that is DIRECTLY compatible with the race it is not overridden
							//by one that is backwards compatible
							if (activeRace.racedata.findBackwardsCompatibleWith(recipe._recipe.compatibleRaces) && activeRace.racedata.wardrobeSlots.Contains(recipe._recipe.wardrobeSlot))
							{
								if (!WardrobeRecipes.ContainsKey(recipe._recipe.wardrobeSlot))
								{
									SetSlot(recipe._recipe);
									if (!requiredAssetsToCheck.Contains(recipe._recipeName))
									{
										requiredAssetsToCheck.Add(recipe._recipeName);
									}
								}
							}
							else
							{
								SetSlot(recipe._recipe);
								if (!requiredAssetsToCheck.Contains(recipe._recipeName))
								{
									requiredAssetsToCheck.Add(recipe._recipeName);
								}
							}
						}
					}
					else
					{
						if (allowDownloadables)
						{
							//this means a temporary recipe was not returned for some reason
							Debug.LogWarning("[CharacterAvatar:LoadDefaultWardrobe] recipe._recipe was null for " + recipe._recipeName);
						}
					}
				}
			}
		}
		/// <summary>
		/// Sets the Expression set for the Avatar based on the Avatars set race.
		/// </summary>
		public void SetExpressionSet()
		{
			if (this.gameObject.GetComponent<UMAExpressionPlayer>() == null)
			{
				return;
			}
			UMAExpressionSet expressionSetToUse = null;
			if (activeRace.racedata != null)
			{
				expressionSetToUse = activeRace.racedata.expressionSet;
			}
			if (expressionSetToUse != null)
			{
				//set the expression set and reset all the values
				var thisExpressionsPlayer = this.gameObject.GetComponent<UMAExpressionPlayer>();
				thisExpressionsPlayer.expressionSet = expressionSetToUse;
				thisExpressionsPlayer.Values = new float[thisExpressionsPlayer.Values.Length];
			}
		}

		/// <summary>
		/// Sets the Animator Controller for the Avatar based on the best match found for the Avatars race. If no animator for the active race has explicitly been set, the default animator is used
		/// </summary>
		public void SetAnimatorController()
		{
			
			int validControllers = raceAnimationControllers.Validate().Count;//triggers resources load or asset bundle download of any animators that are in resources/asset bundles

			RuntimeAnimatorController controllerToUse = raceAnimationControllers.defaultAnimationController;
			if (validControllers > 0)
			{
				foreach (RaceAnimator raceAnimator in raceAnimationControllers.animators)
				{
					if (raceAnimator.raceName == activeRace.name && raceAnimator.animatorController != null)
					{
						controllerToUse = raceAnimator.animatorController;
						break;
					}
				}
			}
			animationController = controllerToUse;
			if (this.gameObject.GetComponent<Animator>())
			{
				this.gameObject.GetComponent<Animator>().runtimeAnimatorController = controllerToUse;
			}
		}

        /// <summary>
        /// Gets the color from the current shared colors.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public OverlayColorData GetColor(string Name)
        {
            OverlayColorData ocd;

            if (characterColors.GetColor(Name, out ocd))
                return ocd;
            return null;
        }

		/// <summary>
		/// Sets the given color name to the given OverlayColorData optionally updating the texture (default:true)
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="colorData"></param>
		/// <param name="UpdateTexture"></param>
		public void SetColor(string Name, OverlayColorData colorData, bool UpdateTexture = true)
		{
			characterColors.SetColor(Name, colorData);
			if (UpdateTexture)
			{
				UpdateColors();
				ForceUpdate(false, UpdateTexture, false);
			}
		}
		/// <summary>
		/// Builds the character by combining the Avatar's raceData.baseRecipe with the any wardrobe recipes that have been applied to the avatar.
		/// </summary>
		/// <returns>Can also be used to return an array of additional slots if this avatars flagForReload field is set to true before calling</returns>
		/// <param name="RestoreDNA">If updating the same race set this to true to restore the current DNA.</param>
		/// <param name="prioritySlot">Priority slot- use this to make slots lower down the wardrobe slot list suppress ones that are higher.</param>
		/// <param name="prioritySlotOver">a list of slots the priority slot overrides</param>
		public UMARecipeBase[] BuildCharacter(bool RestoreDNA = true, string prioritySlot = "", List<string> prioritySlotOver = default(List<string>))
		{
			if (prioritySlotOver == default(List<string>))
				prioritySlotOver = new List<string>();

			HiddenSlots.Clear();

			UMADnaBase[] CurrentDNA = null;
			if (umaData != null)
			{
				if (umaData.umaRecipe != null)
				{
					CurrentDNA = umaData.umaRecipe.GetAllDna();
				}
			}
			if (CurrentDNA == null)
				RestoreDNA = false;

			List<UMARecipeBase> Recipes = new List<UMARecipeBase>();
			List<string> SuppressSlotsStrings = new List<string>();

			if ((preloadWardrobeRecipes.loadDefaultRecipes || WardrobeRecipes.Count > 0) && activeRace.racedata != null)
			{
				foreach (UMATextRecipe utr in WardrobeRecipes.Values)
				{
					if (utr.suppressWardrobeSlots != null)
					{
						if (activeRace.name == "" || ((utr.compatibleRaces.Count == 0 || utr.compatibleRaces.Contains(activeRace.name)) || (activeRace.racedata.findBackwardsCompatibleWith(utr.compatibleRaces) && activeRace.racedata.wardrobeSlots.Contains(utr.wardrobeSlot))))
						{
							if (prioritySlotOver.Count > 0)
							{
								foreach (string suppressedSlot in prioritySlotOver)
								{
									if (suppressedSlot == utr.wardrobeSlot)
									{
										SuppressSlotsStrings.Add(suppressedSlot);
									}
								}
							}
							if (!SuppressSlotsStrings.Contains(utr.wardrobeSlot))
							{
								foreach (string suppressedSlot in utr.suppressWardrobeSlots)
								{
									if (prioritySlot == "" || prioritySlot != suppressedSlot)
										SuppressSlotsStrings.Add(suppressedSlot);
								}
							}
						}
					}
				}

				foreach (string ws in activeRace.racedata.wardrobeSlots)//this doesn't need to validate racedata- we wouldn't be here if it was null
				{
					if (SuppressSlotsStrings.Contains(ws))
					{
						continue;
					}
					if (WardrobeRecipes.ContainsKey(ws))
					{
						UMATextRecipe utr = WardrobeRecipes[ws];
						//we can use the race data here to filter wardrobe slots
						//if checking a backwards compatible race we also need to check the race has a compatible wardrobe slot, 
						//since while a race can be backwards compatible it does not *have* to have all the same wardrobeslots as the race it is compatible with
						if (activeRace.name == "" || ((utr.compatibleRaces.Count == 0 || utr.compatibleRaces.Contains(activeRace.name)) || (activeRace.racedata.findBackwardsCompatibleWith(utr.compatibleRaces) && activeRace.racedata.wardrobeSlots.Contains(utr.wardrobeSlot))))
						{
							Recipes.Add(utr);
							if (utr.Hides.Count > 0)
							{
								foreach (string s in utr.Hides)
								{
									HiddenSlots.Add(s);
								}
							}
						}
					}
				}
			}

			foreach (UMATextRecipe utr in umaAdditionalRecipes)
			{
				if (utr.Hides.Count > 0)
				{
					foreach (string s in utr.Hides)
					{
						HiddenSlots.Add(s);
					}
				}
			}

			// Load all the recipes
			/*if (!flagForReload)
            {
                if (flagForRebuild)
                {
                    flagForReload = true;
                    flagForRebuild = false;
                }*/
			//set the expression set to match the new character- needs to happen before load...
			if (activeRace.racedata != null && !RestoreDNA)
			{
				SetAnimatorController();
				SetExpressionSet();
			}
			Load(umaRecipe, Recipes.ToArray());

			// Add saved DNA
			if (RestoreDNA)
			{
				umaData.umaRecipe.ClearDna();
				foreach (UMADnaBase ud in CurrentDNA)
				{
					umaData.umaRecipe.AddDna(ud);
				}
			}
			return null;
			//}
			/*else
            {
                //CONFIRM THIS IS NOT NEEDED ANY MORE
                flagForReload = false;
                //this is used by the load function in the case where an umaRecipe is directly defined since in this case we dont know what the race of that recipe is until its loaded
                return Recipes.ToArray();
            }*/
		}

		void RemoveHiddenSlots()
		{
			List<SlotData> NewSlots = new List<SlotData>();
			foreach (SlotData sd in umaData.umaRecipe.slotDataList)
			{
				if (sd == null)
					continue;
				if (!HiddenSlots.Contains(sd.asset.slotName))
				{
					NewSlots.Add(sd);
				}
			}
			umaData.umaRecipe.slotDataList = NewSlots.ToArray();
		}

		/// <summary>
		/// Applies these colors to the loaded Avatar and adds any colors the loaded Avatar has which are missing from this list, to this list
		/// </summary>
		public void UpdateColors(bool triggerDirty = false)
		{
			foreach (UMA.OverlayColorData ucd in umaData.umaRecipe.sharedColors)
			{
				if (ucd.HasName())
				{
					OverlayColorData c;
					if (characterColors.GetColor(ucd.name, out c))
					{
						ucd.color = c.color;
						if (ucd.channelAdditiveMask.Length == 3)
							ucd.channelAdditiveMask[2] = c.channelAdditiveMask[2];
					}
					else
					{
						characterColors.SetColor(ucd.name, ucd);
					}
				}
			}
			if (triggerDirty)
			{
				ForceUpdate(false, true, false);
			}
		}

		protected void UpdateUMA()
		{
			if (umaRace != umaData.umaRecipe.raceData)
			{
				UpdateNewRace();
			}
			else
			{
				UpdateSameRace();
			}
		}

		public void ForceUpdate(bool DnaDirty, bool TextureDirty = false, bool MeshDirty = false)
		{
			umaData.Dirty(DnaDirty, TextureDirty, MeshDirty);
		}

		/// <summary>
		/// Clears all the wardrobe slots of any wardrobeRecipes that have been set on the avatar
		/// </summary>
		public void ClearSlots()
		{
			WardrobeRecipes.Clear();
		}
		/// <summary>
		/// Clears the given wardrobe slot of any recipes that have been set on the Avatar
		/// </summary>
		/// <param name="ws"></param>
		public void ClearSlot(string ws)
		{
			if (WardrobeRecipes.ContainsKey(ws))
			{
				WardrobeRecipes.Remove(ws);
			}
		}
		/// <summary>
		/// Use when temporary wardrobe recipes have been used while the real ones have been downloading. Will replace the temp textrecipes with the downloaded ones.
		/// </summary>
		public void UpdateSetSlots(string recipeToUpdate = "")
		{
			string slotsInWardrobe = "";
			foreach (KeyValuePair<string, UMATextRecipe> kp in WardrobeRecipes)
			{
				slotsInWardrobe = slotsInWardrobe + " , " + kp.Key;
			}
			Dictionary<string, UMATextRecipe> newWardrobeRecipes = new Dictionary<string, UMATextRecipe>();
			var thisDCS = context.dynamicCharacterSystem as DynamicCharacterSystem;
			foreach (KeyValuePair<string, UMATextRecipe> kp in WardrobeRecipes)
			{
				if (thisDCS.GetRecipe(kp.Value.name, false) != null)
				{
					newWardrobeRecipes.Add(kp.Key, thisDCS.GetRecipe(kp.Value.name, false));
				}
				else
				{
					newWardrobeRecipes.Add(kp.Key, kp.Value);
				}
			}
			WardrobeRecipes = newWardrobeRecipes;
		}
		/// <summary>
		/// Sets the avatars wardrobe slot to use the given wardrobe recipe (not to be mistaken with an UMA SlotDataAsset)
		/// </summary>
		/// <param name="utr"></param>
		public void SetSlot(UMATextRecipe utr)
		{
			if (utr.wardrobeSlot != "" && utr.wardrobeSlot != "None")
			{
				if (WardrobeRecipes.ContainsKey(utr.wardrobeSlot))
				{
					WardrobeRecipes[utr.wardrobeSlot] = utr;
				}
				else
				{
					WardrobeRecipes.Add(utr.wardrobeSlot, utr);
				}
			}
		}

        /// <summary>
        /// Returns the name for any wardrobe item at that wardrobeslot.
        /// otherwise, returns an empty string;
        /// </summary>
        /// <param name="SlotName"></param>
        /// <returns></returns>
        public string GetWardrobeItemName(string SlotName)
        {
            if (WardrobeRecipes.ContainsKey(SlotName))
            {
                UMATextRecipe utr = WardrobeRecipes[SlotName];
                if (utr != null) return utr.name;
            }
            return "";
        }

        public UMATextRecipe FindSlotRecipe(string Slotname, string Recipename)
        {
            var recipes = AvailableRecipes;

            if (recipes.ContainsKey(Slotname) != true) return null;

            List<UMATextRecipe> SlotRecipes = recipes[Slotname];

            for(int i=0;i<recipes.Count;i++)
            {
                UMATextRecipe utr = SlotRecipes[i];
                if (utr.name == Recipename)
                    return utr;
            }
            return null;
        }

        public void SetSlot(string Slotname, string Recipename)
        {
            UMATextRecipe utr = FindSlotRecipe(Slotname, Recipename);
            if (!utr)
            {
                throw new Exception("Unable to find slot or recipe");
            }
            SetSlot(utr);
        }
		/// <summary>
		/// Change the race of the Avatar, optionally overriding the 'onChangeRace' settings in the avatar component itself
		/// </summary>
		/// <param name="racename"></param>
		/// <param name="keepDNA">If true will attempt to transfer the dna settings for the current race into the dna settings for the new race</param>
		/// <param name="keepWardrobe">If true will attempt to keep the wardrobe the avatar is wearing on the new race if any of it is compatible</param>
		/// <param name="keepBodyColors">If true will keep any colors that are set in the current baseRaceRecipe the same when switching to the new race</param>
		/// <param name="cacheCurrentState">If true will cache the current state of the avatar in this race so that when you switch back to this race it will retain its clothing and colors</param>
		public void ChangeRace(string racename, bool? keepDNA = null, bool? keepWardrobe = null, bool? keepBodyColors = null, bool? cacheCurrentState = null)
		{
			ChangeRace((context.raceLibrary as DynamicRaceLibrary).GetRace(racename), keepDNA, keepWardrobe, keepBodyColors, cacheCurrentState);
		}
		/// <summary>
		/// Change the race of the Avatar, optionally overriding the 'onChangeRace' settings in the avatar component itself
		/// </summary>
		/// <param name="race"></param>
		/// <param name="keepDNA">If true will attempt to transfer the dna settings for the current race into the dna settings for the new race</param>
		/// <param name="keepWardrobe">If true will attempt to keep the wardrobe the avatar is wearing on the new race if any of it is compatible</param>
		/// <param name="keepBodyColors">If true will keep any colors that are set in the current baseRaceRecipe the same when switching to the new race</param>
		/// <param name="cacheCurrentState">If true will cache the current state of the avatar in this race so that when you switch back to this race it will retain its clothing and colors</param>
		public void ChangeRace(RaceData race, bool? keepDNA = null, bool? keepWardrobe = null, bool? keepBodyColors = null, bool? cacheCurrentState = null)
		{
			bool actuallyChangeRace = false;
			if (activeRace.racedata == null)
				actuallyChangeRace = true;
			else if (activeRace.name != race.raceName)
				actuallyChangeRace = true;
			if(actuallyChangeRace)
				StartCoroutine(ChangeRaceCoroutine(race, keepDNA, keepWardrobe, keepBodyColors, cacheCurrentState));
		}

		private IEnumerator ChangeRaceCoroutine(RaceData race, bool? keepDNA, bool? keepWardrobe, bool? keepBodyColors, bool? cacheCurrentState)
		{
			keepDNA = keepDNA == null ? activeRace.keepDNA : keepDNA;
			keepWardrobe = keepWardrobe == null ? activeRace.keepWardrobe : keepWardrobe;
			keepBodyColors = keepBodyColors == null ? activeRace.keepBodyColors : keepBodyColors;
			cacheCurrentState = cacheCurrentState == null ? activeRace.cacheCurrentState : cacheCurrentState;
			if (Application.isPlaying)
			{
				if (cacheCurrentState == true)
				{
					if (!cacheStates.ContainsKey(activeRace.name))
					{
						var tempCols = new List<ColorValue>();
						foreach (ColorValue col in characterColors.Colors)
						{
							tempCols.Add(new ColorValue(col));
						}
						var thisCacheStateInfo = new CacheStateInfo(new Dictionary<string, UMATextRecipe>(WardrobeRecipes), tempCols);
						cacheStates.Add(activeRace.name, thisCacheStateInfo);
					}
					else
					{
						cacheStates[activeRace.name].wardrobeCache = new Dictionary<string, UMATextRecipe>(WardrobeRecipes);
						var tempCols = new List<ColorValue>();
						foreach (ColorValue col in characterColors.Colors)
						{
							tempCols.Add(new ColorValue(col));
						}
						cacheStates[activeRace.name].colorCache = tempCols;
					}
				}
				if (cacheStates.ContainsKey(race.raceName))
				{
					activeRace.name = race.raceName;
					activeRace.data = race;
					umaRecipe = race.baseRaceRecipe;
					//Keep wardrobe will try and keep the wardrobe the previous race had if any of it is compatible
					//if cacheStates is on and keep wardrobe is false it will attempt to load the last wardrobe this avatar had the last time it was this race
					if (keepWardrobe == false)
					{
						WardrobeRecipes = new Dictionary<string, UMATextRecipe>(cacheStates[race.raceName].wardrobeCache);
						//if the WardrobeRecipes count == 0 and preloadWardrobePrecipes = true then the Avatar was first created with a 'old' recipe that did not have wardrobe data
						if (WardrobeRecipes.Count == 0 && preloadWardrobeRecipes.loadDefaultRecipes == true)
						{
							LoadDefaultWardrobe(true);
						}
					}
					if (keepBodyColors == false)
					{
						characterColors.Colors.Clear();
						characterColors.Colors = new List<ColorValue>(cacheStates[race.raceName].colorCache);
					}
					var prevDna1 = new UMADnaBase[0];
					if (keepDNA == true)
					{
						prevDna1 = umaData.umaRecipe.GetAllDna();
					}
					BuildCharacter(false);
					if (keepDNA == true)
					{
						TryImportDNAValues(prevDna1);
					}
					yield break;
				}
			}
			var batchID = DynamicAssetLoader.Instance.GenerateBatchID();
			activeRace.name = race.raceName;
			activeRace.data = race;
			if (DynamicAssetLoader.Instance.downloadingAssetsContains(race.raceName))
			{
				requiredAssetsToCheck.Add(race.raceName);
			}
			if (Application.isPlaying)
			{
				var prevDna = new UMADnaBase[0];
				if (keepDNA == true)
				{
					prevDna = umaData.umaRecipe.GetAllDna();
				}
				umaRecipe = activeRace.racedata.baseRaceRecipe;
				//if the WardrobeRecipes count == 0 and preloadWardrobePrecipes = true then the Avatar was first created with a 'old' recipe that did not have wardrobe data
				if (keepWardrobe == false || (WardrobeRecipes.Count == 0 && preloadWardrobeRecipes.loadDefaultRecipes == true))
				{
					ClearSlots();
					DynamicAssetLoader.Instance.CurrentBatchID = batchID;
					LoadDefaultWardrobe(true);//load defaultWardrobe will add anything it downloads to requiredAssetsToCheck
				}
				if(!loadOptions.waitForBundles && requiredAssetsToCheck.Count > 0)
				{
					BuildCharacter(false);//if we are not waiting for bundles before building, build the placeholder avatar now
				}
				while (DynamicAssetLoader.Instance.downloadingAssetsContains(requiredAssetsToCheck))
				{
					yield return null;
				}
				requiredAssetsToCheck.Clear();
				activeRace.data = context.raceLibrary.GetRace(activeRace.name);
				umaRecipe = activeRace.data.baseRaceRecipe;
				if(keepWardrobe == false)
					UpdateSetSlots();

				//if we are not keeping the body colors that have been set we reset them to the colors set in the avatar component itself
				if (keepBodyColors == false)
				{
                    characterColors.Colors.Clear();
					characterColors.Colors = new List<ColorValue>(cacheStates["NULL"].colorCache);
				}

				BuildCharacter(false);

				if (keepDNA == true)
				{
					TryImportDNAValues(prevDna);
				}
			}
			yield break;
		}

		private void TryImportDNAValues(UMADnaBase[] prevDna)
		{
			Debug.Log("Tried to import previous DNA values");
			umaData.umaRecipe.ApplyDNA(umaData, true);
			//DynamicDNAConverterBehaviourBase.FixUpUMADnaToDynamicUMADna(umaData.umaRecipe);//curiously this on its own doesn't work
			var activeDNA = umaData.umaRecipe.GetAllDna();
			for (int i = 0; i < activeDNA.Length; i++)
			{
				if (activeDNA[i].GetType().ToString().IndexOf("DynamicUMADna") > -1)
				{
					//iterate over each dna in prev dna and try to apply its values to this dna
					foreach (UMADnaBase dna in prevDna)
					{
						((DynamicUMADnaBase)activeDNA[i]).ImportUMADnaValues(dna);
					}
				}
			}
		}

#region LoadSaveFunctions

		/// <summary>
		/// Returns the UMATextRecipe string. This includes data about the current wardrobe for the character but is also backwards compatible with Non-DynamicCharacterAvatarUMAs.
		/// </summary>
		/// <returns></returns>
		public string GetCurrentRecipe()
		{
			// save 
			UMATextRecipe u = new UMATextRecipe();
			u.SaveCharacterSystem(umaData.umaRecipe, context, WardrobeRecipes);
			return u.recipeString;
		}

        public void Preload(string Recipe)
        {
            loadString = Recipe;
            loadPathType = loadPathTypes.String;
            loadFileOnStart = true;
        }

		/// <summary>
		/// Loads the Avatar from the given recipe and additional recipe. 
		/// Has additional functions for removing any slots that should be hidden by any 'wardrobe Recipes' that are in the additional recipes array.
		/// </summary>
		/// <param name="umaRecipe"></param>
		/// <param name="umaAdditionalRecipes"></param>
		public override void Load(UMARecipeBase umaRecipe, params UMARecipeBase[] umaAdditionalSerializedRecipes)
		{
			if (umaRecipe == null)
			{
				return;
			}
			if (umaData == null)
			{
				Initialize();
			}
			//set the umaData.animator if we have an animator already
			if (this.gameObject.GetComponent<Animator>())
			{
				umaData.animator = this.gameObject.GetComponent<Animator>();
			}
			Profiler.BeginSample("Load");

			this.umaRecipe = umaRecipe;

			umaRecipe.Load(umaData.umaRecipe, context);

			/*if (flagForReload)
            {
                activeRace.data = umaData.umaRecipe.raceData;
                activeRace.name = activeRace.racedata.raceName;
                SetAnimatorController();
                umaAdditionalSerializedRecipes = BuildCharacter();
            }*/

			umaData.AddAdditionalRecipes(umaAdditionalRecipes, context);
			AddAdditionalSerializedRecipes(umaAdditionalSerializedRecipes);

			RemoveHiddenSlots();
			UpdateColors();

            //New event that allows for tweaking the resulting recipe before the character is actually generated
            RecipeUpdated.Invoke(umaData);

            if (umaRace != umaData.umaRecipe.raceData)
			{
				UpdateNewRace();
			}
			else
			{
				UpdateSameRace();
			}
			Profiler.EndSample();

			StartCoroutine(UpdateAssetBundlesUsedbyCharacter());
		}

		public void AddAdditionalSerializedRecipes(UMARecipeBase[] umaAdditionalSerializedRecipes)
		{
			if (umaAdditionalSerializedRecipes != null)
			{
				foreach (var umaAdditionalRecipe in umaAdditionalSerializedRecipes)
				{
					UMAData.UMARecipe cachedRecipe = umaAdditionalRecipe.GetCachedRecipe(context);
					umaData.umaRecipe.Merge(cachedRecipe, false);
				}
			}
		}

		/// <summary>
		/// Load a DynamicCharacterAvatar from an UMATextRecipe Asset, optionally waiting for any assets that will need to be downloaded (according to the CharacterAvatar 'loadOptions.waitForBundles' setting) and optionally overriding the 'LoadOptions' settings for loading the Race/Dna/Wardrobe etc for the recipe
		/// </summary>
		/// <param name="umaRecipeToLoad"></param>
		/// <param name="loadRace"></param>
		/// <param name="loadDNA"></param>
		/// <param name="loadWardrobe"></param>
		/// <param name="loadBodyColors">if true will load any colors from the recipe that were also found the the baseRaceRecipe's Shared colors</param>
		/// <param name="loadWardrobeColors"></param>
		public void LoadFromRecipe(UMARecipeBase umaRecipeToLoad, bool? loadRace = null, bool? loadDNA = null, bool? loadWardrobe = null, bool? loadBodyColors = null, bool? loadWardrobeColors = null)
		{
			umaRecipe = umaRecipeToLoad;
			StartCoroutine(LoadFromRecipeCO(loadRace,loadDNA, loadWardrobe, loadBodyColors, loadWardrobeColors));
		}

		IEnumerator LoadFromRecipeCO(bool? loadRace = null, bool? loadDNA = null, bool? loadWardrobe = null, bool? loadBodyColors = null, bool? loadWardrobeColors = null)
		{
			while (!DynamicAssetLoader.Instance.isInitialized)
			{
				yield return null;
			}
			LoadFromRecipeString((umaRecipe as UMATextRecipe).recipeString, loadRace,loadDNA, loadWardrobe, loadBodyColors, loadWardrobeColors);
			yield break;
		}

		/// <summary>
		/// Loads an avatar from a recipe string, optionally waiting for any assets that will need to be downloaded (according to the CharacterAvatar 'loadOptions.waitForBundles' setting) and optionally overriding the 'LoadOptions' settings for loading the Race/Dna/Wardrobe etc for the recipe
		/// This cannot be called before initialization. 
		/// Use Preload when calling before Initialization.
		/// </summary>
		/// <param name="recipeString"></param>
		/// <returns></returns>
		public void LoadFromRecipeString(string recipeText, bool? loadRace = null, bool? loadDNA = null, bool? loadWardrobe = null, bool? loadBodyColors = null, bool? loadWardrobeColors = null)
		{
			StartCoroutine(LoadFromRecipeStringCO(recipeText, loadRace, loadDNA, loadWardrobe, loadBodyColors, loadWardrobeColors));
		}

		private IEnumerator LoadFromRecipeStringCO(string recipeString, bool? loadRace = null, bool? loadDNA = null, bool? loadWardrobe = null, bool? loadBodyColors = null, bool? loadWardrobeColors = null)
		{
			loadRace = loadRace == null ? loadOptions.loadRace : loadRace;
			loadDNA = loadDNA == null ? loadOptions.loadDNA : loadDNA;
			loadWardrobe = loadWardrobe == null ? loadOptions.loadWardrobe : loadWardrobe;
			loadBodyColors = loadBodyColors == null ? loadOptions.loadBodyColors : loadBodyColors;
			loadWardrobeColors = loadWardrobeColors == null ? loadOptions.loadWardrobeColors : loadWardrobeColors;

            if (umaGenerator == null)
			{
				umaGenerator = UMAGenerator.FindInstance();
			}
            if (umaData == null)
            {
                Initialize();
            }
			var umaTextRecipe = ScriptableObject.CreateInstance<UMATextRecipe>();
			umaTextRecipe.name = loadFilename;
			umaTextRecipe.recipeString = recipeString;
			var batchID = DynamicAssetLoader.Instance.GenerateBatchID();
			UMADataCharacterSystem.UMACharacterSystemRecipe tempRecipe = new UMADataCharacterSystem.UMACharacterSystemRecipe();
			var prevDna = new UMADnaBase[0];
			//Before we actually call Load we need to know the race so that any wardrobe recipes can be assigned to it in CharacterSystem.
			Regex raceRegex = new Regex("\"([^\"]*)\"", RegexOptions.Multiline);
			var raceMatches = raceRegex.Match(recipeString.Replace(Environment.NewLine, ""), (recipeString.IndexOf("\"race\":") + 6));
			string raceString = raceMatches.Groups[1].ToString().Replace("\"", "");
			
			if (raceString != "")
			{
				context.GetRace(raceString);//update the race library with this race- triggers the race to download and sets it to placeholder race if its in an assetbundle
			}
			//If the user doesn't have the content and it cannot be downloaded then we get an error. So try-catch...
			try
			{
				//Unpacking the recipe will kick off the downloading of any assets we dont already have if they are available in any assetbundles, 
				umaTextRecipe.LoadCharacterSystem((UMA.UMADataCharacterSystem.UMACharacterSystemRecipe)tempRecipe, context);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[CharacterAvatar.LoadFromRecipeString] Exception: " + e);
				yield break;
			}
            if (tempRecipe.raceData == null)
            {
                Debug.LogError("Race "+raceString+" not found in RaceLibrary");
                yield break;
            }
			if (loadDNA == false && activeRace.racedata != null)
			{
				prevDna = umaData.umaRecipe.GetAllDna();
			}
			if (loadRace == true)
			{
				activeRace.name = tempRecipe.raceData.raceName;
				activeRace.data = tempRecipe.raceData;
				if (loadOptions.waitForBundles == false)
				{
					if (DynamicAssetLoader.Instance.downloadingAssetsContains(activeRace.name)){
						requiredAssetsToCheck.Add(activeRace.name);
					}
                }
				DynamicAssetLoader.Instance.CurrentBatchID = batchID;
				SetStartingRace();
				//If the UmaRecipe is still null, bail - we cant go any further (and SetStartingRace will have shown an error)
				if (umaRecipe == null)
				{
					yield break;
				}
			}
			if (recipeString.IndexOf("wardrobeRecipesJson") != -1)//means we have a characterSystemTextRecipe
			{			
				if (loadWardrobe == true)
				{
					DynamicAssetLoader.Instance.CurrentBatchID = batchID;
					ClearSlots();
					if (tempRecipe.wardrobeRecipes.Count > 0)
					{
						var thisDCS = context.dynamicCharacterSystem as DynamicCharacterSystem;
						foreach (KeyValuePair<string, string> kp in tempRecipe.wardrobeRecipes)
						{
							//by using GetRecipe CharacterSystem will retrieve it from an asset bundle if it needs too, 
							if (thisDCS.GetRecipe(kp.Value) != null)
							{
								UMATextRecipe utr = thisDCS.GetRecipe(kp.Value);
								SetSlot(utr);
								if (DynamicAssetLoader.Instance.downloadingAssetsContains(utr.name))
								{
									requiredAssetsToCheck.Add(utr.name);
								}
							}
						}
					}
				}
				if (loadOptions.waitForBundles == false)
				{
					BuildCharacter(false);//if we are not waiting for asset bundles we can build the placeholder avatar
				}
				//before we can serarate the body colors from everything else we must have the actual base recipe downloaded
				while (DynamicAssetLoader.Instance.downloadingAssetsContains(requiredAssetsToCheck))
				{
					yield return null;
				}
				requiredAssetsToCheck.Clear();
				if (loadRace == true)
				{
					activeRace.data = context.raceLibrary.GetRace(activeRace.name);
					umaRecipe = activeRace.data.baseRaceRecipe;
				}
				UpdateSetSlots();
				//the idea here is that any shared colors defined in the baseRaceRecipe are considered 'body colors'
				//so if you wanted 'Hair' to be considered a 'bodyColor' you would define a shared color for 'Hair' in the baseRaceRecipe (even if you dont define any hair)
				//then we will get these names and see if the incoming recipe contains any shared colors with those names
				List<string> bodyColorNames = new List<string>();	
				List<OverlayColorData> newSharedColors = new List<OverlayColorData>();
				if(umaData.umaRecipe.sharedColors != null)
				{
					newSharedColors.AddRange(umaData.umaRecipe.sharedColors);
				}
				//get the body colors if we can
				UMADataCharacterSystem.UMACharacterSystemRecipe baseRaceRecipeTemp = new UMADataCharacterSystem.UMACharacterSystemRecipe();
				activeRace.data.baseRaceRecipe.Load(baseRaceRecipeTemp, context);
                foreach (OverlayColorData col in baseRaceRecipeTemp.sharedColors)
				{
					bodyColorNames.Add(col.name);
				}
				baseRaceRecipeTemp = null;
				if (loadBodyColors == true)
				{				
					foreach(OverlayColorData col in tempRecipe.sharedColors)
					{
						if (bodyColorNames.Contains(col.name))
						{
							characterColors.SetColor(col.name, col);
							if(!newSharedColors.Contains(col))
								newSharedColors.Add(col);
                        }
					}
				}
				//then here we will apply any colors that are not named in the 'bodyColors' list if loadWardrobeColors is true
				if (loadWardrobeColors == true)
				{
					foreach (OverlayColorData col in tempRecipe.sharedColors)
					{
						if (!bodyColorNames.Contains(col.name))
						{
							characterColors.SetColor(col.name, col);
							if (!newSharedColors.Contains(col))
								newSharedColors.Add(col);
						}
					}
				}
				umaData.umaRecipe.sharedColors = newSharedColors.ToArray();
				//
				BuildCharacter(false);
				if (loadDNA == true)
				{
					umaData.umaRecipe.ClearDna();
					foreach (UMADnaBase dna in tempRecipe.GetAllDna())
					{
						umaData.umaRecipe.AddDna(dna);
					}
				}
				else if(prevDna.Length > 0)
				{
					TryImportDNAValues(prevDna);
                }
			}
			else
			{
				Debug.Log("Recipe was not DCS- performing standard load");
				//old style recipes may still have had assets in an asset bundle so...
				if (loadOptions.waitForBundles == false)
				{
					BuildCharacter(false);
				}
				while (DynamicAssetLoader.Instance.downloadingAssetsContains(requiredAssetsToCheck))
				{
					yield return null;
				}
				requiredAssetsToCheck.Clear();
				if (loadRace == true)
				{
					activeRace.data = context.raceLibrary.GetRace(activeRace.name);
					umaRecipe = activeRace.data.baseRaceRecipe;
				}
				ClearSlots();
				//if its a standard UmaTextRecipe load it directly into UMAData since there wont be any wardrobe slots...
				//BUT we do need to add the 'additional recipes'
				umaData.umaRecipe.sharedColors = tempRecipe.sharedColors;
				umaTextRecipe.Load(umaData.umaRecipe, context);
				umaData.AddAdditionalRecipes(umaAdditionalRecipes, context);
				umaData.umaRecipe.sharedColors = tempRecipe.sharedColors;
				characterColors.Colors = new List<ColorValue>();
				foreach (OverlayColorData col in umaData.umaRecipe.sharedColors)
				{
					characterColors.Colors.Add(new ColorValue(col.name, col.color));
				}
				SetAnimatorController();
				SetExpressionSet();
				UpdateColors();
				if (loadDNA == true)
				{
					umaData.umaRecipe.ClearDna();
					foreach (UMADnaBase dna in tempRecipe.GetAllDna())
					{
						umaData.umaRecipe.AddDna(dna);
					}
				}
				else if (prevDna.Length > 0)
				{
					TryImportDNAValues(prevDna);
				}
				if (umaRace != umaData.umaRecipe.raceData)
				{
					UpdateNewRace();
				}
				else
				{
					UpdateSameRace();
				}
				StartCoroutine(UpdateAssetBundlesUsedbyCharacter());
			}
			tempRecipe = null;
			Destroy(umaTextRecipe);
			yield break;//never sure if we need to do this?
		}
		/// <summary>
		/// Checks what assetBundles (if any) were used in the creation of this Avatar. NOTE: Query this UMA's AssetBundlesUsedbyCharacterUpToDate field before calling this function
		/// </summary>
		/// <param name="verbose">set this to true to get more information to track down when asset bundles are getting dependencies when they shouldn't because they are refrencing things from asset bundles you did not intend them to</param>
		/// <returns></returns>
		//DOS NOTES: The fact this is an Enumerator may cause issues for any script wanting to get upToDate data- on the otherhand we dont want to slow down rendering by making it wait for this list
		IEnumerator UpdateAssetBundlesUsedbyCharacter(bool verbose = false)
		{
			AssetBundlesUsedbyCharacterUpToDate = false;
			yield return null;
			assetBundlesUsedbyCharacter.Clear();
			if (umaData != null)
			{
				var raceLibraryDict = ((DynamicRaceLibrary)context.raceLibrary as DynamicRaceLibrary).assetBundlesUsedDict;
				var slotLibraryDict = ((DynamicSlotLibrary)context.slotLibrary as DynamicSlotLibrary).assetBundlesUsedDict;
				var overlayLibraryDict = ((DynamicOverlayLibrary)context.overlayLibrary as DynamicOverlayLibrary).assetBundlesUsedDict;
				var characterSystemDict = ((DynamicCharacterSystem)context.dynamicCharacterSystem as DynamicCharacterSystem).assetBundlesUsedDict;
				var raceAnimatorsDict = raceAnimationControllers.assetBundlesUsedDict;
				if (raceLibraryDict.Count > 0)
				{
					foreach (KeyValuePair<string, List<string>> kp in raceLibraryDict)
					{
						if (!assetBundlesUsedbyCharacter.Contains(kp.Key))
							if (kp.Value.Contains(activeRace.name))
							{
								assetBundlesUsedbyCharacter.Add(kp.Key);
							}
					}
				}
				var activeSlots = umaData.umaRecipe.GetAllSlots();
				if (slotLibraryDict.Count > 0)
				{
					foreach (SlotData slot in activeSlots)
					{
						if (slot != null)
						{
							foreach (KeyValuePair<string, List<string>> kp in slotLibraryDict)
							{
								if (!assetBundlesUsedbyCharacter.Contains(kp.Key))
									if (kp.Value.Contains(slot.asset.name))
									{
										if (verbose)
											assetBundlesUsedbyCharacter.Add(kp.Key + " (Slot:" + slot.asset.name + ")");
										else
											assetBundlesUsedbyCharacter.Add(kp.Key);
									}
							}
						}
					}
				}
				if (overlayLibraryDict.Count > 0)
				{
					foreach (SlotData slot in activeSlots)
					{
						if (slot != null)
						{
							var overLaysinSlot = slot.GetOverlayList();
							foreach (OverlayData overlay in overLaysinSlot)
							{
								foreach (KeyValuePair<string, List<string>> kp in overlayLibraryDict)
								{
									if (!assetBundlesUsedbyCharacter.Contains(kp.Key))
										if (kp.Value.Contains(overlay.asset.name))
										{
											if (verbose)
												assetBundlesUsedbyCharacter.Add(kp.Key + " (Overlay:" + overlay.asset.name + ")");
											else
												assetBundlesUsedbyCharacter.Add(kp.Key);
										}
								}
							}
						}
					}
				}
				if (characterSystemDict.Count > 0)
				{
					foreach (KeyValuePair<string, UMATextRecipe> recipe in WardrobeRecipes)
					{
						foreach (KeyValuePair<string, List<string>> kp in characterSystemDict)
						{
							if (!assetBundlesUsedbyCharacter.Contains(kp.Key))
								if (kp.Value.Contains(recipe.Key))
								{
									assetBundlesUsedbyCharacter.Add(kp.Key);
								}
						}
					}
				}
				string specificRaceAnimator = "";
				foreach (RaceAnimator raceAnimator in raceAnimationControllers.animators)
				{
					if (raceAnimator.raceName == activeRace.name && raceAnimator.animatorController != null)
					{
						specificRaceAnimator = raceAnimator.animatorControllerName;
						break;
					}
				}
				if (raceAnimatorsDict.Count > 0 && specificRaceAnimator != "")
				{
					foreach (KeyValuePair<string, List<string>> kp in raceAnimatorsDict)
					{
						if (!assetBundlesUsedbyCharacter.Contains(kp.Key))
							if (kp.Value.Contains(specificRaceAnimator))
							{
								assetBundlesUsedbyCharacter.Add(kp.Key);
							}
					}
				}
			}
			AssetBundlesUsedbyCharacterUpToDate = true;
			yield break;
		}
		/// <summary>
		/// Returns the list of AssetBundles used by the avatar. IMPORTANT you must wait for AssetBundlesUsedbyCharacterUpToDate to be true before calling this method.
		/// </summary>
		/// <returns></returns>
		public List<string> GetAssetBundlesUsedByCharacter()
		{
			return assetBundlesUsedbyCharacter;
		}

		/// <summary>
		/// Loads the text file in the loadFilename field to get its recipe string, and then calls LoadFromRecipeString to to process the recipe and load the Avatar.
		/// </summary>
		public void DoLoad()
		{
			StartCoroutine(DoLoadCoroutine());
		}
		IEnumerator DoLoadCoroutine()
		{
			yield return null;
			string path = "";
			string recipeString = "";

            if (!string.IsNullOrEmpty(loadString)&&loadPathType == loadPathTypes.String)
            {
                recipeString = loadString;
            }
#if UNITY_EDITOR
			if (loadFilename == "" && Application.isEditor && loadPathType != loadPathTypes.String)
			{
				loadPathType = loadPathTypes.FileSystem;
			}
#endif
			var thisDCS = context.dynamicCharacterSystem as DynamicCharacterSystem;
			if (loadPathType == loadPathTypes.CharacterSystem)
			{
				if (thisDCS.CharacterRecipes.ContainsKey(loadFilename.Trim()))
				{
					thisDCS.CharacterRecipes.TryGetValue(loadFilename.Trim(), out recipeString);
				}
			}
			if (loadPathType == loadPathTypes.FileSystem)
			{
#if UNITY_EDITOR
				if (Application.isEditor)
				{
					path = EditorUtility.OpenFilePanel("Load saved Avatar", Application.dataPath, "txt");
					if (string.IsNullOrEmpty(path)) yield break;
					recipeString = FileUtils.ReadAllText(path);
					path = "";
				}
				else
#endif
					loadPathType = loadPathTypes.persistentDataPath;
			}
			if (loadPathType == loadPathTypes.persistentDataPath)
			{
				path = Application.persistentDataPath;
			}
			else if (loadPathType == loadPathTypes.streamingAssetsPath)
			{
				path = Application.streamingAssetsPath;
			}
			if (path != "" || loadPathType == loadPathTypes.Resources)
			{
				if (loadPathType == loadPathTypes.Resources)
				{
					TextAsset[] textFiles = Resources.LoadAll<TextAsset>(loadPath);
					for (int i = 0; i < textFiles.Length; i++)
					{
						if (textFiles[i].name == loadFilename.Trim() || textFiles[i].name.ToLower() == loadFilename.Trim())
						{
							recipeString = textFiles[i].text;
						}
					}
				}
				else
				{
					path = (loadPath != "") ? System.IO.Path.Combine(path, loadPath.TrimStart('\\', '/').TrimEnd('\\', '/').Trim()) : path;
					if (loadFilename == "")
					{
						Debug.LogWarning("[CharacterAvatar.DoLoad] No filename specified to load!");
						yield break;
					}
					else
					{
						if (path.Contains("://"))
						{
							WWW www = new WWW(path + loadFilename);
							yield return www;
							recipeString = www.text;
						}
						else
						{
							recipeString = FileUtils.ReadAllText(System.IO.Path.Combine(path, loadFilename));
						}
					}
				}
			}
			if (recipeString != "")
			{
				LoadFromRecipeString(recipeString);
				yield break;
			}
			else
			{
				Debug.LogWarning("[CharacterAvatar.DoLoad] No TextRecipe found with filename " + loadFilename);
			}
			yield break;
		}

		/// <summary>
		/// Saves the current avatar and its wardrobe slots and colors to a UMATextRecipe (text) compatible recipe. Use this instead of GetCurrentRecipe if you want to create a file that includes wardrobe recipes for use with another CharacterAvatar
		/// </summary>
		public void DoSave()
		{
			StartCoroutine(DoSaveCoroutine());
		}
		IEnumerator DoSaveCoroutine()
		{
			yield return null;
			string path = "";
			string filePath = "";
#if UNITY_EDITOR
			if (saveFilename == "" && Application.isEditor)
			{
				savePathType = savePathTypes.FileSystem;
			}
#endif
			if (savePathType == savePathTypes.FileSystem)
			{
#if UNITY_EDITOR
				if (Application.isEditor)
				{
					path = EditorUtility.SaveFilePanel("Save Avatar", Application.dataPath, (saveFilename != "" ? saveFilename : ""), "txt");

				}
				else
#endif
					savePathType = savePathTypes.persistentDataPath;

			}
			//I dont think we can save anywhere but persistentDataPath on most platforms
			if ((savePathType == savePathTypes.Resources || savePathType == savePathTypes.streamingAssetsPath))
			{
#if UNITY_EDITOR
				if (!Application.isEditor)
#endif
					savePathType = savePathTypes.persistentDataPath;
			}
			if (savePathType == savePathTypes.Resources)
			{
				path = System.IO.Path.Combine(Application.dataPath, "Resources");//This needs to be exactly the right folder to work in the editor
			}
			else if (savePathType == savePathTypes.streamingAssetsPath)
			{
				path = Application.streamingAssetsPath;
			}
			else if (savePathType == savePathTypes.persistentDataPath)
			{
				path = Application.persistentDataPath;
			}
			if (path != "")
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				if (makeUnique || saveFilename == "")
				{
					saveFilename = saveFilename + Guid.NewGuid().ToString();
				}
				if (savePathType != savePathTypes.FileSystem)
				{
					path = (savePath != "") ? System.IO.Path.Combine(path, savePath.TrimStart('\\', '/').TrimEnd('\\', '/').Trim()) : path;
					filePath = System.IO.Path.Combine(path, saveFilename + ".txt");
					FileUtils.EnsurePath(path);
				}
				else
				{
					filePath = path;
				}
				var asset = ScriptableObject.CreateInstance<UMATextRecipe>();
				asset.SaveCharacterSystem(umaData.umaRecipe, context, WardrobeRecipes);
				FileUtils.WriteAllText(filePath, asset.recipeString);
				if (savePathType == savePathTypes.Resources || savePathType == savePathTypes.streamingAssetsPath)
				{
#if UNITY_EDITOR
					AssetDatabase.Refresh();
#endif
				}
				ScriptableObject.Destroy(asset);
			}
			else
			{
				Debug.LogError("CharacterSystem Save Error! Could not save file, check you have set the filename and path correctly...");
				yield break;
			}
			yield break;
		}

#endregion
		public void AvatarCreated()
		{
			SkinnedMeshRenderer smr = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			smr.localBounds = new Bounds(smr.localBounds.center + BoundsOffset, smr.localBounds.size);
		}

        /// <summary>
        /// get all of the DNA for the current character, and return it as a list of DnaSetters.
        /// Each DnaSetter will track the DNABase that it came from, and the character that it is attached
        /// to. To modify the DNA on the character, use the Set function on the Setter.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,DnaSetter> GetDNA()
        {
            Dictionary<string,DnaSetter> dna = new Dictionary<string,DnaSetter>();

            foreach (UMADnaBase db in umaData.GetAllDna())
            {
                for (int i=0;i<db.Count;i++)
                {
                    dna.Add(db.Names[i], new DnaSetter(db.Names[i], db.Values[i], i, db));
                }    
            }
            return dna;
        }

	    public UMADnaBase[] GetAllDNA()
		{
			return umaData.GetAllDna();
		} 
	
#region special classes

		[Serializable]
		public class RaceSetter
		{
			public string name;
			[SerializeField]
			RaceData _data;//This was not properly reporting itself as 'missing' when it is set to an asset that is in an asset bundle, so now data is a property that validates itself
			[SerializeField]//Needs to be serialized for the inspector but otherwise no- TODO what happens in a build? will this get saved across sessions- because we dont want that
			RaceData[] _cachedRaceDatas;

			public bool keepDNA = false;
			public bool keepWardrobe = false;
			public bool keepBodyColors = true;
			public bool cacheCurrentState = true;

			/// <summary>
			/// Will return the current active racedata- if it is in an asset bundle or in resources it will find/download it. If you ony need to know if the data is there use the racedata field instead.
			/// </summary>
			//DOS NOTES this is because we have decided to make the libraries get stuff they dont have and return a temporary asset- but there are still occasions where we dont want this to happen
			// or at least there are occasions where we JUST WANT THE LIST and dont want it do do anything else...
			public RaceData data
			{
				get
				{
					if (Application.isPlaying)
						return Validate();
					else
						return _data;
				}
				set
				{
					_data = value;
					/*if (Application.isPlaying)
                        Validate();*/
				}
			}
			/// <summary>
			/// returns the active raceData (quick)
			/// </summary>
			public RaceData racedata
			{
				get { return _data; }
			}

			RaceData Validate()
			{
				RaceData racedata = null;
				var thisContext = UMAContext.FindInstance();
				var thisDynamicRaceLibrary = (DynamicRaceLibrary)thisContext.raceLibrary as DynamicRaceLibrary;
				_cachedRaceDatas = thisDynamicRaceLibrary.GetAllRaces();
				//Debug.LogWarning("[RaceSetter] called Validate()");
				foreach (RaceData race in _cachedRaceDatas)
				{
					if (race.raceName == this.name)
						racedata = race;
				}
				return racedata;
			}
		}

		[Serializable]
		public class WardrobeRecipeListItem
		{
			public string _recipeName;
			public UMATextRecipe _recipe;
			//store compatible races here because when a recipe is not downloaded we dont have access to this info...
			public List<string> _compatibleRaces;

			public WardrobeRecipeListItem()
			{

			}
			public WardrobeRecipeListItem(string recipeName)//TODO: Does this constructor ever get used? We dont want it to...
			{
				_recipeName = recipeName;
			}
			public WardrobeRecipeListItem(UMATextRecipe recipe)
			{
				_recipeName = recipe.name;
				_recipe = recipe;
				_compatibleRaces = recipe.compatibleRaces;
			}
		}

		[Serializable]
		public class WardrobeRecipeList
		{
			[Tooltip("If this is checked and the Avatar is NOT creating itself from a previously saved recipe, recipes in here will be added to the Avatar when it loads")]
			public bool loadDefaultRecipes = true;
			public List<WardrobeRecipeListItem> recipes = new List<WardrobeRecipeListItem>();
			public List<WardrobeRecipeListItem> Validate(/*DynamicCharacterSystem characterSystem,*/ bool allowDownloadables = false, string raceName = "")
			{
				List<WardrobeRecipeListItem> validRecipes = new List<WardrobeRecipeListItem>();
				var thisDCS = UMAContext.Instance.dynamicCharacterSystem as DynamicCharacterSystem;
				if (thisDCS != null)
				{
					foreach (WardrobeRecipeListItem recipe in recipes)
					{
						if (allowDownloadables && (raceName == "" || recipe._compatibleRaces.Contains(raceName)))
						{
							if (thisDCS.GetRecipe(recipe._recipeName, true) != null)
							{
								recipe._recipe = thisDCS.GetRecipe(recipe._recipeName);
								validRecipes.Add(recipe);
							}

						}
						else
						{
							if (thisDCS.RecipeIndex.ContainsKey(recipe._recipeName))
							{
								bool recipeFound = false;
								recipeFound = thisDCS.RecipeIndex.TryGetValue(recipe._recipeName, out recipe._recipe);
								if (recipeFound)
								{
									recipe._compatibleRaces = recipe._recipe.compatibleRaces;
									validRecipes.Add(recipe);
								}

							}
						}
					}
				}
				else
				{
					Debug.LogWarning("There was no DynamicCharacterSystem set up in UMAContext");
				}
				return validRecipes;
			}
		}
		[Serializable]
		public class RaceAnimator
		{
			public string raceName;
			public string animatorControllerName;
			public RuntimeAnimatorController animatorController;
		}

		[Serializable]
		public class RaceAnimatorList
		{
			public RuntimeAnimatorController defaultAnimationController;
			public List<RaceAnimator> animators = new List<RaceAnimator>();
			public bool dynamicallyAddFromResources;
			public string resourcesFolderPath;
			public bool dynamicallyAddFromAssetBundles;
			public string assetBundleNames;
			public Dictionary<string, List<string>> assetBundlesUsedDict = new Dictionary<string, List<string>>();

			public List<string> Validate()
			{
				UpdateAnimators();
				int validAnimators = 0;
				List<string> validAnimatorsList = new List<string>();
				foreach (RaceAnimator animator in animators)
				{
					if (animator.animatorController != null)
					{
						validAnimators++;
						validAnimatorsList.Add(animator.animatorControllerName);
					}
				}
				return validAnimatorsList;
			}
			public void UpdateAnimators()
			{
				foreach (RaceAnimator animator in animators)
				{
					if (animator.animatorController == null)
					{
						if (animator.animatorControllerName != "")
						{
							FindAnimatorByName(animator.animatorControllerName);
						}
					}
				}
			}
			public void FindAnimatorByName(string animatorName)
			{
				/*bool found = false;
                if (dynamicallyAddFromResources)
                {
                    found = DynamicAssetLoader.Instance.AddAssetsFromResources<RuntimeAnimatorController>("", null, animatorName, SetFoundAnimators);
                }
                if(found == false && dynamicallyAddFromAssetBundles)
                {
                    DynamicAssetLoader.Instance.AddAssetsFromAssetBundles<RuntimeAnimatorController>(ref assetBundlesUsedDict, false, "", null, animatorName, SetFoundAnimators);
                }*/
				DynamicAssetLoader.Instance.AddAssets<RuntimeAnimatorController>(ref assetBundlesUsedDict, true, true, false, "", "", null, animatorName, SetFoundAnimators);

			}
			//This function is probablt redundant since animators from this class should never cause assetbundles to download
			//and therefore there should never be any 'temp' assets that need to be replaced
			public void SetAnimator(RuntimeAnimatorController controller)
			{
				foreach (RaceAnimator animator in animators)
				{
					if (animator.animatorControllerName != "")
					{
						if (animator.animatorControllerName == controller.name)
						{
							animator.animatorController = controller;
						}
					}
				}
			}
			private void SetFoundAnimators(RuntimeAnimatorController[] foundControllers)
			{
				foreach (RuntimeAnimatorController foundController in foundControllers)
				{
					foreach (RaceAnimator animator in animators)
					{
						if (animator.animatorController == null)
						{
							if (animator.animatorControllerName != "")
							{
								if (animator.animatorControllerName == foundController.name)
								{
									animator.animatorController = foundController;
								}
							}
						}
					}
				}
				CleanAnimatorsFromResourcesAndBundles();
			}
			public void CleanAnimatorsFromResourcesAndBundles()
			{
				Resources.UnloadUnusedAssets();
			}
		}

		[Serializable]
		public class ColorValue
		{
			public string Name;
			public Color Color = Color.white;
			public Color MetallicGloss = new Color(0, 0, 0, 0);
			public ColorValue() { }

			public ColorValue(string name, Color color)
			{
				Name = name;
				Color = color;
			}
			public ColorValue(string name, OverlayColorData color)
			{
				Name = name;
				Color = color.color;
				if (color.channelAdditiveMask.Length == 3)
				{
					MetallicGloss = color.channelAdditiveMask[2];
				}
			}
			public ColorValue(ColorValue col)
			{
				Name = col.Name;
				Color = new Color(col.Color.r, col.Color.g, col.Color.b, col.Color.a);
				MetallicGloss = new Color(col.MetallicGloss.r, col.MetallicGloss.g, col.MetallicGloss.b, col.MetallicGloss.a);
			}
		}

		//I need colors to be able to have the channelAdditiveMask[2] aswell because if they do you can set Metallic/Glossy colors
		[Serializable]
		public class ColorValueList
		{

			public List<ColorValue> Colors = new List<ColorValue>();

			public ColorValueList()
			{
				//
			}
			public ColorValueList(List<ColorValue> colorValueList)
			{
				Colors = colorValueList;
			}

			private ColorValue GetColorValue(string name)
			{
				foreach (ColorValue cv in Colors)
				{
					if (cv.Name == name)
						return cv;
				}
				return null;
			}

			public bool GetColor(string Name, out Color c)
			{
				ColorValue cv = GetColorValue(Name);
				if (cv != null)
				{
					c = cv.Color;
					return true;
				}
				c = Color.white;
				return false;
			}

			public bool GetColor(string Name, out OverlayColorData c)
			{
				ColorValue cv = GetColorValue(Name);
				if (cv != null)
				{
					c = new OverlayColorData(3);
					c.name = cv.Name;
					c.color = cv.Color;
					c.channelAdditiveMask[2] = cv.MetallicGloss;
					return true;
				}
				c = new OverlayColorData(3);
				return false;
			}

			public void SetColor(string name, Color c)
			{
				ColorValue cv = GetColorValue(name);
				if (cv != null)
				{
					cv.Color = c;
				}
				else
				{
					Colors.Add(new ColorValue(name, c));
				}
			}

			public void SetColor(string name, OverlayColorData c)
			{
				ColorValue cv = GetColorValue(name);
				if (cv != null)
				{
					cv.Color = c.color;
					if (c.channelAdditiveMask.Length == 3)
					{
						cv.MetallicGloss = c.channelAdditiveMask[2];
					}
				}
				else
				{
					Colors.Add(new ColorValue(name, c));
				}
			}

			public void RemoveColor(string name)
			{
				foreach (ColorValue cv in Colors)
				{
					if (cv.Name == name)
						Colors.Remove(cv);
				}
			}
		}

#endregion
	}

    /// <summary>
    /// A DnaSetter is used to set a specific piece of DNA on the avatar
    /// that it is pulled from.
    /// </summary>
    public class DnaSetter
    {
        public string Name; // The name of the DNA.
        public float Value; // Current value of the DNA.

        protected int OwnerIndex;    // position of DNA in index, created at initialization
        protected UMADnaBase Owner;  // owning DNA class. Used to set the DNA by index

        /// <summary>
        /// Construct a DnaSetter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="ownerIndex"></param>
        /// <param name="owner"></param>
        public DnaSetter(string name, float value, int ownerIndex, UMADnaBase owner)
        {
            Name = name;
            Value = value;
            OwnerIndex = ownerIndex;
            Owner = owner;
        }

        /// <summary>
        /// Set the current DNA value. You will need to rebuild the character to see 
        /// the results change.
        /// </summary>
        public void Set(float val)
        {
            Value = val;
            Owner.SetValue(OwnerIndex, val);
        }

        /// <summary>
        /// Set the current DNA value. You will need to rebuild the character to see 
        /// the results change.
        /// </summary>
        public void Set()
        {
            Owner.SetValue(OwnerIndex, Value);
        }

		/// <summary>
        /// Gets the current DNA value.
        /// </summary>
        public float Get()
        {
            return Owner.GetValue(OwnerIndex); 
        }
    }
}
