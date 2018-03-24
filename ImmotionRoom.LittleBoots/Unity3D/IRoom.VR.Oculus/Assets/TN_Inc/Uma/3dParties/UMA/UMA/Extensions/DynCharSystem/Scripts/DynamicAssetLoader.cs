using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UMAAssetBundleManager;

namespace UMA
{
	public class DynamicAssetLoader : MonoBehaviour
	{
		static DynamicAssetLoader _instance;

		[Tooltip("Set the server URL that assetbundles can be loaded from. Used in a live build and when the LocalAssetServer is turned off. Requires trailing slash but NO platform name")]
		public string remoteServerURL = "";
		[Tooltip("Use the JSON version of the assetBundleIndex rather than the assetBundleVersion.")]
		public bool useJsonIndex = false;
		[Tooltip("Set the server URL for the AssetBundleIndex json data. You can use this to make a server request that could generate an index on the fly for example. Used in a live build and when the LocalAssetServer is turned off. TIP use [PLATFORM] to use the current platform name in the URL")]
		public string remoteServerIndexURL = "";
		public bool makePersistent;
		[Tooltip("A list of assetbundles to preload when the game starts. After these have completed loading any GameObject in the gameObjectsToActivate field will be activated.")]
		public List<string> assetBundlesToPreLoad = new List<string>();
		[Tooltip("GameObjects that will be activated after the list of assetBundlesToPreLoad has finished downloading.")]
		public List<GameObject> gameObjectsToActivate = new List<GameObject>();
		[Tooltip("GameObjects that will be activated after Initialization completes.")]
		public List<GameObject> gameObjectsToActivateOnInit = new List<GameObject>();
		[Space]
		public GameObject loadingMessageObject;
		public Text loadingMessageText;
		public string loadingMessage = "";
		[HideInInspector]
		[System.NonSerialized]
		public float percentDone = 0f;
		[HideInInspector]
		[System.NonSerialized]
		public bool assetBundlesDownloading;
		[HideInInspector]
		[System.NonSerialized]
		public bool canCheckDownloadingBundles;
		bool isInitializing = false;
		[HideInInspector]
		public bool isInitialized = false;
		//WE DONT NEED THIS- if there is anything in the list its true and afterwards the list should be cleared TODO: Confirm
		[HideInInspector]
		[System.NonSerialized]
		public bool gameObjectsActivated;
		[Space]
		//Default assets fields
		//TODO These should be sent by whatever requested that type of asset so that more types than this can request assets and get a placeholder back while the asset is downloading
		public RaceData placeholderRace;//temp race based on UMAMale with a baseRecipe to generate a temp umaMale TODO: Could have a female too and search the required racename to see if it contains female...
		public UMATextRecipe placeholderWardrobeRecipe;//empty temp wardrobe recipe
		public SlotDataAsset placeholderSlot;//empty temp slot
		public OverlayDataAsset placeholderOverlay;//empty temp overlay. Would be nice if there was some way we could have a shader on this that would 'fill up' as assets loaded maybe?
		//TODO: Just visible for dev
		//[System.NonSerialized]//not sure about this one
		[ReadOnly]
		public DownloadingAssetsList downloadingAssets = new DownloadingAssetsList();

		int? _currentBatchID = null;

		//For SimulationMode in the editor - equivalent of AssetBundleManager.m_downloadedBundles
		//should persist betweem scene loads but not between plays
#if UNITY_EDITOR
		List<string> simulatedDownloadedBundles = new List<string>();
#endif

		/// <summary>
		/// Gets the currentBatchID or generates a new one if it is null. Sets the currentBatch ID to a given value
		/// BatchID's are used so that multiple requests get processed in one loop so that even if overlays are in different bundles to slots
		/// They can be added to the libraries at the same time and avoid slots materials not matching overlay materials
		/// Tip: Rather than setting this explicily, consider calling GenerateBatchID which will provide a unique random id number and set this property at the same time.
		/// </summary>
		public int CurrentBatchID
		{
			get
			{
				if (_currentBatchID == null)
					_currentBatchID = GenerateBatchID();
				return (int)_currentBatchID;
			}
			set
			{
				_currentBatchID = value;
			}
		}

