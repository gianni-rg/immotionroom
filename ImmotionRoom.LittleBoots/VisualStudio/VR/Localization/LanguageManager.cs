namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization
{
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Manages Localization of VR package strings.
    /// This class is singleton
    /// </summary>
    public sealed class LanguageManager
    {
        #region Public Events and Delegates

        /// <summary>
        /// Delegates of operations of changing language of a <see cref="LanguageManager"/> object
        /// </summary>
        /// <param name="eventArgs">Result of discovering operation</param>
        public delegate void LanguageManagerChangedHandler(LanguageManagerChangedEventArgs eventArgs);

        /// <summary>
        /// Event triggered when <see cref="LanguageManager"/> current language changes
        /// </summary>
        public event LanguageManagerChangedHandler LanguageManagerChanged;

        #endregion

        #region Private fields

        /// <summary>
        /// Dictionary that, for each language, stores the dictionary of translated strings.
        /// The dictionary of translated strings, holds, for each key string, a particular sentence in the language it refers to
        /// </summary>
        Dictionary<ManagedLanguage, Dictionary<string, string>> m_languageStrings;

        /// <summary>
        /// Current language selected by the user
        /// </summary>
        ManagedLanguage m_currentLanguage;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get or set current language of the system
        /// </summary>
        public ManagedLanguage CurrentLanguage
        {
            get
            {
                return m_currentLanguage;
            }
            set
            {
                if(m_currentLanguage != value)
                {
                    m_currentLanguage = value;

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("LanguageManager - changed language. Language is now {0}", m_currentLanguage.ToString());
                    }

                    //trigger the event
                    if(LanguageManagerChanged != null)
                    {
                        var copiedEvent = LanguageManagerChanged;

                        copiedEvent(new LanguageManagerChangedEventArgs() { NewCurrentLanguage = value });
                    }
                }
                
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Init the Manager with all the strings.
        /// TODO: this strings are to be read from a file, can't be hardcoded
        /// </summary>
        private void Init()
        {
            //create root structure
            m_languageStrings = new Dictionary<ManagedLanguage, Dictionary<string, string>>();

            //create all English translations
            m_languageStrings[ManagedLanguage.enUS] = new Dictionary<string, string>();
            m_languageStrings[ManagedLanguage.enUS]["InitCompleted"] = "COMPLETED";
	        m_languageStrings[ManagedLanguage.enUS]["Initialization"] = "INITIALIZATION";
	        m_languageStrings[ManagedLanguage.enUS]["InitInPlaceRotationString"] = "ROTATE IN-PLACE TO MAKE SENSORS SEE YOU";
	        m_languageStrings[ManagedLanguage.enUS]["InitNotDetected"] = "NOT DETECTED";
	        m_languageStrings[ManagedLanguage.enUS]["InitNowStop"] = "NOW STOP!";
	        m_languageStrings[ManagedLanguage.enUS]["InitOk"] = "VERY GOOD!";
	        m_languageStrings[ManagedLanguage.enUS]["InitReadyToUse"] = "NOW YOU ARE READY TO USE THE SYSTEM";
	        m_languageStrings[ManagedLanguage.enUS]["InitRotatLeft"] = "TURN LEFT";
	        m_languageStrings[ManagedLanguage.enUS]["InitRotatRight"] = "TURN RIGHT";
	        m_languageStrings[ManagedLanguage.enUS]["InitRotatString"] = "NOW ROTATE UNTIL YOU RETURN TO INITIAL ORIENTATION";
	        m_languageStrings[ManagedLanguage.enUS]["InitStayStraight"] = "STAND PERFECTLY STRAIGHT";
	        m_languageStrings[ManagedLanguage.enUS]["InitStep"] = "STEP";
	        m_languageStrings[ManagedLanguage.enUS]["NoConnection"] = " ";
	        m_languageStrings[ManagedLanguage.enUS]["Wait"] = "PLEASE WAIT...";

            //create all Italian translations
            m_languageStrings[ManagedLanguage.itIT] = new Dictionary<string, string>();
            m_languageStrings[ManagedLanguage.itIT]["InitCompleted"] = "COMPLETA";
	        m_languageStrings[ManagedLanguage.itIT]["Initialization"] = "INIZIALIZZAZIONE";
	        m_languageStrings[ManagedLanguage.itIT]["InitInPlaceRotationString"] = "GIRA SU TE STESSO PER FARTI VEDERE DAI SENSORI ";
	        m_languageStrings[ManagedLanguage.itIT]["InitNotDetected"] = "NON RILEVATA";
	        m_languageStrings[ManagedLanguage.itIT]["InitNowStop"] = "ADESSO FERMATI!";
	        m_languageStrings[ManagedLanguage.itIT]["InitOk"] = "OTTIMO!";
	        m_languageStrings[ManagedLanguage.itIT]["InitReadyToUse"] = "ORA SEI PRONTO PER USARE IL SISTEMA";
	        m_languageStrings[ManagedLanguage.itIT]["InitRotatLeft"] = "GIRA VERSO SINISTRA";
	        m_languageStrings[ManagedLanguage.itIT]["InitRotatRight"] = "GIRA VERSO DESTRA";
	        m_languageStrings[ManagedLanguage.itIT]["InitRotatString"] = "ORA GIRA FINCHE' NON SEI DI NUOVO NELLA POSIZIONE DI PARTENZA";
	        m_languageStrings[ManagedLanguage.itIT]["InitStayStraight"] = "STAI SULL'ATTENTI PER QUALCHE ISTANTE";
	        m_languageStrings[ManagedLanguage.itIT]["InitStep"] = "FASE";
	        m_languageStrings[ManagedLanguage.itIT]["NoConnection"] = " ";
	        m_languageStrings[ManagedLanguage.itIT]["Wait"] = "ATTENDI...";

            //init language to english
            m_currentLanguage = ManagedLanguage.enUS;

            if(Log.IsDebugEnabled)
            {
                Log.Debug("LanguageManager - initialized. Language is {0}", m_currentLanguage.ToString());
            }
        }

        #endregion

        #region Public string localization methods

        /// <summary>
        /// Returns if the manager contains a string corresponding to the given key for current language
        /// </summary>
        /// <param name="key">Key corresponding to the string of interest</param>
        /// <returns>True if such a string is contained, false otherwise</returns>
        public bool HasLocalizedString(string key)
        {
            return HasLocalizedStringPrivate(key);
        }

        /// <summary>
        /// Returns the string corresponding to the given key for current language.
        /// If such string does not exists, a default string is returned
        /// </summary>
        /// <param name="key">Key corresponding to the string of interest</param>
        /// <param name="defaultString">String to return if the key is not present in the dictionary</param>
        /// <returns>String corresponding to the key of interest for current language, or a default string if such string does not exists</returns>
        public string GetLocalizedString(string key, string defaultString = "<MISSING TRANSLATION>")
        {
            return GetLocalizedStringPrivate(key, defaultString);
        }

        #endregion

        #region Private string localization methods

        /// <summary>
        /// Returns if the manager contains a string corresponding to the given key for current language
        /// </summary>
        /// <param name="key">Key corresponding to the string of interest</param>
        /// <returns>True if such a string is contained, false otherwise</returns>
        private bool HasLocalizedStringPrivate(string key)
        {
            return m_languageStrings[m_currentLanguage].ContainsKey(key);
        }

        /// <summary>
        /// Returns the string corresponding to the given key for current language.
        /// If such string does not exists, a default string is returned
        /// </summary>
        /// <param name="key">Key corresponding to the string of interest</param>
        /// <param name="defaultString">String to return if the key is not present in the dictionary</param>
        /// <returns>String corresponding to the key of interest for current language, or a default string if such string does not exists</returns>
        private string GetLocalizedStringPrivate(string key, string defaultString)
        {
            if (HasLocalizedString(key))
                return m_languageStrings[m_currentLanguage][key];
            else
                return defaultString;
        }

        #endregion

        #region Singleton Pattern

        //Singleton code from http://www.yoda.arachsys.com/csharp/singleton.html

        /// <summary>
        /// Private constructor, to block creation from outside.
        /// It will called by the nested class
        /// </summary>
        private LanguageManager()
        {
            this.Init();
        }

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static LanguageManager Instance
        {
            get
            {
                return NestedLManager.instance;
            }
        }
    
        /// <summary>
        /// Nested class, that will create the instance. Refer to the provided link to understand why this class is necessary
        /// </summary>
        private class NestedLManager
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedLManager()
            {
            }

            /// <summary>
            /// Get instance of enclosing class
            /// </summary>
            internal static readonly LanguageManager instance = new LanguageManager();
        }

        #endregion
    }

    /// <summary>
    /// Defines event arguments for event of language manager current language change
    /// </summary>
    public class LanguageManagerChangedEventArgs : EventArgs
    {
        /// <summary>
        /// New language set on the manager
        /// </summary>
        public ManagedLanguage NewCurrentLanguage { get; set; }
    }
}
