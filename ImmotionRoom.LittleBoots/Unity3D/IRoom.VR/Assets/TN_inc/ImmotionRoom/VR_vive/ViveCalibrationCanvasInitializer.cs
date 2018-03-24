namespace ImmotionAR.ImmotionRoom.LittleBoots.VR
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Initialize a 2D canvas to be displayed in front of the eyes of the user, in a Vive environment
    /// </summary>
    public class ViveCalibrationCanvasInitializer : MonoBehaviour
    {

        /// <summary>
        /// Function executed at script startup 
        /// </summary>
        void Start()
        {
            //put object in front of user's eyes, then scale it so it shows with appropriate dimensions
            //(otherwise, in front of the eyes we would not see anything)
            gameObject.transform.SetParent(GameObject.Find("Camera (head)/Camera (eye)").transform, false);
            RectTransform r = gameObject.GetComponent<RectTransform>();
            r.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);
            r.localPosition = new Vector3(0.01f, 0.01f, 0.21f);
            r.localEulerAngles = Vector3.zero;
        }

    }

}
