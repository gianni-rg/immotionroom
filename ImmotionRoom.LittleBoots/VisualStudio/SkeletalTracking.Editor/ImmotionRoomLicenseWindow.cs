namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Editor
{
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Editor window showing plugin licensing
    /// </summary>
    public class ImmotionRoomLicenseWindow : EditorWindow
    {
        /// <summary>
        /// Logo of ImmotionRoom SDK
        /// </summary>
        Texture2D m_immotionRoomLogo;

        // Add menu item named "About" to the ImmotionRoom menu
        [MenuItem("ImmotionRoom/About...")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(ImmotionRoomLicenseWindow));          
        }

        void OnEnable()
        {
            //set gui stuff
            titleContent.text = "About";
            m_immotionRoomLogo = Resources.Load<Texture2D>("ImmotionRoomSdkLogo");

            //set win size
            maxSize = minSize = new Vector2(360, 300);
        }

        void OnGUI()
        {
            //show current version, logo and copyright
            GUILayout.Label("About ImmotionRoom\n", EditorStyles.boldLabel);
            GUILayout.Label(m_immotionRoomLogo);
            GUILayout.Label("ImmotionRoom SDK v" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            GUILayout.Label("by ImmotionAR");
            GUILayout.Label("\nCopyright (c) 2017-2018 Gianni Rosa Gallina.\nCopyright (c) 2014-2017 ImmotionAR.");

            //show licensing
            var boldNoMarginStyle = new GUIStyle(EditorStyles.boldLabel);
            boldNoMarginStyle.alignment = TextAnchor.UpperLeft;
            boldNoMarginStyle.margin = new RectOffset(0, 0, 2, 0);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(250));
            GUILayout.Label("License type: ");
            GUILayout.Label("GNU General Public License v3", boldNoMarginStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(250));
            GUILayout.Label("See LICENSE file for more information. ");
            GUILayout.EndHorizontal();

            //show awesome message
            GUILayout.Label("\nThank you for supporting our VR dream! You're awesome!", EditorStyles.miniLabel);
        }
    }
}
