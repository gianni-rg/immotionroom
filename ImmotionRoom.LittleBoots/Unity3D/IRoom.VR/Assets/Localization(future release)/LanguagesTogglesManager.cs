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
namespace ImmotionAR.ImmotionRoom.LittleBoots.IRoom.VR
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization;

    /// <summary>
    /// Handles Toggles to choose language in Language choosing scene
    /// </summary>
    [RequireComponent(typeof(LanguageChooser))]
    public class LanguagesTogglesManager : MonoBehaviour
    {
        /// <summary>
        /// Called when English Toggle changes its value
        /// </summary>
        /// <param name="newValue">New value</param>
        public void EnglishToggleHandler(bool newValue)
        {
            if (newValue == true)
                GetComponent<LanguageChooser>().ChangeLanguage(ManagedLanguage.enUS);
        }

        /// <summary>
        /// Called when Italian Toggle changes its value
        /// </summary>
        /// <param name="newValue">New value</param>
        public void ItalianToggleHandler(bool newValue)
        {
            if (newValue == true)
                GetComponent<LanguageChooser>().ChangeLanguage(ManagedLanguage.itIT);
        }
    }

}