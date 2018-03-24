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
    /// Defines menu item and behaviour to apply when building ImmotionRoom for SteamVR setup
    /// </summary>
    public class MultibuildMenu_Vive : Editor
    {

        /// <summary>
        /// Actions to apply when building ImmotionRoom for SteamVR headsets
        /// </summary>
        [MenuItem("ImmotionRoom/Platform Settings/SteamVR (Vive)", false, 101)]
        public static void BuildForPC()
        {
            //init for PC & enable VR
            MultibuildHelpers.InitVRForTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "OpenVR");

            //use Vive camera
            MultibuildHelpers.DoStandardInitOnPlayerController<ViveHmdManager>("[CameraRig]");
        }

    }

}