		public static DynamicAssetLoader Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindInstance();
				}
				return _instance;
			}
			set { _instance = value; }
		}

		#region BASE METHODS
		/*void OnEnable()
        {
            if (_instance == null) _instance = this;
            if (!isInitialized)
            {
                StartCoroutine(Initialize());
            }
        }*/

		IEnumerator Start()
		{
			bool destroyingThis = false;
			if (_instance == null)
			{
				_instance = this;
				if (makePersistent)
				{
					DontDestroyOnLoad(this.gameObject);
				}
				if (!isInitialized)
				{
					yield return StartCoroutine(Initialize());
				}
			}
			else if (_instance != this)
			{
				//copy some values over and then destroy this
				if (_instance.makePersistent)
				{
					Debug.Log("[DynamicAssetLoader] _instance was NOT this one and was persistent");
					_instance.assetBundlesToPreLoad.Clear();
					_instance.assetBundlesToPreLoad.AddRange(this.assetBundlesToPreLoad);
					_instance.gameObjectsToActivate.Clear();
					_instance.gameObjectsToActivate.AddRange(this.gameObjectsToActivate);
					_instance.remoteServerIndexURL = this.remoteServerIndexURL;
					Destroy(this.gameObject);
					destroyingThis = true;
				}
				else
				{
					_instance = this;
				}
			}
			else if (_instance == this)//sometimes things have called Instance before Start has actually happenned on this
			{
				if (makePersistent)
				{
					DontDestroyOnLoad(this.gameObject);
				}
				if (!isInitialized)
				{
					yield return StartCoroutine(Initialize());
				}
			}
			/*if (!isInitialized)
            {
                yield return StartCoroutine(Initialize());
            }*/

			//Load any preload asset bundles if there are any
			if (!destroyingThis)
				if (assetBundlesToPreLoad.Count > 0)
				{
					//yield return StartCoroutine(LoadAssetBundlesAsync(assetBundlesToPreLoad));//why does this need to yeild return?
					List<string> bundlesToSend = new List<string>(assetBundlesToPreLoad.Count);
					bundlesToSend.AddRange(assetBundlesToPreLoad);
					StartCoroutine(LoadAssetBundlesAsync(bundlesToSend));
					assetBundlesToPreLoad.Clear();
				}
		}

		void Update()
		{
			if (assetBundlesToPreLoad.Count > 0)
			{
				//yield return StartCoroutine(LoadAssetBundlesAsync(assetBundlesToPreLoad));//why does this need to yeild return?
				List<string> bundlesToSend = new List<string>(assetBundlesToPreLoad.Count);
				bundlesToSend.AddRange(assetBundlesToPreLoad);
				StartCoroutine(LoadAssetBundlesAsync(bundlesToSend));
				assetBundlesToPreLoad.Clear();
			}
#if UNITY_EDITOR
			if (AssetBundleManager.SimulateAssetBundleInEditor)
			{
				//if (!gameObjectsActivated)
				// {
				if (gameObjectsToActivate.Count > 0)
				{
					foreach (GameObject go in gameObjectsToActivate)
					{
						if (!go.activeSelf)
						{
							go.SetActive(true);
						}
					}
					gameObjectsToActivate.Clear();
				}
				//gameObjectsActivated = true;
				//}
				/*if (simulationModeDCSBundlesToUpdate.Count > 0)//will this be too late? Does it even need to happen when the assets have been explicitly added?
				{
					var context = UMAContext.Instance;
					if (UMAContext.Instance != null)
					{
						var thisDCS = UMAContext.Instance.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem;
						if (thisDCS != null)
						{
							for(int i = 0; i < simulationModeDCSBundlesToUpdate.Count; i++)
							{
								thisDCS.Refresh(false,simulationModeDCSBundlesToUpdate[i]);
							}
						}
					}
					simulationModeDCSBundlesToUpdate.Clear();
                }*/
			}
			else
			{
#endif
				if (downloadingAssets.downloadingItems.Count > 0)
					downloadingAssets.Update();
				if (downloadingAssets.areDownloadedItemsReady == false)
					assetBundlesDownloading = true;
				if ((assetBundlesDownloading || downloadingAssets.areDownloadedItemsReady == false) && canCheckDownloadingBundles == true)
				{
					if (!AssetBundleManager.AreBundlesDownloading() && downloadingAssets.areDownloadedItemsReady == true)
					{
						assetBundlesDownloading = false;
						//LoadBundleAsync should do this now- and only if the bundle has UMATextRecipes in or (TODO- maybe) TextAssets
						//but text assets could be anything... Should we really update if just ANY text assets are found?
						//I think if CharacterRecipes are created before buildTime they should perhaps be ScriptableObjects- of a Type- pretty much aka UMATextRecipe Assets!
						//That way the only thing left to deal with is actual text assets created at run time and thats beyond the scope of DCS/UMA really
						/*var context = UMAContext.Instance;
						if (UMAContext.Instance != null)
						{
							var thisDCS = UMAContext.Instance.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem;
							if (thisDCS != null)
							{
								thisDCS.Refresh();//DCSRefresh only needs to be called if the downloaded asset bundle contained UMATextRecipes (or character recipes) but I dont know how to check for that
							}
						}*/

						if (gameObjectsToActivate.Count > 0)
						{
							foreach (GameObject go in gameObjectsToActivate)
							{
								if (!go.activeSelf)
								{
									go.SetActive(true);
								}
							}
							gameObjectsToActivate.Clear();
						}
					}
				}
#if UNITY_EDITOR
			}
#endif
		}

		/*IEnumerator EnableActivatables()
		{
			var goToActivate = new List<GameObject>();
			goToActivate.AddRange(gameObjectsToActivate);
			gameObjectsToActivate.Clear();
			yield return null;
			foreach (GameObject go in goToActivate)
			{
				if (!go.activeSelf)
				{
					go.SetActive(true);
					yield return null;
				}
			}
		}*/
		/// <summary>
		/// Finds the DynamicAssetLoader in the scene and treats it like a singleton.
		/// </summary>
		/// <returns>The DynamicAssetLoader.</returns>
		public static DynamicAssetLoader FindInstance()
		{
			if (_instance == null)
			{
				DynamicAssetLoader[] dynamicAssetLoaders = FindObjectsOfType(typeof(DynamicAssetLoader)) as DynamicAssetLoader[];
				if (dynamicAssetLoaders.Length > 0)
				//if (dynamicAssetLoaders[0] != null)
				{
					_instance = dynamicAssetLoaders[0];
				}
			}
			return _instance;
		}
		#endregion

		#region CHECK DOWNLOADS METHODS
		public bool downloadingAssetsContains(string assetToCheck)
		{
			return downloadingAssets.DownloadingItemsContains(assetToCheck);
		}

		public bool downloadingAssetsContains(List<string> assetsToCheck)
		{
			return downloadingAssets.DownloadingItemsContains(assetsToCheck);
		}

		#endregion

		#region DOWNLOAD METHODS

		/// <summary>
		/// Initialize the downloading URL. eg. local server / iOS ODR / or the download URL as defined in the component settings if Simulation Mode and Local Asset Server is off
		/// </summary>
		void InitializeSourceURL()
		{
			string URLToUse = "";
			if (SimpleWebServer.ServerURL != "")
			{
#if UNITY_EDITOR
				if (SimpleWebServer.serverStarted)//this is not true in builds no matter what- but we in the editor we need to know
#endif
					URLToUse = remoteServerURL = SimpleWebServer.ServerURL;
				Debug.Log("[DynamicAssetLoader] SimpleWebServer.ServerURL = " + URLToUse);
			}
			else
			{
				URLToUse = remoteServerURL;
			}
			//#endif
			if (URLToUse != "")
				AssetBundleManager.SetSourceAssetBundleURL(URLToUse);
			else
			{
                string errorString = "LocalAssetBundleServer was off and no remoteServerURL was specified. One of these must be set in order to use any AssetBundles!";
#if UNITY_EDITOR
				errorString = "Switched to Simulation Mode because LocalAssetBundleServer was off and no remoteServerURL was specified in the Scenes' DynamicAssetLoader. One of these must be set in order to actually use your AssetBundles.";

#endif
				AssetBundleManager.SimulateOverride = true;
				var context = UMAContext.FindInstance();
				if (context != null)
				{
					if ((context.dynamicCharacterSystem != null && (context.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem).dynamicallyAddFromAssetBundles)
						|| (context.raceLibrary != null && (context.raceLibrary as DynamicRaceLibrary).dynamicallyAddFromAssetBundles)
						|| (context.slotLibrary != null && (context.slotLibrary as DynamicSlotLibrary).dynamicallyAddFromAssetBundles)
						|| (context.overlayLibrary != null && (context.overlayLibrary as DynamicOverlayLibrary).dynamicallyAddFromAssetBundles))
					{
						Debug.LogWarning(errorString);
					}
				}
				else //if you are just using dynamicassetLoader independently of UMA then you may still want this message
				{
					Debug.LogWarning(errorString);
				}
			}
			return;

		}
		/// <summary>
		/// Initializes AssetBundleManager which loads the AssetBundleManifest object and the AssetBundleIndex object.
		/// </summary>
		/// <returns></returns>
		protected IEnumerator Initialize()
		{
#if UNITY_EDITOR
			if (AssetBundleManager.SimulateAssetBundleInEditor)
			{
				isInitialized = true;
				yield break;
			}
#endif
			if (isInitializing == false)
			{
				isInitializing = true;
				InitializeSourceURL();//in the editor this might set AssetBundleManager.SimulateAssetBundleInEditor to be true aswell so check that
#if UNITY_EDITOR
				if (AssetBundleManager.SimulateAssetBundleInEditor)
				{
					isInitialized = true;
					isInitializing = false;
					if (gameObjectsToActivateOnInit.Count > 0)
					{
						for (int i = 0; i < gameObjectsToActivateOnInit.Count; i++)
						{
							gameObjectsToActivateOnInit[i].SetActive(true);
						}
						gameObjectsToActivateOnInit.Clear();
					}
					yield break;
				}else
#endif
					//DnamicAssetLoader should still say its initialized even no remoteServer URL was set (either manually or by the LocalWebServer)
					//because we still want to run normally to load assets from Resources
					if (remoteServerURL == "")
				{
					isInitialized = true;
					isInitializing = false;
					if (gameObjectsToActivateOnInit.Count > 0)
					{
						for (int i = 0; i < gameObjectsToActivateOnInit.Count; i++)
						{
							gameObjectsToActivateOnInit[i].SetActive(true);
						}
						gameObjectsToActivateOnInit.Clear();
					}
					yield break;
				}
				var request = AssetBundleManager.Initialize(useJsonIndex, remoteServerIndexURL);
				if (request != null)
				{
					while (AssetBundleManager.IsOperationInProgress(request))
					{
						yield return null;
					}
					isInitializing = false;
					if (AssetBundleManager.AssetBundleIndexObject != null)
					{
						isInitialized = true;
						if (gameObjectsToActivateOnInit.Count > 0)
						{
							for (int i = 0; i < gameObjectsToActivateOnInit.Count; i++)
							{
								gameObjectsToActivateOnInit[i].SetActive(true);
							}
							gameObjectsToActivateOnInit.Clear();
						}
					}
					else
					{
						//if we are in the editor this can only have happenned because the asset bundles were not built and by this point
						//an error will have already been shown about that and AssetBundleManager.SimulationOverride will be true so we can just continue.
#if UNITY_EDITOR
						if (AssetBundleManager.AssetBundleIndexObject == null)
						{
							isInitialized = true;
							yield break;
						}
#endif
					}
				}
				else
				{
					Debug.LogWarning("AssetBundleManager failed to initialize correctly");
				}
			}
		}
		/// <summary>
		/// Generates a batch ID for use when grouping assetbundle asset requests together so they can be processed in the same cycle (to avoid UMA Generation errors).
		/// </summary>
		/// <returns></returns>
		public int GenerateBatchID()
		{
			CurrentBatchID = UnityEngine.Random.Range(1000000, 2000000);
			return CurrentBatchID;
		}
		/// <summary>
		/// Load a single assetbundle (and its dependencies) asynchroniously and sets the Loading Messages.
		/// </summary>
		/// <param name="assetBundleToLoad"></param>
		/// <param name="loadingMsg"></param>
		/// <param name="loadedMsg"></param>
		public void LoadAssetBundle(string assetBundleToLoad, string loadingMsg = "", string loadedMsg = "")
		{
			var assetBundlesToLoadList = new List<string>();
			assetBundlesToLoadList.Add(assetBundleToLoad);
			LoadAssetBundles(assetBundlesToLoadList, loadingMsg, loadedMsg);
		}
		/// <summary>
		/// Load multiple assetbundles (and their dependencies) asynchroniously and sets the Loading Messages.
		/// </summary>
		/// <param name="assetBundlesToLoad"></param>
		/// <param name="loadingMsg"></param>
		/// <param name="loadedMsg"></param>
		public void LoadAssetBundles(string[] assetBundlesToLoad, string loadingMsg = "", string loadedMsg = "")
		{
			var assetBundlesToLoadList = new List<string>(assetBundlesToLoad);
			LoadAssetBundles(assetBundlesToLoadList, loadingMsg, loadedMsg);
		}
		/// <summary>
		/// Load multiple assetbundles (and their dependencies) asynchroniously and sets the Loading Messages.
		/// </summary>
		/// <param name="assetBundlesToLoad"></param>
		/// <param name="loadingMsg"></param>
		/// <param name="loadedMsg"></param>
		public void LoadAssetBundles(List<string> assetBundlesToLoad, string loadingMsg = "", string loadedMsg = "")
		{
#if UNITY_EDITOR
			if (AssetBundleManager.SimulateAssetBundleInEditor)
			{
				//Actually we DO still need to do something here
				foreach (string requiredBundle in assetBundlesToLoad)
				{
					SimulateLoadAssetBundle(requiredBundle);
				}
				return;
			}
#endif
			List<string> assetBundlesToReallyLoad = new List<string>();
			foreach (string requiredBundle in assetBundlesToLoad)
			{
				if (!AssetBundleManager.IsAssetBundleDownloaded(requiredBundle))
				{
					assetBundlesToReallyLoad.Add(requiredBundle);
				}
			}
			if (assetBundlesToReallyLoad.Count > 0)
			{
				assetBundlesDownloading = true;
				canCheckDownloadingBundles = false;
				StartCoroutine(LoadAssetBundlesAsync(assetBundlesToReallyLoad));
			}
		}
		/// <summary>
		/// Loads a list of asset bundles and their dependencies asynchroniously
		/// </summary>
		/// <param name="assetBundlesToLoad"></param>
		/// <returns></returns>
		protected IEnumerator LoadAssetBundlesAsync(List<string> assetBundlesToLoad, string loadingMsg = "", string loadedMsg = "")
		{
#if UNITY_EDITOR
			if (AssetBundleManager.SimulateAssetBundleInEditor)
				yield break;
#endif
			if (!isInitialized)
			{
				if (!isInitializing)
				{
					yield return StartCoroutine(Initialize());
				}
				else
				{
					while (isInitialized == false)
					{
						yield return null;
					}
				}
			}
			string[] bundlesInManifest = AssetBundleManager.AssetBundleIndexObject.GetAllAssetBundles();
			foreach (string assetBundleName in assetBundlesToLoad)
			{
				foreach (string bundle in bundlesInManifest)
				{
					if ((bundle == assetBundleName || bundle.IndexOf(assetBundleName + "/") > -1))
					{
						Debug.Log("Started loading of " + bundle);
						if(AssetBundleLoadingIndicator.Instance)
							AssetBundleLoadingIndicator.Instance.Show(bundle, loadingMsg, "", loadedMsg);
						StartCoroutine(LoadAssetBundleAsync(bundle));
					}
				}
			}
			canCheckDownloadingBundles = true;
			assetBundlesDownloading = true;
			yield return null;
		}
		/// <summary>
		/// Loads an asset bundle and its dependencies asynchroniously
		/// </summary>
		/// <param name="bundle"></param>
		/// <returns></returns>
		//DOS NOTES: if the local server is turned off after it was on when AssetBundleManager was initialized 
		//(like could happen in the editoror if you run a build that uses the local server but you have not started Unity and turned local server on)
		//then this wrongly says that the bundle has downloaded
