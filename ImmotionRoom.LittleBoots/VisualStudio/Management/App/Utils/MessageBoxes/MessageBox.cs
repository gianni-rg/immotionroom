namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.UI;

    /// <summary>
    /// Handles Message Boxes visualization in the Manager App
    /// </summary>
    public class MessageBox
    {
        #region Constants

        /// <summary>
        /// Name of the prefab of the Message boxes
        /// </summary>
        const string MessageBoxPrefabName = "Message Box";

        #endregion

        #region Public Message Box methods

        /// <summary>
        /// Shows a message box onto current screen
        /// </summary>
        /// <param name="header">Header of the message box, written in red</param>
        /// <param name="message">Message of the message, written in whitish-pink</param>
        /// <param name="onClickAction">Delegate to be called when the OK button of the message box gets clicked</param>
        /// <param name="toDisableButtons">Buttons that have to be disabled while the pop up is on</param>
        public static void Show(string header, string message, UnityEngine.Events.UnityAction onClickAction, Selectable[] toDisableButtons = null)
        {
            //call the more generic method, passing default colors for title and body
            Show(header, message, onClickAction, Color.red, new Color(1.0f, 184 / 255.0f, 184 / 255.0f), toDisableButtons);
        }

        /// <summary>
        /// Shows a message box onto current screen
        /// </summary>
        /// <param name="header">Header of the message box, written in red</param>
        /// <param name="message">Message of the message, written in whitish-pink</param>
        /// <param name="onClickAction">Delegate to be called when the OK button of the message box gets clicked</param>
        /// <param name="toDisableButtons">Buttons that have to be disabled while the pop up is on</param>
        /// <param name="headerColor">The color to write the header with</param>
        /// <param name="bodyColor">The color to write the body with</param>
        public static void Show(string header, string message, UnityEngine.Events.UnityAction onClickAction, Color headerColor, Color bodyColor,
                                Selectable[] toDisableButtons = null)
        {
            ShowPrivate(header, message, onClickAction, headerColor, bodyColor, toDisableButtons);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Shows a message box onto current screen
        /// </summary>
        /// <param name="header">Header of the message box, written in red</param>
        /// <param name="message">Message of the message, written in whitish-pink</param>
        /// <param name="onClickAction">Delegate to be called when the OK button of the message box gets clicked</param>
        /// <param name="toDisableButtons">Buttons that have to be disabled while the pop up is on</param>
        /// <param name="headerColor">The color to write the header with</param>
        /// <param name="bodyColor">The color to write the body with</param>
        private static void ShowPrivate(string header, string message, UnityEngine.Events.UnityAction onClickAction, Color headerColor, Color bodyColor,
                                        Selectable[] toDisableButtons = null)
        {
            //create the message box
            GameObject messageBoxGo = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>(MessageBoxPrefabName));

            //assign the title and the message
            messageBoxGo.transform.GetChild(0).Find("Title").GetComponent<Text>().text = header;
            messageBoxGo.transform.GetChild(0).Find("Title").GetComponent<Text>().color = headerColor;
            messageBoxGo.transform.GetChild(0).Find("Body").GetComponent<Text>().text = message;
            messageBoxGo.transform.GetChild(0).Find("Body").GetComponent<Text>().color = bodyColor;

            //disable the buttons of the main window
            if (toDisableButtons != null)
                foreach (Selectable button in toDisableButtons)
                    button.interactable = false;

            //assign the event handler. Notice that we also add an event handler to kill the box whenever OK gets pressed
            Button okButton = messageBoxGo.transform.GetChild(0).Find("OK Buttons Panel").Find("OK Button").GetComponent<Button>();
            okButton.onClick.AddListener(() =>
            {
                //destroy the message box
                Object.Destroy(messageBoxGo);

                //re-enable the disabled button
                if (toDisableButtons != null)
                    foreach (Selectable button in toDisableButtons)
                        button.interactable = true;
            });
            okButton.onClick.AddListener(onClickAction);
        }

        #endregion
    }

}