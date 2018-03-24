namespace ImmotionAR.ImmotionRoom.LittleBoots.VR
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Initialize a 2D canvas to be displayed in front of the eyes of the user, in a OSVR environment
    /// </summary>
    public class OSVRCalibrationCanvasInitializer : MonoBehaviour
    {

        /// <summary>
        /// Function executed at script startup 
        /// </summary>
        void Start()
        {
            //put object in front of user's eyes, then scale it so it shows with appropriate dimensions
            //(otherwise, in front of the eyes we would not see anything)
            gameObject.transform.SetParent(GameObject.Find("VRDisplayTracked/VRViewer0").transform, false);
            transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            transform.localPosition = new Vector3(0.0f, 0.0f, 0.21f);
        }

    }

}
