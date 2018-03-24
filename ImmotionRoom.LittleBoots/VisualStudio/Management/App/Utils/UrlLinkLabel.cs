namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;

    /// <summary>
    /// Open a link when the user clicks on the text label
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class UrlLinkLabel : MonoBehaviour
    {
        /// <summary>
        /// Url to open when the user clicks on the label
        /// </summary>
        [Tooltip("Url to open when the user clicks on the label")]
        public string LinkUrl;

        /// <summary>
        /// Reference to rect ransform of this object
        /// </summary>
        private RectTransform m_rectTransform;

        // Use this for initialization
        void Start()
        {
            m_rectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void OnGUI()
        {
            //code from http://answers.unity3d.com/questions/21261/can-i-place-a-link-such-as-a-href-into-the-guilabe.html

            //if we have a Mouse up event on this object
            if (Event.current != null && Event.current.type == EventType.MouseUp)
            {
                //get world coordinates of this rect transform
                Vector3[] worldCorners = new Vector3[4];
                m_rectTransform.GetWorldCorners(worldCorners);

                Rect worldRect = new Rect(worldCorners[0].x,
                                          Screen.height - worldCorners[2].y,
                                          (worldCorners[2].x - worldCorners[0].x),
                                          (worldCorners[2].y - worldCorners[0].y) );

                //if the world rect contains the mouse pointer, open the link
                if (worldRect.Contains(Event.current.mousePosition))
                    Application.OpenURL(LinkUrl);
            }
                
        }
    }

}
