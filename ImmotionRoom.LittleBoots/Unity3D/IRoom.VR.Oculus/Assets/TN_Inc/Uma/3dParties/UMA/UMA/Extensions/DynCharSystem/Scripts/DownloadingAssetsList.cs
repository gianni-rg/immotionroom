using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UMAAssetBundleManager;

namespace UMA
{

	[System.Serializable]
	public class DownloadingAssetsList
	{
		public List<DownloadingAssetItem> downloadingItems = new List<DownloadingAssetItem>();
		Dictionary<int, List<DownloadingAssetItem>> downloadingItemsDict = new Dictionary<int, List<DownloadingAssetItem>>();
		public bool areDownloadedItemsReady = true;

		public bool DownloadingItemsContains(string itemToCheck)
		{
			bool res = false;
			if (downloadingItems.Find(item => item.requiredAssetName == itemToCheck) != null)
			{
				res = true;
			}
			return res;
		}
		public bool DownloadingItemsContains(List<string> itemsToCheck)
		{
			bool res = false;
			for (int i = 0; i < itemsToCheck.Count; i++)
			{
				if (downloadingItems.Find(item => item.requiredAssetName == itemsToCheck[i]) != null)
				{
					res = true;
					break;
				}
			}
			return res;
		}
		/// <summary>
		/// Generates a temporary item of type T. It then adds a new DownloadingAssetItem to downloadingItems that contains a refrence to this created temp asset and the name of the asset that it should be replaced by once the given assetbundle has completed downloading.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requiredAssetName"></param>
		/// <param name="containingBundle"></param>
		/// <returns></returns>
		public T AddDownloadItem<T>(int batchID, string requiredAssetName, int? requiredAssetNameHash, string containingBundle, Delegate callback = null/*, UMAAvatarBase requestingUma = null*/) where T : UnityEngine.Object
		{
			T thisTempAsset = null;
			if (downloadingItems.Find(item => item.requiredAssetName == requiredAssetName) == null)
			{
				if (requiredAssetNameHash == null)
				{
					requiredAssetNameHash = UMAUtils.StringToHash(requiredAssetName);
				}
				if (typeof(T) == typeof(RaceData))
				{
					thisTempAsset = ScriptableObject.Instantiate(DynamicAssetLoader.Instance.placeholderRace) as T;
					(thisTempAsset as RaceData).raceName = requiredAssetName;
					(thisTempAsset as RaceData).name = requiredAssetName;
				}
				else if (typeof(T) == typeof(SlotDataAsset))
				{
					thisTempAsset = ScriptableObject.Instantiate(DynamicAssetLoader.Instance.placeholderSlot) as T;
					(thisTempAsset as SlotDataAsset).name = requiredAssetName;
					(thisTempAsset as SlotDataAsset).slotName = requiredAssetName;
					//also needs the name hash
					(thisTempAsset as SlotDataAsset).nameHash = (int)requiredAssetNameHash;//we can safely force because we just set this above
				}
				else if (typeof(T) == typeof(OverlayDataAsset))
				{
					thisTempAsset = ScriptableObject.Instantiate(DynamicAssetLoader.Instance.placeholderOverlay) as T;
					(thisTempAsset as OverlayDataAsset).name = requiredAssetName;
					(thisTempAsset as OverlayDataAsset).overlayName = requiredAssetName;
					(thisTempAsset as OverlayDataAsset).nameHash = (int)requiredAssetNameHash;
				}
				else if (typeof(T) == typeof(UMATextRecipe))
				{
					if (AssetBundleManager.AssetBundleIndexObject.IsAssetWardrobeRecipe(containingBundle, requiredAssetName))
					{
						thisTempAsset = ScriptableObject.Instantiate(DynamicAssetLoader.Instance.placeholderWardrobeRecipe) as T;
						(thisTempAsset as UMATextRecipe).recipeType = "Wardrobe";
						(thisTempAsset as UMATextRecipe).wardrobeSlot = AssetBundleManager.AssetBundleIndexObject.AssetWardrobeSlot(containingBundle, requiredAssetName);
						(thisTempAsset as UMATextRecipe).Hides = AssetBundleManager.AssetBundleIndexObject.AssetWardrobeHides(containingBundle, requiredAssetName);
						(thisTempAsset as UMATextRecipe).compatibleRaces = AssetBundleManager.AssetBundleIndexObject.AssetWardrobeCompatibleWith(containingBundle, requiredAssetName);
					}
					else
					{
						thisTempAsset = ScriptableObject.Instantiate(DynamicAssetLoader.Instance.placeholderRace.baseRaceRecipe) as T;
					}
					thisTempAsset.name = requiredAssetName;
				}
				else if (typeof(T) == typeof(RuntimeAnimatorController))//Possibly can be removed- just here because we may need to do something else with animators...
				{
					thisTempAsset = (T)Activator.CreateInstance(typeof(T));
					(thisTempAsset as RuntimeAnimatorController).name = requiredAssetName;
				}
				else
				{
					//Need to check for ScriptableObjects here because they require different instantiation
					if (typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
					{
						thisTempAsset = ScriptableObject.CreateInstance(typeof(T)) as T;
					}
					else
					{
						thisTempAsset = (T)Activator.CreateInstance(typeof(T));
					}
					thisTempAsset.name = requiredAssetName;
				}
				var thisDlItem = new DownloadingAssetItem(batchID, requiredAssetName, thisTempAsset, containingBundle, callback/*, requestingUma*/);
				downloadingItems.Add(thisDlItem);
				if (!downloadingItemsDict.ContainsKey(batchID))
				{
					downloadingItemsDict[batchID] = new List<DownloadingAssetItem>();
				}
				if (!downloadingItemsDict[batchID].Contains(thisDlItem))
				{
					downloadingItemsDict[batchID].Add(thisDlItem);
				}
			}
			else
			{
				DownloadingAssetItem dlItem = null;
				if (downloadingItems.Find(item => item.requiredAssetName == requiredAssetName) != null)
					dlItem = downloadingItems.Find(item => item.requiredAssetName == requiredAssetName);
				if (dlItem != null)
				{
					thisTempAsset = dlItem.tempAsset as T;
				}
				else
				{
					Debug.LogWarning("Could not get TempAsset for " + requiredAssetName);
				}
			}
			return thisTempAsset;
		}
		/// <summary>
		/// Removes a list of downloadingAssetItems from the downloadingItems List
		/// TODO: This should be generic too. The downloadingItem needs to store the UnityAction and do that when the download completes (like DynamicAssetLoader does when the download is started and the temp asset is returned)
		/// </summary>
		/// <param name="assetName"></param>
		public IEnumerator RemoveDownload(List<DownloadingAssetItem> itemsToRemove /*, string onlyUpdateType = ""*/)
		{
			//Not used any more UMAs check the status of stuff they asked for themselves
			//Dictionary<UMAAvatarBase, List<string>> updatedUMAs = new Dictionary<UMAAvatarBase, List<string>>();
			foreach (DownloadingAssetItem item in itemsToRemove)
			{
				item.isBeingRemoved = true;
			}

			foreach (DownloadingAssetItem item in itemsToRemove)
			{
				string error = "";
				//we need to check everyitem in this batch belongs to an asset bundle that has actually been loaded
				LoadedAssetBundle loadedBundleTest = AssetBundleManager.GetLoadedAssetBundle(item.containingBundle, out error);
				AssetBundle loadedBundleABTest = loadedBundleTest.m_AssetBundle;
				if (loadedBundleABTest == null && (error == null || error == ""))
				{
					while (loadedBundleTest.m_AssetBundle == null)
					{
						//could say we are unpacking here
						yield return null;
					}
				}
				if ((error != null && error != ""))
				{
					Debug.LogError(error);
					yield break;
				}
			}
			//Now every item in the batch should be in a loaded bundle that is ready to use.
			foreach (DownloadingAssetItem item in itemsToRemove)
			{
				if (item != null)
				{
					string error = "";
					var loadedBundle = AssetBundleManager.GetLoadedAssetBundle(item.containingBundle, out error);
					var loadedBundleAB = loadedBundle.m_AssetBundle;
					if ((error != null && error != ""))
					{
						Debug.LogError(error);
						yield break;
					}
					if (item.tempAsset.GetType() == typeof(RaceData))
					{
						RaceData actualRace = loadedBundleAB.LoadAsset<RaceData>(item.requiredAssetName);
						UMAContext.Instance.raceLibrary.AddRace(actualRace);
						UMAContext.Instance.raceLibrary.UpdateDictionary();
					}
					else if (item.tempAsset.GetType() == typeof(SlotDataAsset))
					{
						SlotDataAsset thisSlot = null;
						thisSlot = loadedBundleAB.LoadAsset<SlotDataAsset>(item.requiredAssetName);
						if (thisSlot == null)
						{
							//check for item.requiredAssetName + "_Slot" here since we cant get SlotDataAsset.slotName 
							//unless the asset is actually loaded and we can only load from an asset bundle by file name
							thisSlot = loadedBundleAB.LoadAsset<SlotDataAsset>(item.requiredAssetName + "_Slot");
						}
						if (thisSlot != null)
						{
							UMAContext.Instance.slotLibrary.AddSlotAsset(thisSlot);
						}
						else
						{
							Debug.LogWarning("[DynamicAssetLoader] could not add downloaded slot" + item.requiredAssetName);
						}
					}
					else if (item.tempAsset.GetType() == typeof(OverlayDataAsset))
					{
						OverlayDataAsset thisOverlay = null;
						thisOverlay = loadedBundleAB.LoadAsset<OverlayDataAsset>(item.requiredAssetName);
						if (thisOverlay != null)
						{
							UMAContext.Instance.overlayLibrary.AddOverlayAsset(thisOverlay);
						}
						else
						{
							Debug.LogWarning("[DynamicAssetLoader] could not add downloaded overlay" + item.requiredAssetName + " from assetbundle " + item.containingBundle);
						}
					}
					else if (item.tempAsset.GetType() == typeof(UMATextRecipe))
					{
						UMATextRecipe downloadedRecipe = loadedBundleAB.LoadAsset<UMATextRecipe>(item.requiredAssetName);
						(UMAContext.Instance.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem).AddRecipe(downloadedRecipe);
					}
					//TODO what should be happening is that the requested animator - if in assetbundle- is added to the assets that the UMA is waiting for
					/*else if (item.tempAsset.GetType() == typeof(RuntimeAnimatorController) && item.requestingUma != null)
                    {
                        var downloadedController = loadedBundleAB.LoadAsset<RuntimeAnimatorController>(item.requiredAssetName);
                        (item.requestingUma as UMACharacterSystem.DynamicCharacterAvatar).raceAnimationControllers.SetAnimator(downloadedController);
                    }*/
					else if (item.dynamicCallback != null)
					{
						//get the asset as whatever the type of the tempAsset is
						//send this as an array to the dynamicCallback
						Debug.LogWarning("[DynamicAssetList] used item.DynamicCallback - item.tempAsset.GetType() was " + item.tempAsset.GetType());
						var downloadedAsset = loadedBundleAB.LoadAsset(item.requiredAssetName, item.tempAsset.GetType());
						var downloadedAssetArray = Array.CreateInstance(item.tempAsset.GetType(), 1);
						downloadedAssetArray.SetValue(downloadedAsset, 0);
						item.dynamicCallback.DynamicInvoke(downloadedAssetArray);
					}
					if (error != "" && error != null)
					{
						Debug.LogError(error);
					}
				}
				var batchId = item.batchID;
				downloadingItems.Remove(item);
				downloadingItemsDict[batchId].Remove(item);
			}
			List<int> dlDictsToRemove = new List<int>();
			foreach (KeyValuePair<int, List<DownloadingAssetItem>> dlDictKp in downloadingItemsDict)
			{
				if (dlDictKp.Value.Count == 0)
				{
					/* //UMA now check the status of what they asked for themselves- Only DynamicCharacterAvatar does this though so if you are using assetBundles you now MUST use a DynamicCharacterAvatar (or derived)
					if (updatedUMAs.Count > 0)
                    {
                        foreach (KeyValuePair<UMAAvatarBase, List<string>> kp in updatedUMAs)
                        {
                            if (kp.Key as UMACharacterSystem.DynamicCharacterAvatar)// TODO check how this works with derived classes
                            {
								//removed
							}
                            else
                            {
                                kp.Key.umaData.Dirty(true, true, true);
                            }
                        }
                    }*/
					dlDictsToRemove.Add(dlDictKp.Key);
				}
			}
			for (int dlri = 0; dlri < dlDictsToRemove.Count; dlri++)
			{
				downloadingItemsDict.Remove(dlDictsToRemove[dlri]);
			}
			if (downloadingItems.Count == 0)
				areDownloadedItemsReady = true;
			//yield break;
		}

		/// <summary>
		/// Updates the list of downloadingItems, checks if any have finished downloading and if they have triggers the RemoveDownload method on them
		/// </summary>
		public void Update()
		{
			List<DownloadingAssetItem> finishedItems = new List<DownloadingAssetItem>();
			if (downloadingItems.Count > 0)
			{
				areDownloadedItemsReady = false;
				List<string> finishedBundles = new List<string>();
				foreach (KeyValuePair<int, List<DownloadingAssetItem>> kp in downloadingItemsDict)
				{
					bool canProcessBatch = true;
					foreach (DownloadingAssetItem dl in kp.Value)
					{
						dl.UpdateProgress();
						string error = "";
						if (finishedBundles.Contains(dl.containingBundle))
						{
							if (dl.flagForRemoval == false)
							{
								dl.flagForRemoval = true;
							}
							else
							{
								if (dl.isBeingRemoved)
									canProcessBatch = false;
							}
						}
						else if (AssetBundleManager.GetLoadedAssetBundle(dl.containingBundle, out error) != null)
						{
							finishedBundles.Add(dl.containingBundle);
							if (dl.flagForRemoval == false)
							{
								dl.flagForRemoval = true;
							}
							else
							{
								if (dl.isBeingRemoved)
									canProcessBatch = false;
							}
						}
						else
						{
							canProcessBatch = false;
						}
						if (error != "")//May need to check if error != null too
						{
							//AssetBundleManager already logs the error
						}
					}
					if (canProcessBatch)
					{
						finishedItems.AddRange(kp.Value);
					}
				}
			}
			//send the finished downloads to be processed
			if (finishedItems.Count > 0)
			{
				DynamicAssetLoader.Instance.StartCoroutine(RemoveDownload(finishedItems));
			}
		}
		/// <summary>
		/// Returns the temporary asset that was generated when the DownloadingAssetItem for the given assetName was created
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assetName"></param>
		/// <returns></returns>
		public T Get<T>(string assetName) where T : UnityEngine.Object
		{
			T tempAsset = null;
			if (downloadingItems.Find(item => item.requiredAssetName == assetName) != null)
			{
				if (downloadingItems.Find(item => item.requiredAssetName == assetName).tempAsset.GetType() == typeof(T))
					tempAsset = downloadingItems.Find(item => item.requiredAssetName == assetName) as T;
			}
			return tempAsset;
		}
		/// <summary>
		/// Returns the download progress of the asset bundle(s) required for the given asset to become available
		/// </summary>
		/// <param name="assetName"></param>
		/// <returns></returns>
		public float GetDownloadProgressOf(string assetName)
		{
			float progress = 0;
			DownloadingAssetItem item = null;
			item = downloadingItems.Find(aitem => aitem.requiredAssetName == assetName);
			if (item != null)
			{
				progress = item.Progress;
			}
			else
			{
				Debug.Log(assetName + " was not downloading");
			}
			return progress;
		}
	}
}
