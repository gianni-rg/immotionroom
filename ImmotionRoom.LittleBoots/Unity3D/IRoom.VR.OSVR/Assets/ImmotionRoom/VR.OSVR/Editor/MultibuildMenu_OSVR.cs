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
    /// Defines menu item and behaviour to apply when building ImmotionRoom for OSVR setup
    /// </summary>
    public class MultibuildMenu_OSVR : Editor
    {

        /// <summary>
        /// Actions to apply when building ImmotionRoom for OSVR
        /// </summary>
        [MenuItem("ImmotionRoom/Platform Settings/OSVR", false, 151)]
        public static void BuildForPC()
        {
            //init for PC & disable VR (OSVR works this way)
            MultibuildHelpers.InitVRForTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, "None", false);

            //use OSVR camera
            MultibuildHelpers.DoStandardInitOnPlayerController<OSVRHmdManager>("VRDisplayTracked");
        }

    }

}