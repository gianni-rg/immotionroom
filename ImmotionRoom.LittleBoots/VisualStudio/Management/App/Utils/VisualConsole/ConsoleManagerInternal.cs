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
        /// <summary>
        /// Contains the actual definition of ConsoleManager, for obfuscation purposes
        /// </summary>
        private class ConsoleManagerInternal
        {

            #region Private fields

            /// <summary>
            /// The Console Manager that contains this object
            /// </summary>
            private ConsoleManager m_consoleManager;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="consoleManager">Enclosing instance, whose code has to be implemented</param>
            internal ConsoleManagerInternal(ConsoleManager consoleManager)
            {
                m_consoleManager = consoleManager;
            }

            #endregion

            #region Behavior methods

            internal void Awake()
            {
                //remove all lines added into the editor 
                Transform linesRootTransform = m_consoleManager.transform.GetChild(0);

                foreach (Transform consoleLine in linesRootTransform)
                    if (consoleLine.GetInstanceID() != linesRootTransform.GetInstanceID())
                        Destroy(consoleLine.gameObject);
            }

            internal void Start()
            {
                //if font is null, use default Arial font
                m_consoleManager.ConsoleFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            #endregion

            #region Lines management methods

            /// <summary>
            /// Write a Information string onto the console
            /// </summary>
            /// <param name="text">Text to write</param>
            internal void WriteInfoString(string text)
            {
                //write a line using info color
                AddConsoleLineOrLines(text, m_consoleManager.InfoLinesColor);
            }

            /// <summary>
            /// Write a Highlighted Information string onto the console
            /// </summary>
            /// <param name="text">Text to write</param>
            internal void WriteHighlightInfoString(string text)
            {
                //write a line using info color
                AddConsoleLineOrLines(text, m_consoleManager.HighlightedInfoLinesColor);
            }

            /// <summary>
            /// Write a Error string onto the console
            /// </summary>
            /// <param name="text">Text to write</param>
            internal void WriteErrorString(string text)
            {
                //write a line using info color
                AddConsoleLineOrLines(text, m_consoleManager.ErrorLinesColor);
            }

            /// <summary>
            /// Add a single console line to the current console panel.
            /// The line may contain new line characters, so the line may actually be a bunch of lines
            /// </summary>
            /// <param name="text">Text string to write</param>
            /// <param name="color">Color of the writing</param>
            private void AddConsoleLineOrLines(string text, Color color)
            {
                //split the string using new line characters as separator 
                string[] lines = text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

                //call the internal writing function for each line
                foreach (string line in lines)
                    AddConsoleLine(line, color);

                //remove lines if we have reached the limit
                Transform linesRootTransform = m_consoleManager.transform.GetChild(0);
                int toDeleteLines = Mathf.Max(0, linesRootTransform.childCount - m_consoleManager.MaximumLinesNum);

                for (int i = 0; i < toDeleteLines; i++)
                    Destroy(linesRootTransform.GetChild(i).gameObject);
            }

            /// <summary>
            /// Add a single console line to the current console panel
            /// </summary>
            /// <param name="text">Text string to write. It must not contain new line characters</param>
            /// <param name="color">Color of the writing</param>
            private void AddConsoleLine(string text, Color color)
            {
                Transform linesRootTransform = m_consoleManager.transform.GetChild(0);

                //add current line
                GameObject lineGo = new GameObject("Console line");
                Text lineText = lineGo.AddComponent<Text>();
                lineText.font = m_consoleManager.ConsoleFont;
                lineText.text = text;
                lineText.fontSize = m_consoleManager.FontSize;
                lineText.resizeTextForBestFit = m_consoleManager.BestFitText;
                lineText.resizeTextMinSize = m_consoleManager.MinSize;
                lineText.resizeTextMaxSize = m_consoleManager.MaxSize;
                lineText.horizontalOverflow = HorizontalWrapMode.Wrap;
                lineText.verticalOverflow = VerticalWrapMode.Overflow;
                lineText.color = color;
                lineGo.transform.SetParent(linesRootTransform, false);
            }

            #endregion

        }
    }

}
