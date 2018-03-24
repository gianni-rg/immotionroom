namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Editor.Multibuild
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditorInternal;
    using PlayerController;
    using HeadsetManagement;

    /// <summary>
    /// Helper methods to perform multi build configurations, i.e. fast configuration switches between different headsets
    /// </summary>
    public static class MultibuildHelpers
    {
        #region Public methods

        /// <summary>
        /// Performs initialization to build for a particular VR target.
        /// The method will switch the build platform (this may require re-import of all assets!), set the Virtual Reality Supported flag and add the required VR SDK as top VR sdk in the list
        /// To see the changes, a refresh of the inspector window may be required
        /// </summary>
        /// <param name="buildTargetGroup">Build target group of current configuration</param>
        /// <param name="buildTarget">Build target of current configuration</param>
        /// <param name="vrTarget">VR Target string of current headset (e.g. "Oculus")</param>
        /// <param name="vrEnabled">True to enable VR, false to disable it</param>
        public static void InitVRForTarget(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, string vrTarget, bool vrEnabled = true)
        {
            //enable-disable VR
            UnityEditorInternal.VR.VREditor.SetVREnabledOnTargetGroup(BuildTargetGroup.Standalone, vrEnabled);

            //add the requested vr target to the VR SDK list as put it at first place
            SetFirstVRTarget(buildTargetGroup, vrTarget);

            //change build target (may require re-import)
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

            //refresh all views
            InternalEditorUtility.RepaintAllViews();
        }

        /// <summary>
        /// Instantiate a prefab by name and put it as child of the provided parent.
        /// If no prefab gets found with that name, the call produce no results
        /// If multiple prefab exist with that name, the first found one will be considered
        /// </summary>
        /// <param name="name">Name of the prefab</param>
        /// <param name="parent">Parent to which add the object as child</param>
        public static void InstantiatePrefab(string name, GameObject parent)
        {
            //find all prefabs with that name
            var foundPrefabs = FindAssetsByName(name);

            //if we found one, instantiate it and connect it to the prefab instance (this doesn't happen by default)
            //set the parent of the new object as requested
            if (foundPrefabs.Count > 0)
            {
                GameObject createdObject = PrefabUtility.ConnectGameObjectToPrefab(Object.Instantiate<GameObject>(foundPrefabs[0]), foundPrefabs[0]);
                createdObject.transform.SetParent(parent.transform, false);
                createdObject.transform.SetAsFirstSibling(); //put it as first element in the list, so we always know that cameras are first child in list
            }
        }

        /// <summary>
        /// Handy method to do a standard initialization on current player controller for the headset of interest
        /// </summary>
        /// <typeparam name="HeadsetManagerScriptType">Type of script that manages the headset</typeparam>
        /// <param name="vrCameraPrefabName">Name of the prefab of the VR camera for the headset</param>
        public static void DoStandardInitOnPlayerController<HeadsetManagerScriptType>(string vrCameraPrefabName) where HeadsetManagerScriptType : HeadsetManager
        {
            //gets player controller inside the scene
            IroomPlayerController playerController = Object.FindObjectOfType<IroomPlayerController>();

            //if it exists
            if (playerController != null)
            {
                //replace current headset manager with the one of the headset of interest
                HeadsetManager currentManager = playerController.gameObject.GetComponent<HeadsetManager>();
                playerController.gameObject.AddComponent<HeadsetManagerScriptType>();
                Object.DestroyImmediate(currentManager);

                //get reference to the Headset frame of reference of current headset
                Transform headsetFrameOfReference = playerController.transform.GetChild(1);

                //Destroy first children (the current headset camera)
                Object.DestroyImmediate(headsetFrameOfReference.GetChild(0).gameObject);

                //put the VR camera of current headset
                MultibuildHelpers.InstantiatePrefab(vrCameraPrefabName, headsetFrameOfReference.gameObject);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Set the provided VR target as the top-priority Virtual Reality SDK in the Player Settings.
        /// To see the changes, a refresh of the inspector window may be required
        /// </summary>
        /// <param name="buildTargetGroup">Build target group</param>
        /// <param name="vrTarget">ID String of the VR target to build (e.g. "Oculus")</param>
        private static void SetFirstVRTarget(BuildTargetGroup buildTargetGroup, string vrTarget)
        {
            //get current vr devices
            string[] currentVrDevices = UnityEditorInternal.VR.VREditor.GetVREnabledDevicesOnTargetGroup(buildTargetGroup);

            //copy current vr devices list, setting/adding the provided one as first one
            List<string> vrDevices = new List<string>();

            vrDevices.Add(vrTarget);

            foreach (string vrDevice in vrDevices)
            {
                if (vrDevice != vrTarget)
                {
                    vrDevices.Add(vrDevice);
                }
            }

            //set the new list as the correct list for VR targets
            UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(buildTargetGroup, vrDevices.ToArray());
        }

        /// <summary>
        /// Finds all the prefabs with the provided name
        /// </summary>
        /// <param name="name">Name of the prefabs to find</param>
        /// <returns>List of prefabs with the provided name, loaded from the Asset Database</returns>
        private static List<GameObject> FindAssetsByName(string name)
        {
            //code from http://answers.unity3d.com/questions/486545/getting-all-assets-of-the-specified-type.html

            List<GameObject> assets = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets(name);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            
            return assets;
        }

        #endregion
    }
}