#pragma warning disable 0219 //remove the warning that we are not using loadedBundle- since we want the error
		protected IEnumerator LoadAssetBundleAsync(string bundle)
		{
			float startTime = Time.realtimeSinceStartup;
			AssetBundleManager.LoadAssetBundle(bundle, false);
			while (AssetBundleManager.IsAssetBundleDownloaded(bundle) == false)
			{
				yield return null;
			}
			string error = null;
			LoadedAssetBundle loadedBundle = AssetBundleManager.GetLoadedAssetBundle(bundle, out error);
			float elapsedTime = Time.realtimeSinceStartup - startTime;
			Debug.Log(bundle + (error != null ? " was not" : " was") + " loaded successfully in " + elapsedTime + " seconds");
			if (error != null)
			{
				Debug.LogError("[DynamicAssetLoader] Bundle Load Error: " + error);
			}
			yield return true;
			//If this assetBundle contains UMATextRecipes we may need to trigger some post processing...
			//it may have downloaded some dependent bundles too so these may need processing aswell
			var dependencies = AssetBundleManager.AssetBundleIndexObject.GetAllDependencies(bundle);
			//DOS 04112016 so maybe what we need to do here is check the dependencies are loaded too
			if(dependencies.Length > 0)
			{
				for(int i = 0; i < dependencies.Length; i++)
				{
					while(AssetBundleManager.IsAssetBundleDownloaded(dependencies[i]) == false)
					{
						yield return null;
					}
				}
			}
			UMACharacterSystem.DynamicCharacterSystem thisDCS = null;
			if (UMAContext.Instance != null)
			{
				thisDCS = UMAContext.Instance.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem;
			}
			if (thisDCS != null)
			{
				if (AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(bundle, "UMATextRecipe").Length > 0)
				{

					//DCSRefresh only needs to be called if the downloaded asset bundle contained UMATextRecipes (or character recipes) but I dont know how to check for for just that type of text asset
					//Also it actually ONLY needs to search this bundle
					thisDCS.Refresh(false, bundle);
				}
				for (int i = 0; i < dependencies.Length; i++)
				{
					if (AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(dependencies[i], "UMATextRecipe").Length > 0)
					{
						//DCSRefresh only needs to be called if the downloaded asset bundle contained UMATextRecipes (or character recipes) but I dont know how to check for for just that type of text asset
						//Also it actually ONLY needs to search this bundle
						thisDCS.Refresh(false, dependencies[i]);
					}
				}
			}
		}
