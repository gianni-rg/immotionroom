namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Girello
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Globalization;
    using System.IO;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using System;

    /// <summary>
    /// Draws a bounding box around the virtual player to let him know where the physical boundaries of the game area are.
    /// The more the player will be near its boundaries, the more the box will become opaque
    /// </summary>
    public partial class TrackingServiceVirtualGirello : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual implementation of the TrackingServiceVirtualGirello, for obfuscation purposes
        /// </summary>
        private class TrackingServiceVirtualGirelloInternal
        {
            #region Private fields

            /// <summary>
            /// Bounds of the bounding box of the physical play area (aligned with tracking boxes world reference frame)
            /// </summary>
            private Bounds m_girelloBounds;

            /// <summary>
            /// Inner bounds of the physicalPlayArea (aligned with tracking boxes world reference frame)
            /// </summary>
            private Bounds m_innerBounds;

            /// <summary>
            /// The last sides of the girello box that the players is risking to surpass
            /// </summary>
            private GirelloLimitType[] m_lastTrespassingBoundaries;

            /// <summary>
            /// Reference to the material used by the arrows
            /// </summary>
            private Material m_arrowsMaterial;

            /// <summary>
            /// Game main camera
            /// </summary>
            private GameObject m_mainCamera;

            /// <summary>
            /// The TrackingServiceVirtualGirello object that contains this object
            /// </summary>
            private TrackingServiceVirtualGirello m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal TrackingServiceVirtualGirelloInternal(TrackingServiceVirtualGirello enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            // Use this for initialization
            internal void Start()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Virtual girello - started");
                }

                Collider inspectorPlayer = m_enclosingInstance.Player;
                m_enclosingInstance.Player = null;

                m_enclosingInstance.StartCoroutine(InitGirello(inspectorPlayer));       
            }

            internal void OnDestroy()
            {
                m_enclosingInstance.StopAllCoroutines(); //stop init, if still waiting for a tracking service manager
            }

            // Update is called once per frame
            internal void Update()
            {
                //if there is a player to track
                if (m_enclosingInstance.Player != null)
                {
                    //get reference to main camera
                    m_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

                    //calculate distance of player towards all the girello bounds... and get which sides the player is near to trespassing
                    //(i.e. the boundaries for which the player is outside the inner bounds)
                    Dictionary<GirelloLimitType, float> playerDistances;

                    FindPlayerDistances(out playerDistances, out m_lastTrespassingBoundaries);

                    //update the bounding box of the Girello, drawing it in the 3D world
                    UpdateBox(playerDistances, m_lastTrespassingBoundaries);

                    //update the arrows
                    if (m_enclosingInstance.DrawArrows)
                        UpdateArrows(playerDistances, m_lastTrespassingBoundaries);
                }

            }

            internal void OnDrawGizmos()
            {
                if (m_innerBounds != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.matrix = m_enclosingInstance.transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(m_girelloBounds.center, m_girelloBounds.size);
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(m_innerBounds.center, m_innerBounds.size);
                }
            }

            #endregion

            #region Girello Update Private methods

            /// <summary>
            /// Finds the distances of the player from the girello boundaries.
            /// The method returns a dictionary, where, for each bound type, a number in the range [0, 1] is returned: 0 means the player is inside the inner girello; 1 that it is outside the girello; intermediate values mean that he's in between.
            /// The method also returns a list of bounds for which the player is outside the inner limits
            /// </summary>
            /// <param name="playerDistances">Out parameter, that will hold a dictionary of player distances from the girello sides</param>
            /// <param name="trespassingBoundaries">Out parameter, that will hold the boundaries for which the player is outside the inner limits</param>
            private void FindPlayerDistances(out Dictionary<GirelloLimitType, float> playerDistances, out GirelloLimitType[] trespassingBoundaries)
            {
                //init distances vector as the player was distant to each side of the Girello (wow, he's ubiquitous!)
                playerDistances = new Dictionary<GirelloLimitType, float>()
                {
                    {GirelloLimitType.Right, 0},
                    {GirelloLimitType.Left, 0},
                    {GirelloLimitType.Front, 0},
                    {GirelloLimitType.Back, 0},
                    {GirelloLimitType.Top, 0},
                    {GirelloLimitType.Down, 0},
                };

                //get player bounds aabb vertices
                Vector3[] aabbVertices = new Vector3[] 
                {
                    m_enclosingInstance.Player.bounds.center + new Vector3(m_enclosingInstance.Player.bounds.extents.x, m_enclosingInstance.Player.bounds.extents.y, m_enclosingInstance.Player.bounds.extents.z),
                    m_enclosingInstance.Player.bounds.center + new Vector3(m_enclosingInstance.Player.bounds.extents.x, m_enclosingInstance.Player.bounds.extents.y, -m_enclosingInstance.Player.bounds.extents.z),
                    m_enclosingInstance.Player.bounds.center + new Vector3(-m_enclosingInstance.Player.bounds.extents.x, m_enclosingInstance.Player.bounds.extents.y, -m_enclosingInstance.Player.bounds.extents.z),
                    m_enclosingInstance.Player.bounds.center + new Vector3(-m_enclosingInstance.Player.bounds.extents.x, m_enclosingInstance.Player.bounds.extents.y, m_enclosingInstance.Player.bounds.extents.z),
                    m_enclosingInstance.Player.bounds.center + new Vector3(m_enclosingInstance.Player.bounds.extents.x, -m_enclosingInstance.Player.bounds.extents.y, m_enclosingInstance.Player.bounds.extents.z),
                    m_enclosingInstance.Player.bounds.center + new Vector3(m_enclosingInstance.Player.bounds.extents.x, -m_enclosingInstance.Player.bounds.extents.y, -m_enclosingInstance.Player.bounds.extents.z),
                    m_enclosingInstance.Player.bounds.center + new Vector3(-m_enclosingInstance.Player.bounds.extents.x, -m_enclosingInstance.Player.bounds.extents.y, -m_enclosingInstance.Player.bounds.extents.z),
                    m_enclosingInstance.Player.bounds.center + new Vector3(-m_enclosingInstance.Player.bounds.extents.x, -m_enclosingInstance.Player.bounds.extents.y, m_enclosingInstance.Player.bounds.extents.z),
                };

                //for each vertex representing the player calculate distance from faces, then keep the maximum one among all the vertices
                foreach (Vector3 aabbVertex in aabbVertices)
                {
                    //get vertex position in local frame of reference
                    Vector3 aabbVertexLocalPosition = m_enclosingInstance.transform.InverseTransformPoint(aabbVertex);

                    //compute the distance of the vertex from each side of the box and show only the arrows for the faces where the player is outside the inner bounds.
                    //Compute overall transparency of box using the minimum distance from the outer bounds.
                    //Notice that we compute distances horizontally and vertically from each bound and that we clamp it in the range [0, 1].
                    //Actually, we use the clamp method also to retain the highest value
                    playerDistances[GirelloLimitType.Right] = Mathf.Clamp((aabbVertexLocalPosition.x - m_innerBounds.center.x - m_innerBounds.extents.x) / Mathf.Max(0.0001f, m_girelloBounds.extents.x - m_innerBounds.extents.x), playerDistances[GirelloLimitType.Right], 1);
                    playerDistances[GirelloLimitType.Left] = Mathf.Clamp((-aabbVertexLocalPosition.x + m_innerBounds.center.x - m_innerBounds.extents.x) / Mathf.Max(0.0001f, m_girelloBounds.extents.x - m_innerBounds.extents.x), playerDistances[GirelloLimitType.Left], 1);
                    playerDistances[GirelloLimitType.Front] = Mathf.Clamp((aabbVertexLocalPosition.z - m_innerBounds.center.z - m_innerBounds.extents.z) / Mathf.Max(0.0001f, m_girelloBounds.extents.z - m_innerBounds.extents.z), playerDistances[GirelloLimitType.Front], 1);
                    playerDistances[GirelloLimitType.Back] = Mathf.Clamp((-aabbVertexLocalPosition.z + m_innerBounds.center.z - m_innerBounds.extents.z) / Mathf.Max(0.0001f, m_girelloBounds.extents.z - m_innerBounds.extents.z), playerDistances[GirelloLimitType.Back], 1);
                    playerDistances[GirelloLimitType.Top] = Mathf.Clamp((aabbVertexLocalPosition.y - m_innerBounds.center.y - m_innerBounds.extents.y) / Mathf.Max(0.0001f, m_girelloBounds.extents.y - m_innerBounds.extents.y), playerDistances[GirelloLimitType.Top], 1);

                    if(!m_enclosingInstance.DisableLowerBound) //check lower bound only if requested
                        playerDistances[GirelloLimitType.Down] = Mathf.Clamp((-aabbVertexLocalPosition.y + m_innerBounds.center.y - m_innerBounds.extents.y) / Mathf.Max(0.0001f, m_girelloBounds.extents.y - m_innerBounds.extents.y), playerDistances[GirelloLimitType.Down], 1);
                }

                //get the sides for which the computed distance is above 0         
                trespassingBoundaries = playerDistances.Where(pair => pair.Value > 0).Select(pair => pair.Key).ToArray();
            }

            /// <summary>
            /// Updates the bounding box of the Girello, drawing it on the screen.
            /// </summary>
            /// <param name="playerDistances">Dictionary of player distances from the girello sides</param>
            /// <param name="trespassingBoundaries">Boundaries for which the player is outside the inner limits</param>
            private void UpdateBox(Dictionary<GirelloLimitType, float> playerDistances, GirelloLimitType[] trespassingBoundaries)
            {
                //if the player is inside the inner bounds area, hide all.
                //The second condition make sure we were previously outside the inner area: if a=0, then we are already entered this condition in a past frame and it is not necessary to execute it again
                if (trespassingBoundaries.Length == 0)
                {
                    if (m_enclosingInstance.BoxColor.a != 0)
                    {
                        //deactivate all childs
                        foreach (Transform child in m_enclosingInstance.transform)
                            if (child.GetInstanceID() != m_enclosingInstance.transform.GetInstanceID())
                                child.gameObject.SetActive(false);

                        //set box color to transparent
                        m_enclosingInstance.BoxColor = new Color(m_enclosingInstance.BoxColor.r, m_enclosingInstance.BoxColor.g, m_enclosingInstance.BoxColor.b, 0);
                    }
                }
                //else, we are outside the area and should draw everything
                else
                {
                    //if we are here, it is the first frame we exit from the inner area, so we have to re-activate all the children
                    if (m_enclosingInstance.BoxColor.a == 0)
                    {
                        foreach (Transform child in m_enclosingInstance.transform)
                            if (child.GetInstanceID() != m_enclosingInstance.transform.GetInstanceID())
                                child.gameObject.SetActive(true);
                    }


                    //take maximum distance and set that one as the transparency of everything
                    float maxAlpha = playerDistances.Values.Max();

                    if (maxAlpha == 0) //0 is only for player inside inner bounds
                        maxAlpha = 0.001f;

                    //set box color to the found alpha
                    m_enclosingInstance.BoxColor = new Color(m_enclosingInstance.BoxColor.r, m_enclosingInstance.BoxColor.g, m_enclosingInstance.BoxColor.b, maxAlpha);
                }

            }

            /// <summary>
            /// Updates the flashing arrows of the Girello, drawing them on the screen.
            /// </summary>
            /// <param name="playerDistances">Dictionary of player distances from the girello sides</param>
            /// <param name="trespassingBoundaries">Boundaries for which the player is outside the inner limits</param>
            private void UpdateArrows(Dictionary<GirelloLimitType, float> playerDistances, GirelloLimitType[] trespassingBoundaries)
            {
                //update arrow color material (i.e. the correct transparency)
                m_arrowsMaterial.color = m_enclosingInstance.BoxColor;

                //for each face of flashing arrows
                foreach (Transform arrowFaceSetChild in m_enclosingInstance.transform.GetChild(0))
                {
                    if (arrowFaceSetChild.GetInstanceID() != m_enclosingInstance.transform.GetChild(0).GetInstanceID())
                    {
                        //if the face represents a face the player is trespassing, show it, otherwise hide it
                        GirelloLimitType faceLimitType = (GirelloLimitType)Int32.Parse(arrowFaceSetChild.name);

                        if (trespassingBoundaries.Contains(faceLimitType) && playerDistances[faceLimitType] > m_enclosingInstance.MinShowArrowDistance)
                            arrowFaceSetChild.gameObject.SetActive(true);
                        else
                        {
                            arrowFaceSetChild.gameObject.SetActive(false);
                        }

                    }
                }

            }

            #endregion

            #region Girello Box drawing methods

            /// <summary>
            /// Draw a quad that simulates a thick line between two points.
            /// To simulate thickness, draw line as a quad scaled along the camera's vertical axis.
            /// </summary>
            /// <param name="p1">First point</param>
            /// <param name="p2">Second point</param>
            /// <param name="thickness">Thickness of the line to draw</param>
            private void DrawQuadLine(Vector3 p1, Vector3 p2, float thickness)
            {
                //code inspired by http://answers.unity3d.com/questions/144693/wireframe-rendering.html

                float thisWidth = 1.0f / Screen.width * thickness * 0.5f;
                Vector3 edge1 = m_mainCamera.transform.position - (p2 + p1) / 2.0f;    //vector from line center to camera
                Vector3 edge2 = p2 - p1;    //vector from point to point
                Vector3 perpendicular = Vector3.Cross(edge1, edge2).normalized * thisWidth;

                GL.Vertex(p1 - perpendicular);
                GL.Vertex(p1 + perpendicular);
                GL.Vertex(p2 + perpendicular);
                GL.Vertex(p2 - perpendicular);
            }

            /// <summary>
            /// Draws a face of the box, in wireframe mode
            /// </summary>
            /// <param name="pointA">First point of the face, in counter-clockwise order</param>
            /// <param name="pointB">Second point of the face, in counter-clockwise order</param>
            /// <param name="pointC">Third point of the face, in counter-clockwise order</param>
            /// <param name="pointD">Fourth point of the face, in counter-clockwise order</param>
            /// <param name="drawingLimit">Identifies which face we are drawing. It is used to evaluate if the internal grid has to be drawn because player is near face limit</param>
            private void DrawBoxQuad(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, GirelloLimitType drawingLimit)
            {
                float avgLossyScale = (m_enclosingInstance.transform.lossyScale.x + m_enclosingInstance.transform.lossyScale.y + m_enclosingInstance.transform.lossyScale.z) / 3; 

                //Draw the wireframe contour of the face (change width of line considering overall scale of the object)
                DrawQuadLine(pointA, pointB, m_enclosingInstance.BoxLinesThickness * avgLossyScale);
                DrawQuadLine(pointB, pointC, m_enclosingInstance.BoxLinesThickness * avgLossyScale);
                DrawQuadLine(pointC, pointD, m_enclosingInstance.BoxLinesThickness * avgLossyScale);
                DrawQuadLine(pointD, pointA, m_enclosingInstance.BoxLinesThickness * avgLossyScale);

                //if at last evaluation, the player was near this boundary, we have to draw the internal grid
                if (m_lastTrespassingBoundaries.Contains(drawingLimit))
                {
                    //calculate some data, along the two directions of the quad (horizontal and vertical):
                    //- length of each side
                    //- distance vector along each side
                    //- versor of each side of the quad
                    //- distance, along each side, between two consecutive lines of the grid
                    float ABdist = Vector3.Distance(pointA, pointB);
                    float BCdist = Vector3.Distance(pointB, pointC);
                    Vector3 ABdiff = (pointB - pointA);
                    Vector3 BCdiff = (pointC - pointB);
                    Vector3 ABdirection = ABdiff.normalized;
                    Vector3 BCdirection = BCdiff.normalized;
                    Vector3 ABLineIncrement = ABdiff / (m_enclosingInstance.GridLinesNumber + 1); //notice the +1, because the grid lines are INTERNAL lines
                    Vector3 BCLineIncrement = BCdiff / (m_enclosingInstance.GridLinesNumber + 1);

                    //for each grid line to be drawn
                    for (int i = 0; i < m_enclosingInstance.GridLinesNumber; i++)
                    {
                        //draw line using AB side as starting point and BC direction
                        DrawQuadLine(pointA + (i + 1) * ABLineIncrement, pointA + (i + 1) * ABLineIncrement + BCdiff, m_enclosingInstance.BoxGridLinesThickness * avgLossyScale);

                        //draw line using BC side as starting point and AB direction
                        DrawQuadLine(pointB + (i + 1) * BCLineIncrement, pointB + (i + 1) * BCLineIncrement - ABdiff, m_enclosingInstance.BoxGridLinesThickness * avgLossyScale);
                    }

                }

            }

            /// <summary>
            /// Converts a point to its world global position
            /// </summary>
            /// <param name="vec"></param>
            /// <returns></returns>
            private Vector3 ToWorldPosition(Vector3 vec)
            {
                return m_enclosingInstance.gameObject.transform.TransformPoint(vec);
            }

            /// <summary>
            /// Called on object rendering
            /// </summary>
            internal void OnRenderObject()
            {
                if (m_enclosingInstance.Player == null || !m_enclosingInstance.DrawBox || m_mainCamera == null)
                    return;

                //draw the wireframe cube
                m_enclosingInstance.LinesMaterial.SetPass(0);
                GL.Begin(GL.QUADS);
                GL.Color(m_enclosingInstance.BoxColor);

                //draw wireframe of upper face of the box
                DrawBoxQuad(ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         GirelloLimitType.Top);

                //draw wireframe of lower face of the box
                DrawBoxQuad(ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         GirelloLimitType.Down);

                //draw wireframe of left face of the box
                DrawBoxQuad(ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, +m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, +m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         GirelloLimitType.Left);

                //draw wireframe of right face of the box
                DrawBoxQuad(ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, +m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, +m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         GirelloLimitType.Right);

                //draw wireframe of front face of the box
                DrawBoxQuad(ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, +m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, +m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, +m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, +m_girelloBounds.extents.z)),
                         GirelloLimitType.Front);

                //draw wireframe of back face of the box
                DrawBoxQuad(ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         ToWorldPosition(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z)),
                         GirelloLimitType.Back);

                GL.End();
            }

            #endregion

            #region Girello Arrows drawing methods

            /// <summary>
            /// Create the flashing arrow for all the box faces
            /// </summary>
            private void CreateFlashingArrows()
            {
                if (!m_enclosingInstance.DrawArrows)
                    return;

                //create root object for arrows
                GameObject go = new GameObject();
                go.name = "Arrows root";
                go.transform.SetParent(m_enclosingInstance.transform, false);

                //create arrows of upper face of the box
                CreateFlashingArrowFace(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         GirelloLimitType.Top);

                //create arrows of lower face of the box
                CreateFlashingArrowFace(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         GirelloLimitType.Down);

                //create arrows of left face of the box
                CreateFlashingArrowFace(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, +m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, +m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         GirelloLimitType.Left);

                //create arrows of right face of the box
                CreateFlashingArrowFace(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, +m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, +m_girelloBounds.extents.y, m_girelloBounds.extents.z),
                         GirelloLimitType.Right);

                //create arrows of front face of the box
                CreateFlashingArrowFace(m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, +m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, +m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, +m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, +m_girelloBounds.extents.z),
                         GirelloLimitType.Front);

                //create arrows of back face of the box
                CreateFlashingArrowFace(m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(-m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, -m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         m_girelloBounds.center + new Vector3(m_girelloBounds.extents.x, m_girelloBounds.extents.y, -m_girelloBounds.extents.z),
                         GirelloLimitType.Back);

                //get arrow material reference and set arrows material to the same color of the box.
                //Notice that we have to create a dummy object, because GetComponentInChildren on an un-itialized object doesn't work
                go = Instantiate<GameObject>(m_enclosingInstance.ArrowObject);
                Renderer goRenderer = go.GetComponent<Renderer>();

                if (goRenderer == null) //find it in root or in the children
                    goRenderer = go.transform.GetComponentInChildren<Renderer>();

                if (goRenderer == null)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("Virtual girello - Arrow object must have a Renderer behaviour!");
                    }

                    throw new ArgumentException("Virtual girello - Arrow object must have a Renderer behaviour!");
                }

                m_arrowsMaterial = goRenderer.sharedMaterial;
                m_arrowsMaterial.color = m_enclosingInstance.BoxColor;
                Destroy(go);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Virtual girello - created arrows");
                }

                go.SetActive(false);
            }

            /// <summary>
            /// Create a set of arrow for a box face
            /// </summary>
            /// <param name="pointA">First vertex of the box face</param>
            /// <param name="pointB">Second vertex of the box face</param>
            /// <param name="pointC">Third vertex of the box face</param>
            /// <param name="pointD">Fourth vertex of the box face</param>
            /// <param name="drawingLimit">Which girello limit this face represent</param>
            private void CreateFlashingArrowFace(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, GirelloLimitType drawingLimit)
            {
                //create a child game object to contain all the arrows for this face
                GameObject arrowFaceGo = new GameObject();
                arrowFaceGo.name = ((int)drawingLimit).ToString(); //we give as name the integer representation of the enum value. This will come handy in the UpdateArrows method
                arrowFaceGo.transform.SetParent(m_enclosingInstance.transform.GetChild(0), false);

                //calculate some data, along the two directions of the quad (horizontal and vertical):
                //- length of each side
                //- distance vector along each side
                //- versor of each side of the quad
                //- distance, along each side, between two consecutive lines of the grid
                //- vector that represents the direction of the normal of this face, pointing inside the box
                //- rotation that rotates the arrow prefab so that it is aligned with the up vector
                float ABdist = Vector3.Distance(pointA, pointB);
                float BCdist = Vector3.Distance(pointB, pointC);
                Vector3 ABdiff = (pointB - pointA);
                Vector3 BCdiff = (pointC - pointB);
                Vector3 ABdirection = ABdiff.normalized;
                Vector3 BCdirection = BCdiff.normalized;
                Vector3 ABLineIncrement = ABdiff / (m_enclosingInstance.GridLinesNumber + 1); //notice the +1, because the grid lines are INTERNAL lines
                Vector3 BCLineIncrement = BCdiff / (m_enclosingInstance.GridLinesNumber + 1);
                Vector3 upVector = Vector3.Cross(ABdirection, BCdirection);
                Quaternion upRotation = Quaternion.LookRotation(ABdirection, upVector);

                //for each grid row line
                for (int row = 0; row < m_enclosingInstance.GridLinesNumber; row++)
                {
                    //for each grid column line
                    for (int col = 0; col < m_enclosingInstance.GridLinesNumber; col++)
                    {
                        //create the arrow object at the intersection of the two lines, and align it to the computed rotation, so that it points inside the box
                        GameObject arrowGo = Instantiate<GameObject>(m_enclosingInstance.ArrowObject);
                        arrowGo.transform.localPosition = pointA + (row + 1) * ABLineIncrement + (col + 1) * BCLineIncrement;
                        arrowGo.transform.localRotation = upRotation;
                        arrowGo.transform.SetParent(arrowFaceGo.transform, false);
                    }

                }

            }

            #endregion

            #region Initialization methods

            /// <summary>
            /// Coroutine to initialize the girello: it connects to the tracking service to get girello dimensions and then
            /// it initialize all stuff to show the box
            /// </summary>
            /// <param name="initialPlayer">Collider game object to be considered as the m_enclosingInstance.Player to track, if any set by parameter</param>
            /// <returns></returns>
            private IEnumerator InitGirello(Collider initialPlayer)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Virtual girello - init called");
                }
                
                //wait for tracking service to connect
                while (TrackingServiceManagerBasic.Instance == null || !TrackingServiceManagerBasic.Instance.IsTracking)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                //initialize everything

                //init color to transparent to avoid showing it opaque in first frame
                m_enclosingInstance.BoxColor.a = 0.001f;

                //ensure scale is one (Girello does not have sense with a scale multiplier)
                m_enclosingInstance.transform.localScale = Vector3.one;

                //just avoid null check in update
                m_lastTrespassingBoundaries = new GirelloLimitType[0];

                //get girello (inner and outer) extents from the tracking service
                m_girelloBounds = new Bounds(TrackingServiceManagerBasic.Instance.TrackingServiceEnvironment.SceneDescriptor.GameArea.Center.ToVector3(),
                                             TrackingServiceManagerBasic.Instance.TrackingServiceEnvironment.SceneDescriptor.GameArea.Size.ToVector3());
                Vector3 m_innerBoundsExtents = TrackingServiceManagerBasic.Instance.TrackingServiceEnvironment.SceneDescriptor.GameAreaInnerLimits.ToVector3();

                //calculate inner bounds 
                m_innerBounds = new Bounds(m_girelloBounds.center, m_innerBoundsExtents * 2);

                //if box has no volume, just disable girello
                if (m_girelloBounds.size.x == 0 || m_girelloBounds.size.y == 0 || m_girelloBounds.size.z == 0)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Virtual girello - Bounds represent a null-volume box. Girello will disable itself");
                    }

                    m_enclosingInstance.enabled = false;
                    yield break;
                }

                //if the provided collider is null, look for a character controller in the scene
                if (initialPlayer == null)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Virtual girello - No Collider provided... looking for a Character Controller");
                    }

                    m_enclosingInstance.Player = FindObjectOfType<CharacterController>();

                    //if no one is found, disable the girello (it is useless)
                    if (m_enclosingInstance.Player == null)
                    {
                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("Virtual girello - No Character Controller found. Girello will disable itself");
                        }

                        m_enclosingInstance.enabled = false;
                    }
                }
                else
                    m_enclosingInstance.Player = initialPlayer;

                //create flashing arrows for the faces
                CreateFlashingArrows();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Virtual girello - init finished");
                }

            }

            #endregion
        }

    }
}
