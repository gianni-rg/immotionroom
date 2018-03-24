namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.VisualConsole
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.UI;

    /// <summary>
    /// Manages a visual console for the management app
    /// </summary>
    public partial class ConsoleManager : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// Font to use for the console lines
        /// </summary>
        [Tooltip("Font to use for the console lines")]
        public Font ConsoleFont;

        /// <summary>
        /// Font size of the console lines
        /// </summary>
        [Tooltip("Font size of the console lines")]
        public int FontSize = 14;

        /// <summary>
        /// Minimum allowed size for the console lines
        /// </summary>
        [Tooltip("Minimum allowed size for the console lines")]
        public int MinSize = 8;

        /// <summary>
        /// Maximum allowed size for the console lines
        /// </summary>
        [Tooltip("Maximum allowed size for the console lines")]
        public int MaxSize = 18;

        /// <summary>
        /// True to use Best Fit behaviour, false otherwise
        /// </summary>
        [Tooltip("True to use Best Fit behaviour, false otherwise")]
        public bool BestFitText = true;

        /// <summary>
        /// Color for Info lines to be written onto the console
        /// </summary>
        [Tooltip("Color for Info lines to be written onto the console")]
        public Color InfoLinesColor = Color.white;

        /// <summary>
        /// Color for highlighted Info lines to be written onto the console
        /// </summary>
        [Tooltip("Color for Highlighted Info lines to be written onto the console")]
        public Color HighlightedInfoLinesColor = new Color(0, 128 / 255.0f, 255 / 255.0f);

        /// <summary>
        /// Color for Error lines to be written onto the console
        /// </summary>
        [Tooltip("Color for Error lines to be written onto the console")]
        public Color ErrorLinesColor = Color.red;

        /// <summary>
        /// Maximum lines allowed inside this console. Above this number, older lines gets deleted
        /// </summary>
        [Tooltip("Maximum lines allowed inside this console. Above this number, older lines gets deleted")]
        public int MaximumLinesNum = 11;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        ConsoleManagerInternal m_internalImplementation;

        #endregion

        #region Behavior methods

        void Awake()
        {
            m_internalImplementation = new ConsoleManagerInternal(this);
            m_internalImplementation.Awake();
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        //uncomment for debug purposes
        //void Update()
        //{
        //    if(Input.GetKeyDown(KeyCode.H))
        //        WriteHighlightInfoString("Ciao");
        //    if (Input.GetKeyDown(KeyCode.I))
        //        WriteInfoString("Come stai\nIo bene");
        //    if (Input.GetKeyDown(KeyCode.E))
        //        WriteErrorString("Nonononono");
        //}

        #endregion

        #region Lines management methods

        /// <summary>
        /// Write a Information string onto the console
        /// </summary>
        /// <param name="text">Text to write</param>
        public void WriteInfoString(string text)
        {
            m_internalImplementation.WriteInfoString(text);
        }

        /// <summary>
        /// Write a Highlighted Information string onto the console
        /// </summary>
        /// <param name="text">Text to write</param>
        public void WriteHighlightInfoString(string text)
        {
            m_internalImplementation.WriteHighlightInfoString(text);
        }

        /// <summary>
        /// Write a Error string onto the console
        /// </summary>
        /// <param name="text">Text to write</param>
        public void WriteErrorString(string text)
        {
            m_internalImplementation.WriteErrorString(text);
        }

        #endregion
    }

}