#pragma warning restore 0219

		#endregion

		#region LOAD ASSETS METHODS
		List<Type> deepResourcesScanned = new List<Type>();
		public bool AddAssets<T>(ref Dictionary<string, List<string>> assetBundlesUsedDict, bool searchResources, bool searchBundles, bool downloadAssetsEnabled, string bundlesToSearch = "", string resourcesFolderPath = "", int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
		{
			bool found = false;
			List<T> assetsToReturn = new List<T>();
			string[] resourcesFolderPathArray = SearchStringToArray(resourcesFolderPath);
			string[] bundlesToSearchArray = SearchStringToArray(bundlesToSearch);
			bool doDeepSearch = (assetName == "" && assetNameHash == null && deepResourcesScanned.Contains(typeof(T)) == false);
			//first do the quick resourcesIndex search if searchResources and we have either a name or a hash
			if (searchResources)
			{
				if (UMAResourcesIndex.Instance != null)
				{
					doDeepSearch = doDeepSearch == true ? UMAResourcesIndex.Instance.enableDynamicIndexing : doDeepSearch;
					found = AddAssetsFromResourcesIndex<T>(ref assetsToReturn, resourcesFolderPathArray, assetNameHash, assetName);
					if ((assetName != "" || assetNameHash != null) && found)
						doDeepSearch = false;
				}
				else
				{
					Debug.LogWarning("[DynamicAssetLoader] UMAResourcesIndex.Instance WAS NULL");
				}
			}
			//if we can and want to search asset bundles
			if ((AssetBundleManager.AssetBundleIndexObject != null || AssetBundleManager.SimulateAssetBundleInEditor == true) || Application.isPlaying == false)
				if (searchBundles && (found == false || (assetName == "" && assetNameHash == null)))
				{
					bool foundHere = AddAssetsFromAssetBundles<T>(ref assetBundlesUsedDict, ref assetsToReturn, downloadAssetsEnabled, bundlesToSearchArray, assetNameHash, assetName, callback, forceDownloadAll);
					found = foundHere == true ? true : found;
					if ((assetName != "" || assetNameHash != null) && found)
						doDeepSearch = false;
				}
			//if enabled and we have not found anything yet or we are getting all items of a type do a deep resources search (slow)
			if (searchResources && ((found == false && doDeepSearch) || doDeepSearch == true))
			{
				//Debug.Log("DID DEEP RESOURCES SEARCH");
				bool foundHere = AddAssetsFromResources<T>(ref assetsToReturn, resourcesFolderPathArray, assetNameHash, assetName);
				found = foundHere == true ? true : found;
				if (assetName == "" && assetNameHash == null && deepResourcesScanned.Contains(typeof(T)) == false)
					deepResourcesScanned.Add(typeof(T));
			}
			if (callback != null)
			{
				callback(assetsToReturn.ToArray());
			}
			return found;
		}

		public bool AddAssets<T>(bool searchResources, bool searchBundles, bool downloadAssetsEnabled, string bundlesToSearch = "", string resourcesFolderPath = "", int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
		{
			var dummyDict = new Dictionary<string, List<string>>();
			return AddAssets<T>(ref dummyDict, searchResources, searchBundles, downloadAssetsEnabled, bundlesToSearch, resourcesFolderPath, assetNameHash, assetName, callback, forceDownloadAll);
		}

		public bool AddAssetsFromResourcesIndex<T>(ref List<T> assetsToReturn, string[] resourcesFolderPathArray, int? assetNameHash = null, string assetName = "") where T : UnityEngine.Object
		{
			bool found = false;
			if (UMAResourcesIndex.Instance == null)
				return found;
			if (assetNameHash != null || assetName != "")
			{
				string foundAssetPath = "";
				if (assetNameHash != null)
				{
					foundAssetPath = UMAResourcesIndex.Instance.Index.GetPath<T>((int)assetNameHash, resourcesFolderPathArray);
				}
				else if (assetName != "")
				{
					foundAssetPath = UMAResourcesIndex.Instance.Index.GetPath<T>(assetName, resourcesFolderPathArray);
				}
				if (foundAssetPath != "")
				{
					T foundAsset = Resources.Load<T>(foundAssetPath);
					if (foundAsset != null)
					{
						assetsToReturn.Add(foundAsset);
						found = true;
					}
				}
			}
			else if (assetNameHash == null && assetName == "")
			{
				if (UMAResourcesIndex.Instance == null)
					Debug.Log("UMAResourcesIndex.Instance WAS NULL");
				else if (UMAResourcesIndex.Instance.Index == null)
					Debug.Log("UMAResourcesIndex.Instance.Index WAS NULL");
				else if (resourcesFolderPathArray == null)
					Debug.Log("resourcesFolderPathArray WAS NULL");
				foreach (string path in UMAResourcesIndex.Instance.Index.GetPaths<T>(resourcesFolderPathArray))
				{
					T foundAsset = Resources.Load<T>(path);
					if (foundAsset != null)
					{
						assetsToReturn.Add(foundAsset);
						found = true;
					}
				}
			}
			return found;
		}

		/// <summary>
		/// Generic Library function to search Resources for a type of asset, optionally filtered by folderpath and asset assetNameHash or assetName. 
		/// Optionally sends the found assets to the supplied callback for processing.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="resourcesFolderPath"></param>
		/// <param name="assetNameHash"></param>
		/// <param name="assetName"></param>
		/// <param name="callback"></param>
		/// <returns>Returns true if a assetNameHash or assetName were specified and an asset with that assetNameHash or assetName is found. Else returns false.</returns>
		public bool AddAssetsFromResources<T>(ref List<T> assetsToReturn, string[] resourcesFolderPathArray, int? assetNameHash = null, string assetName = "") where T : UnityEngine.Object
		{
			bool found = false;
			foreach (string path in resourcesFolderPathArray)
			{
				T[] foundAssets = new T[0];
				var pathPrefix = path == "" ? "" : path + "/";
				if ((typeof(T) == typeof(SlotDataAsset)) || (typeof(T) == typeof(OverlayDataAsset)) || (typeof(T) == typeof(RaceData)))
				{
					//This is hugely expensive but we have to do this as we dont know the asset name, only the race/slot/overlayName which may not be the same. 
					//This will only happen once now that I added the UMAResourceIndex
					foundAssets = Resources.LoadAll<T>(path);
				}
				else
				{
					if (assetName == "")
						foundAssets = Resources.LoadAll<T>(path);
					else
					{
						if (pathPrefix != "")
						{
							T foundAsset = Resources.Load<T>(pathPrefix + assetName);
							if (foundAsset != null)
							{
								if (UMAResourcesIndex.Instance != null)
									UMAResourcesIndex.Instance.Add(foundAsset);
								assetsToReturn.Add(foundAsset);
								found = true;
							}
							else
							{
								foundAssets = Resources.LoadAll<T>(path);
							}
						}
						else
						{
							foundAssets = Resources.LoadAll<T>(path);
						}
					}
				}
				if (found == false)
				{
                    
					for (int i = 0; i < foundAssets.Length; i++)
					{
						if (assetNameHash != null)
						{
							int foundHash = UMAUtils.StringToHash(foundAssets[i].name);
							if (typeof(T) == typeof(SlotDataAsset))
							{
								foundHash = (foundAssets[i] as SlotDataAsset).nameHash;
							}
							if (typeof(T) == typeof(OverlayDataAsset))
							{
								foundHash = (foundAssets[i] as OverlayDataAsset).nameHash;
							}
							if (typeof(T) == typeof(RaceData))
							{
								foundHash = UMAUtils.StringToHash((foundAssets[i] as RaceData).raceName);
							}
							if (foundHash == assetNameHash)
							{
								if (UMAResourcesIndex.Instance != null)
									UMAResourcesIndex.Instance.Add(foundAssets[i], foundHash, true);
								assetsToReturn.Add(foundAssets[i]); 
								found = true;
							}
						}
						else if (assetName != "")
						{
							string foundName = foundAssets[i].name;
							if (typeof(T) == typeof(OverlayDataAsset))
							{
								foundName = (foundAssets[i] as OverlayDataAsset).overlayName;
							}
							if (typeof(T) == typeof(SlotDataAsset))
							{
								foundName = (foundAssets[i] as SlotDataAsset).slotName;
							}
							if (typeof(T) == typeof(RaceData))
							{
								foundName = (foundAssets[i] as RaceData).raceName;
							}
							if (foundName == assetName)
							{
								if (UMAResourcesIndex.Instance != null)
									UMAResourcesIndex.Instance.Add(foundAssets[i], foundName, true);
								assetsToReturn.Add(foundAssets[i]);
								found = true;
							}

						}
						else
						{
							if (UMAResourcesIndex.Instance != null)
								UMAResourcesIndex.Instance.Add(foundAssets[i], true);
							assetsToReturn.Add(foundAssets[i]);
							found = true;
						}
					}
                    if (found)
                    {
                        UMAResourcesIndex.Instance.Save();
                    }
				}
			}
			return found;
		}

		/// <summary>
		/// Generic Library function to search AssetBundles for a type of asset, optionally filtered by bundle name, and asset assetNameHash or assetName. 
		/// Optionally sends the found assets to the supplied callback for processing.
		/// Automatically performs the operation in SimulationMode if AssetBundleManager.SimulationMode is enabled or if the Application is not playing.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="bundlesToSearch"></param>
		/// <param name="assetNameHash"></param>
		/// <param name="assetName"></param>
		/// <param name="callback"></param>
		public bool AddAssetsFromAssetBundles<T>(ref Dictionary<string, List<string>> assetBundlesUsedDict, ref List<T> assetsToReturn, bool downloadAssetsEnabled, string[] bundlesToSearchArray, int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
		{
#if UNITY_EDITOR
			if (AssetBundleManager.SimulateAssetBundleInEditor)
			{
				return SimulateAddAssetsFromAssetBundlesNew<T>(ref assetBundlesUsedDict, ref assetsToReturn, bundlesToSearchArray, assetNameHash, assetName, callback, forceDownloadAll);
			}
			else
			{
#endif
				if (AssetBundleManager.AssetBundleIndexObject == null)
				{
#if UNITY_EDITOR
					Debug.LogWarning("[DynamicAssetLoader] No AssetBundleManager.AssetBundleIndexObject found. Do you need to rebuild your AssetBundles and/or upload the platform index bundle?");
					AssetBundleManager.SimulateOverride = true;
					return SimulateAddAssetsFromAssetBundlesNew<T>(ref assetBundlesUsedDict, ref assetsToReturn, bundlesToSearchArray, assetNameHash, assetName, callback);
#else
					Debug.LogError("[DynamicAssetLoader] No AssetBundleManager.AssetBundleIndexObject found. Do you need to rebuild your AssetBundles and/or upload the platform index bundle?");
                    return false;
#endif
				}
				string[] allAssetBundleNames = AssetBundleManager.AssetBundleIndexObject.GetAllAssetBundleNames();
				string[] assetBundleNamesArray = allAssetBundleNames;
				Type typeParameterType = typeof(T);
				var typeString = typeParameterType.FullName;
				if (bundlesToSearchArray.Length > 0 && bundlesToSearchArray[0] != "")
				{
					List<string> processedBundleNamesArray = new List<string>();
					for (int i = 0; i < bundlesToSearchArray.Length; i++)
					{
						for (int ii = 0; ii < allAssetBundleNames.Length; ii++)
						{
							if (allAssetBundleNames[ii].IndexOf(bundlesToSearchArray[i]) > -1 && !processedBundleNamesArray.Contains(allAssetBundleNames[ii]))
							{
								processedBundleNamesArray.Add(allAssetBundleNames[ii]);
							}
						}
					}
					assetBundleNamesArray = processedBundleNamesArray.ToArray();
				}
				bool assetFound = false;
				for (int i = 0; i < assetBundleNamesArray.Length; i++)
				{
					string error = "";
					if (assetNameHash != null && assetName == "")
					{
						assetName = AssetBundleManager.AssetBundleIndexObject.GetAssetNameFromHash(assetBundleNamesArray[i], assetNameHash, typeString);
					}
					if (assetName != "" || assetNameHash != null)
					{
						if (assetName == "" && assetNameHash != null)
						{
							continue;
						}
						bool assetBundleContains = AssetBundleManager.AssetBundleIndexObject.AssetBundleContains(assetBundleNamesArray[i], assetName, typeString);
						if (!assetBundleContains && typeof(T) == typeof(SlotDataAsset))
						{
							//try the '_Slot' version
							assetBundleContains = AssetBundleManager.AssetBundleIndexObject.AssetBundleContains(assetBundleNamesArray[i], assetName + "_Slot", typeString);
						}
						if (assetBundleContains)
						{
							if (AssetBundleManager.IsAssetBundleDownloaded(assetBundleNamesArray[i]))
							{
								T target = (T)AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error).m_AssetBundle.LoadAsset<T>(assetName);
								if (target == null && typeof(T) == typeof(SlotDataAsset))
								{
									target = (T)AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error).m_AssetBundle.LoadAsset<T>(assetName + "_Slot");
								}
								if (target != null)
								{
									assetFound = true;
									if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
									{
										assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
									}
									if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(assetName))
									{
										assetBundlesUsedDict[assetBundleNamesArray[i]].Add(assetName);
									}
									assetsToReturn.Add(target);
									if (assetName != "")
										break;
								}
								else
								{
									if (error != "" || error != null)
									{
										Debug.LogWarning(error);
									}
								}
							}
							else if (downloadAssetsEnabled)
							{
								//Here we return a temp asset and wait for the bundle to download
								//We dont want to create multiple downloads of the same bundle so check its not already downloading
								if (AssetBundleManager.AreBundlesDownloading(assetBundleNamesArray[i]) == false)
								{
									LoadAssetBundle(assetBundleNamesArray[i]);
								}
								else
								{
									//do nothing its already downloading
								}
								if (assetNameHash == null)
								{
									assetNameHash = AssetBundleManager.AssetBundleIndexObject.GetAssetHashFromName(assetBundleNamesArray[i], assetName, typeString);
								}
								T target = downloadingAssets.AddDownloadItem<T>(CurrentBatchID, assetName, assetNameHash, assetBundleNamesArray[i], callback/*, requestingUMA*/);
								if (target != null)
								{
									assetFound = true;
									if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
									{
										assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
									}
									if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(assetName))
									{
										assetBundlesUsedDict[assetBundleNamesArray[i]].Add(assetName);
									}
									assetsToReturn.Add(target);
									if (assetName != "")
										break;
								}
							}
						}
					}
					else //we are just loading in all assets of type from the downloaded bundles- only realistically possible when the bundles have been downloaded already because otherwise this would trigger the download of all possible assetbundles that contain anything of type T...
					{
						//the problem is that later this asks for get loaded asset bundle and that ALWAYS checks dependencies
						//so rather than make this break is we dont ask for dependencies, we need a version of GetLoadedAssetBundle that DOESNT check dependencies
						if (AssetBundleManager.IsAssetBundleDownloaded(assetBundleNamesArray[i]))
						{
							//Debug.LogWarning(typeof(T).ToString() + " asked for all assets of type in " + assetBundleNamesArray[i]);
							string[] assetsInBundle = AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(assetBundleNamesArray[i], typeString);
							if (assetsInBundle.Length > 0)
							{
								foreach (string asset in assetsInBundle)
								{
									//sometimes this errors out if the bundle is downloaded but not LOADED
									T target = null;
									try
									{
										target = (T)AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error).m_AssetBundle.LoadAsset<T>(asset);
									}
									catch
									{
										Debug.LogWarning("SOMETHING WENT WRONG");
										var thiserror = "";
										AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out thiserror);
										if (thiserror != "" && thiserror != null)
											Debug.LogWarning("GetLoadedAssetBundle error was " + thiserror);
										else if (AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out thiserror).m_AssetBundle == null)
										{
											//The problem is here the bundle is downloaded but not LOADED
											Debug.LogWarning("Bundle was ok but m_AssetBundle was null");
										}
										else if (AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error).m_AssetBundle.LoadAsset<T>(asset) == null)
										{
											Debug.LogWarning("Load Asset screwed up T was " + typeof(T).ToString() + " asset was " + asset);
										}
									}
									if (target == null && typeof(T) == typeof(SlotDataAsset))
									{
										target = (T)AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error).m_AssetBundle.LoadAsset<T>(asset + "_Slot");
									}
									if (target != null)
									{
										assetFound = true;
										if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
										{
											assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
										}
										if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(asset))
										{
											assetBundlesUsedDict[assetBundleNamesArray[i]].Add(asset);
										}
										assetsToReturn.Add(target);
									}
									else
									{
										if (error != "" || error != null)
										{
											Debug.LogWarning(error);
										}
									}
								}
							}
						}
						else if (forceDownloadAll && downloadAssetsEnabled)//if its not downloaded but we are forcefully downloading any bundles that contain a type of asset make it download and add the temp asset to the downloading assets list
						{
							string[] assetsInBundle = AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(assetBundleNamesArray[i], typeString);
							if (assetsInBundle.Length > 0)
							{
								Debug.Log("[DynamicAssetLoader] forceDownloadAll was true for " + typeString + " and found in " + assetBundleNamesArray[i]);
								for (int aib = 0; aib < assetsInBundle.Length; aib++)
								{
									//Here we return a temp asset and wait for the bundle to download
									//We dont want to create multiple downloads of the same bundle so check its not already downloading
									if (AssetBundleManager.AreBundlesDownloading(assetBundleNamesArray[i]) == false)
									{
										LoadAssetBundle(assetBundleNamesArray[i]);
									}
									else
									{
										//do nothing its already downloading
									}
									var thisAssetName = assetsInBundle[aib];
									var thisAssetNameHash = AssetBundleManager.AssetBundleIndexObject.GetAssetHashFromName(assetBundleNamesArray[i], thisAssetName, typeString);
									T target = downloadingAssets.AddDownloadItem<T>(CurrentBatchID, thisAssetName, thisAssetNameHash, assetBundleNamesArray[i], callback/*, requestingUMA*/);
									if (target != null)
									{
										//assetFound = true;
										if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
										{
											assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
										}
										if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(thisAssetName))
										{
											assetBundlesUsedDict[assetBundleNamesArray[i]].Add(thisAssetName);
										}
										assetsToReturn.Add(target);
										/*if (assetName != "")
											break;*/
									}
								}
							}
						}
					}
				}
				if (!assetFound && assetName != "")
				{
					string[] assetIsInArray = AssetBundleManager.AssetBundleIndexObject.FindContainingAssetBundle(assetName, typeString);
					string assetIsIn = assetIsInArray.Length > 0 ? " but it was in " + assetIsInArray[0] : ". Do you need to reupload you platform manifest and index?";
					Debug.LogWarning("Dynamic" + typeof(T).Name + "Library (" + typeString + ") could not load " + assetName + " from any of the AssetBundles searched" + assetIsIn);
				}
				//AddAssetsFromAssetBundles was not sending the callback before- we only need it now to populate the dynamicCallback field
				/*if (assetsToReturn.Count > 0 && callback != null)
                {
                    callback(assetsToReturn.ToArray());
                }*/

				return assetFound;
