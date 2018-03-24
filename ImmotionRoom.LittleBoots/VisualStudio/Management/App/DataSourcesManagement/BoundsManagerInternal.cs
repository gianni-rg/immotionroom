namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Manages <see cref="Bounds"/> object, allowing operations on them like changing limits and drawing them
    /// </summary>
    public partial class BoundsManager : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the BoundsManager, for obfuscation purposes
        /// </summary>
        private partial class BoundsManagerInternal
        {
            #region Private fields

            /// <summary>
            /// The Bounds Manager that contains this object
            /// </summary>
            private BoundsManager m_boundsManager;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="boundsManager">Enclosing instance, whose code has to be implemented</param>
            internal BoundsManagerInternal(BoundsManager boundsManager)
            {
                m_boundsManager = boundsManager;
            }

            #endregion

            #region Behaviour Methods

            internal void Start()
            {
                //create the 4 LineRenderers to draw the quad
                m_boundsManager.BoundsLinesMaterial.color = m_boundsManager.BoundsLinesColor;

                for (int i = 0; i < 4; i++) //we create 4 children, one for each side of the bounding box
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.SetParent(m_boundsManager.transform, false);
                    go.GetComponent<Renderer>().material = m_boundsManager.BoundsLinesMaterial;
                    go.GetComponent<Renderer>().material.color = m_boundsManager.BoundsLinesColor;
                }
            }

            internal void Update()
            {
                //front side
                m_boundsManager.transform.GetChild(0).position = m_boundsManager.BoundsCenter + new Vector3(0, 0, m_boundsManager.BoundsExtents.z);
                m_boundsManager.transform.GetChild(0).localScale = new Vector3(2 * m_boundsManager.BoundsExtents.x + m_boundsManager.BoundsLinesThickness, m_boundsManager.BoundsExtents.y, m_boundsManager.BoundsLinesThickness);

                //left side
                m_boundsManager.transform.GetChild(1).position = m_boundsManager.BoundsCenter + new Vector3(m_boundsManager.BoundsExtents.x, 0, 0);
                m_boundsManager.transform.GetChild(1).localScale = new Vector3(m_boundsManager.BoundsLinesThickness, m_boundsManager.BoundsExtents.y, 2 * m_boundsManager.BoundsExtents.z + m_boundsManager.BoundsLinesThickness);

                //back side
                m_boundsManager.transform.GetChild(2).position = m_boundsManager.BoundsCenter + new Vector3(0, 0, -m_boundsManager.BoundsExtents.z);
                m_boundsManager.transform.GetChild(2).localScale = new Vector3(2 * m_boundsManager.BoundsExtents.x + m_boundsManager.BoundsLinesThickness, m_boundsManager.BoundsExtents.y, m_boundsManager.BoundsLinesThickness);

                //right side
                m_boundsManager.transform.GetChild(3).position = m_boundsManager.BoundsCenter + new Vector3(-m_boundsManager.BoundsExtents.x, 0, 0);
                m_boundsManager.transform.GetChild(3).localScale = new Vector3(m_boundsManager.BoundsLinesThickness, m_boundsManager.BoundsExtents.y, 2 * m_boundsManager.BoundsExtents.z + m_boundsManager.BoundsLinesThickness);
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Set a new boundary limit for the managed bounding box.
            /// The provided point becomes the new limit for the bounding element, changing the present limit specified as parameter
            /// </summary>
            /// <param name="newLimitType">Limit to change (e.g. if front limit is specified, only z value will get affected)</param>
            /// <param name="newLimit">Point to use as new limit</param>
            internal void SetNewBoundLimit(BoundsGrowingType newLimitType, Vector3 newLimit)
            {
                //resize the bounds rectangle, depending on the type of new bound limit provided.
                //Use the provided point and the limit that remains unchanged to calculate new dimension and center

                if (newLimitType == BoundsGrowingType.LeftLimit)
                {
                    float rightLimit = m_boundsManager.BoundsCenter.x + m_boundsManager.BoundsExtents.x;
                    float distance = rightLimit - newLimit.x;

                    if (distance < 0)
                    {
                        newLimit.x = rightLimit;
                        distance = 0;
                    }

                    m_boundsManager.BoundsCenter.x = (rightLimit + newLimit.x) / 2;
                    m_boundsManager.BoundsExtents.x = distance / 2;
                }
                else if (newLimitType == BoundsGrowingType.RightLimit)
                {
                    float leftLimit = m_boundsManager.BoundsCenter.x - m_boundsManager.BoundsExtents.x;
                    float distance = -leftLimit + newLimit.x;

                    if (distance < 0)
                    {
                        newLimit.x = leftLimit;
                        distance = 0;
                    }

                    m_boundsManager.BoundsCenter.x = (leftLimit + newLimit.x) / 2;
                    m_boundsManager.BoundsExtents.x = distance / 2;
                }
                else if (newLimitType == BoundsGrowingType.BackLimit)
                {
                    float frontLimit = m_boundsManager.BoundsCenter.z + m_boundsManager.BoundsExtents.z;
                    float distance = frontLimit - newLimit.z;

                    if (distance < 0)
                    {
                        newLimit.x = frontLimit;
                        distance = 0;
                    }

                    m_boundsManager.BoundsCenter.z = (frontLimit + newLimit.z) / 2;
                    m_boundsManager.BoundsExtents.z = distance / 2;
                }
                else if (newLimitType == BoundsGrowingType.FrontLimit)
                {
                    float backLimit = m_boundsManager.BoundsCenter.z - m_boundsManager.BoundsExtents.z;
                    float distance = -backLimit + newLimit.z;

                    if (distance < 0)
                    {
                        newLimit.x = backLimit;
                        distance = 0;
                    }

                    m_boundsManager.BoundsCenter.z = (backLimit + newLimit.z) / 2;
                    m_boundsManager.BoundsExtents.z = distance / 2;
                }

            }

            /// <summary>
            /// Set bounding box extents so that the box is null
            /// </summary>
            internal void SetNullLimits()
            {
                m_boundsManager.BoundsCenter = new Vector3(0, m_boundsManager.BoundsCenter.y, 0);
                m_boundsManager.BoundsExtents = new Vector3(0, m_boundsManager.BoundsExtents.y, 0); //leaves room on y because otherwise, when the box will be changed on the XZ plane, we'll still have cubes with 0-volume (because their height is null)
            }


            #endregion

        }
    }
}

