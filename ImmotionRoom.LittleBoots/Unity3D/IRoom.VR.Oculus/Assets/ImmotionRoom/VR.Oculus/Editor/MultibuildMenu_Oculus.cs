/************************************************************************************************************
 * 
 * Copyright (C) 2014-2016 ImmotionAR, a division of Beps Engineering. All rights reserved.
 * 
 * Licensed under the ImmotionAR ImmotionRoom SDK License (the "License");
 * you may not use the ImmotionAR ImmotionRoom SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 * 
 * You may obtain a copy of the License at
 * 
 * http://www.immotionar.com/legal/ImmotionRoomSDKLicense.PDF
 * 
 ************************************************************************************************************/
namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Editor.Multibuild
{
    using UnityEditor;
    using HeadsetManagement;

    /// <summary>
    /// Defines menu item and behaviour to apply when building ImmotionRoom for Oculus setup
    /// </summary>
    public class MultibuildMenu_Oculus : Editor
    {

        /// <summary>
        /// Actions to apply when building ImmotionRoom for Oculus Rift
        /// </summary>
        [MenuItem("ImmotionRoom/Platform Settings/Oculus Rift", false, 51)]
        public static void BuildForPC()
        {
            //init for PC & enable VR
            MultibuildHelpers.InitVRForTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "Oculus");

            //use OVR camera
            MultibuildHelpers.DoStandardInitOnPlayerController<OculusHmdManager>("OVRCameraRig");
        }

        /// <summary>
        /// Actions to apply when building ImmotionRoom for Samsung Gear VR
        /// </summary>
        [MenuItem("ImmotionRoom/Platform Settings/Samsung Gear VR", false, 52)]
        public static void BuildForAndroid()
        {
            //init for Android & enable VR
            MultibuildHelpers.InitVRForTarget(BuildTargetGroup.Android, BuildTarget.Android, "Oculus");

            //use OVR camera
            MultibuildHelpers.DoStandardInitOnPlayerController<OculusHmdManager>("OVRCameraRig");
        }

    }

}