#if UNITY_EDITOR
			}
#endif
		}

#if UNITY_EDITOR
		/// <summary>
		/// Simulates the loading of assets when AssetBundleManager is set to 'SimulationMode'
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="bundlesToSearch"></param>
		/// <param name="assetNameHash"></param>
		/// <param name="assetName"></param>
		/// <param name="callback"></param>
		bool SimulateAddAssetsFromAssetBundlesNew<T>(ref Dictionary<string, List<string>> assetBundlesUsedDict, ref List<T> assetsToReturn, string[] bundlesToSearchArray, int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
		{
			Type typeParameterType = typeof(T);
			var typeString = typeParameterType.FullName;
			int currentSimulatedDownloadedBundlesCount = simulatedDownloadedBundles.Count;
			if (assetNameHash != null)
			{
				// We could load all assets of type, iterate over them and get the hash and see if it matches...But then that would be as slow as loading from resources was
				Debug.Log("It is not currently possible to search for assetBundle assets in SimulationMode using the assetNameHash. " + typeString + " is trying to do this with assetNameHash " + assetNameHash);
			}
			string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
			string[] assetBundleNamesArray;
			if (bundlesToSearchArray.Length > 0 && bundlesToSearchArray[0] != "")
			{
				List<string> processedBundleNamesArray = new List<string>();
				for (int i = 0; i < bundlesToSearchArray.Length; i++)
				{
					for (int ii = 0; ii < allAssetBundleNames.Length; ii++)
					{
						if (allAssetBundleNames[ii].IndexOf(bundlesToSearchArray[i]) > -1 && !processedBundleNamesArray.Contains(allAssetBundleNames[ii]))
						{
							processedBundleNamesArray.Add(allAssetBundleNames[ii]);
						}
					}
				}
				assetBundleNamesArray = processedBundleNamesArray.ToArray();
			}
			else
			{
				assetBundleNamesArray = allAssetBundleNames;
			}
			bool assetFound = false;
			///a list of all the assets any assets we load depend on
			List<string> dependencies = new List<string>();
			for (int i = 0; i < assetBundleNamesArray.Length; i++)
			{
				if (assetFound && assetName != "")//Do we want to break actually? What if the user has named two overlays the same? Or would this not work anyway?
					break;
				string[] possiblePaths = new string[0];
				if (assetName != "")
				{
					//This is a compromise for the sake of speed that assumes slot/overlay/race assets have the same slotname/overlayname/racename as their actual asset
					//if we dont do this we have to load all the assets of that type and check their name which is really slow
					//I think its worth having this compromise because this does not happen when the local server is on or the assets are *actually* downloaded
					//if this is looking for SlotsDataAssets then the asset name has _Slot after it usually even if the slot name doesn't have that-but the user might have renamed it so cover both cases
					if (typeof(T) == typeof(SlotDataAsset))
					{
						string[] possiblePathsTemp = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], assetName);
						string[] possiblePaths_SlotTemp = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], assetName + "_Slot");
						List<string> possiblePathsList = new List<string>(possiblePathsTemp);
						foreach (string path in possiblePaths_SlotTemp)
						{
							if (!possiblePathsList.Contains(path))
							{
								possiblePathsList.Add(path);
							}
						}
						possiblePaths = possiblePathsList.ToArray();
					}
					else
					{
						possiblePaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], assetName);
					}
				}
				else
				{
					//if the application is not playing we want to load ALL the assets from the bundle this asset will be in
					if (!Application.isPlaying /*|| typeof(T) == typeof(UMATextRecipe)*/)//addin a check for type of UMATextRecipe jus makes this load all he textt recipes- not just the ones that should be there
					{
						possiblePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
					}
					else if (simulatedDownloadedBundles.Contains(assetBundleNamesArray[i]) || forceDownloadAll)
					{
						//DCS.Refresh calls for assets without sending a name and in reality this just checks bundles that are already downloaded
						//this mimics that behaviour
						possiblePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
					}
				}
				foreach (string path in possiblePaths)
				{
					T target = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
					if (target != null)
					{
						assetFound = true;
						if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
						{
							assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
						}
						if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(assetName))
						{
							assetBundlesUsedDict[assetBundleNamesArray[i]].Add(assetName);
						}
						assetsToReturn.Add(target);
						//Add the bundle this asset was in to the simulatedDownloadedBundles list if its not already there
						if (!simulatedDownloadedBundles.Contains(assetBundleNamesArray[i]))
							simulatedDownloadedBundles.Add(assetBundleNamesArray[i]);
						//Find the dependencies for all the assets in this bundle because AssetBundleManager would automatically download those bundles too
						//Dont bother finding dependencies when something is just trying to load all asset of type
						if (Application.isPlaying && assetName != "" && forceDownloadAll == false)
						{
							//var thisAssetBundlesAssets = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
							// for (int ii = 0; ii < thisAssetBundlesAssets.Length; ii++)
							//{
							//Actually using AssetDatabase.GetDependencies TOTALLY DESTROYS performance
							var thisDependencies = AssetDatabase.GetDependencies(path, false);
							for (int depi = 0; depi < thisDependencies.Length; depi++)
							{
								if (!dependencies.Contains(thisDependencies[depi]))
								{
									dependencies.Add(thisDependencies[depi]);
								}
							}
							//Debug.Log("[DynamicAssetLoader] asset " + assetName + " had " + dependencies.Count + " dependencies");
							//}
						}
						if (assetName != "")
							break;
					}
				}
			}
			/*if (!assetFound && assetName != "")
            {
                Debug.LogWarning("Dynamic" + typeString + "Library could not simulate the loading of " + assetName + " from any AssetBundles");
            }
            if (assetsToReturn.Count > 0 && callback != null)
            {
                callback(assetsToReturn.ToArray());
            }*/
			//LOOKS LIKE THIS MAY BE CAUSING AN INFINITE LOOP
			if (dependencies.Count > 0 && assetName != "" && forceDownloadAll == false)
			{
				//we need to load ALL the assets from every Assetbundle that has a dependency in it.
				List<string> AssetBundlesToFullyLoad = new List<string>();
				for (int i = 0; i < assetBundleNamesArray.Length; i++)
				{
					//if anything in the dependencies list is in this asset bundle AssetBundleManager would have loaded this bundle aswell
					/*for (int di = 0; di < dependencies.Count; di++)
					{
						Debug.Log("[DynamicAssetLoader] checking if " + dependencies[di] + " is in " + assetBundleNamesArray[i]);
						//check if tthis asset bundle contains that dependency
						if(AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], dependencies[di]).Length > 0)
						{
							if (!AssetBundlesToFullyLoad.Contains(assetBundleNamesArray[i]))
								AssetBundlesToFullyLoad.Add(assetBundleNamesArray[i]);
							//Add this bundle to the simulatedDownloadedBundles list if its not already there because that would have been downloaded too
							if (!simulatedDownloadedBundles.Contains(assetBundleNamesArray[i]))
								simulatedDownloadedBundles.Add(assetBundleNamesArray[i]);
							//if any dependencies are in the bundle we can sttop checking this bundle and move on
							Debug.Log("[DynamicAssetLoader] dependency " + dependencies[di] + " will cause download of " + assetBundleNamesArray[i]);
							break;
						}
					}*/
					var allAssetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
					bool processed = false;
					for (int ii = 0; ii < allAssetBundlePaths.Length; ii++)
					{
						for (int di = 0; di < dependencies.Count; di++)
						{
							if (allAssetBundlePaths[ii] == dependencies[di])
							{
								if (!AssetBundlesToFullyLoad.Contains(assetBundleNamesArray[i]))
									AssetBundlesToFullyLoad.Add(assetBundleNamesArray[i]);
								//Add this bundle to the simulatedDownloadedBundles list if its not already there because that would have been downloaded too
								if (!simulatedDownloadedBundles.Contains(assetBundleNamesArray[i]))
									simulatedDownloadedBundles.Add(assetBundleNamesArray[i]);
								processed = true;
								break;
							}
						}
						if (processed) break;
					}
				}
				//We need to add the recipes from the bundle to DCS, other assets add them selves as they are requested by the recipes
				//Actually RaceLibrary will call DCS.Refresh if it adds a new race so as long as the above assets are loaded BEFORE this happens DCS will find them
				//ACTUALLY I dont think this needs to happen- apart from maybe in the editor when the game is not running
				//It doent have anything to do with DCS though
				var thisDCS = UMAContext.Instance.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem;
				if (thisDCS != null)
				{
					foreach (string assetBundleName in AssetBundlesToFullyLoad)
					{
						//THIS DOESNT WORK
						/*if (typeof(T) == typeof(RaceData))//With DCS if a new race is added it needs to scan any new bundles for recipes for that race
						{
							if(!simulationModeDCSBundlesToUpdate.Contains(assetBundleName))
								simulationModeDCSBundlesToUpdate.Add(assetBundleName);
						}*/
						var allAssetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
						for (int ai = 0; ai < allAssetBundlePaths.Length; ai++)
						{
							UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(allAssetBundlePaths[ai]);
							if (obj.GetType() == typeof(UMATextRecipe) && typeof(T) == typeof(UMATextRecipe))
							{
								//I think the reason why this is not working right is that DCS may not have the race in its dictionary
								//thisDCS.AddRecipe(obj as UMATextRecipe);
								//doDCSSimulationRefresh = true;
								//assetsToReturn.Add(obj as T);
							}
						}
					}
					/*if (doDCSSimulationRefresh)//makes no difference to werewolf not getting initial wardrobe in simulation mode
					{
						thisDCS.Refresh();
						doDCSSimulationRefresh = false;
					}*/
				}
			}
			if (!assetFound && assetName != "")
			{
				Debug.LogWarning("Dynamic" + typeString + "Library could not simulate the loading of " + assetName + " from any AssetBundles");
			}
			if (assetsToReturn.Count > 0 && callback != null)
			{
				callback(assetsToReturn.ToArray());
			}
			//Racedata will trigger an update of DCS itself if it added a race DCS needs to know about
			//Other assets may have caused psuedo downloads of bundles DCS should check for UMATextRecipes
			//Effectively this mimics DynamicAssetLoader loadAssetBundleAsyncs call of DCS.Refresh
			//- but without loading all the assets to check if any of them are UMATextRecipes because that is too slow
			if (currentSimulatedDownloadedBundlesCount != simulatedDownloadedBundles.Count && typeof(T) != typeof(RaceData) && assetName != "")
			{
				var thisDCS = UMAContext.Instance.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem;
				if (thisDCS != null)
				{
					//but it only needs to add stuff from tthe bundles that were added
					for (int i = currentSimulatedDownloadedBundlesCount; i < simulatedDownloadedBundles.Count; i++)
					{
						thisDCS.Refresh(false, simulatedDownloadedBundles[i]);
					}
				}
			}
			return assetFound;
		}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// Mimics the check dynamicAssetLoader does when an actual LoadBundleAsync happens 
		/// where it checks if the asset has any UMATextRecipes in it and if it does makes DCS.Refresh to get them
		/// </summary>
		/// <param name="assetBundleToLoad"></param>
		/// TODO this would also download dependent bundles but AssetDatabase.GetDependencies is REALLY slow
		public void SimulateLoadAssetBundle(string assetBundleToLoad)
		{
			bool bundleAlreadySimulated = true;
			if (!simulatedDownloadedBundles.Contains(assetBundleToLoad))
			{
				simulatedDownloadedBundles.Add(assetBundleToLoad);
				bundleAlreadySimulated = false;
			}
			var allAssetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleToLoad);
			//We need to add the recipes from the bundle to DCS, other assets add them selves as they are requested by the recipes
			var thisDCS = UMAContext.Instance.dynamicCharacterSystem as UMACharacterSystem.DynamicCharacterSystem;
			bool dcsNeedsRefresh = false;
			if (thisDCS)
			{
				for (int i = 0; i < allAssetBundlePaths.Length; i++)
				{
					UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(allAssetBundlePaths[i]);
					if (obj.GetType() == typeof(UMATextRecipe))
					{
						if (bundleAlreadySimulated == false)
							dcsNeedsRefresh = true;
						break;
					}
				}
				if (dcsNeedsRefresh)
				{
					thisDCS.Refresh(false, assetBundleToLoad);
				}
			}
		}
#endif
		/// <summary>
		/// Splits the 'ResourcesFolderPath(s)' and 'AssetBundleNamesToSearch' fields up by comma if the field is using that functionality...
		/// </summary>
		/// <param name="searchString"></param>
		/// <returns></returns>
		string[] SearchStringToArray(string searchString = "")
		{
			string[] searchArray;
			if (searchString == "")
			{
				searchArray = new string[] { "" };
			}
			else
			{
				searchString.Replace(" ,", ",").Replace(", ", ",");
				if (searchString.IndexOf(",") == -1)
				{
					searchArray = new string[1] { searchString };
				}
				else
				{
					searchArray = searchString.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
				}
			}
			return searchArray;
		}

		#endregion

		#region SPECIAL TYPES
		//DownloadingAssetsList and DownloadingAssetItem moved into their own scripts to make this one a bit more manageable!        
		#endregion
	}
}
