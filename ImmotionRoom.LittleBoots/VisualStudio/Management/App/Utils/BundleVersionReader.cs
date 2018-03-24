namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;

    /// <summary>
    /// Read the bundle version of the program, and adds it at the end of the Text element of the same gameobject
    /// </summary>
    public class BundleVersionReader : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            Text currentText = GetComponent<Text>();
            currentText.text += " v" + Application.version;
        }

    }

}
