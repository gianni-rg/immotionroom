﻿/************************************************************************************************************
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
    /// Defines menu item and behaviour to apply when building ImmotionRoom for flat 2D Screen setup
    /// </summary>
    public class MultibuildMenu_PlainCamera : Editor
    {

        /// <summary>
        /// Actions to apply when building ImmotionRoom for a Flat 2D Screen setup
        /// </summary>
        [MenuItem("ImmotionRoom/Platform Settings/Flat PC Screen", false, 1)]
        public static void BuildForPC()
        {
            //init for PC & disable VR
            MultibuildHelpers.InitVRForTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "None", false);

            //use a simple camera
            MultibuildHelpers.DoStandardInitOnPlayerController<DummyHmdManager>("PlainScreenCamera");
        }

        /// <summary>
        /// Actions to apply when building ImmotionRoom for a Flat 2D Screen setup
        /// </summary>
        [MenuItem("ImmotionRoom/Platform Settings/Flat Android Screen", false, 2)]
        public static void BuildForAndroid()
        {
            //init for Android & disable VR
            MultibuildHelpers.InitVRForTarget(BuildTargetGroup.Android, BuildTarget.Android, "None", false);

            //use a simple camera
            MultibuildHelpers.DoStandardInitOnPlayerController<DummyHmdManager>("PlainScreenCamera");
        }

    }

}