namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.Helpers
{

    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Put all children of this object into a 2D grid, evenly spacing them horizontally and vertically.
    /// If a line is incomplete, the elements get centered
    /// </summary>
    [ExecuteInEditMode]
    public class SimpleObjectsGrid : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// Number of Columns of the grid
        /// </summary>
        [Tooltip("Number of Columns of the grid")]
        public int NumColumns = 3;

        /// <summary>
        /// Space that has to be left between the children, inside the grid
        /// </summary>
        [Tooltip("Space that has to be left between the children, inside the grid")]
        public Vector2 InterObjectSpace = new Vector2(1.0f, 0.75f);

        #endregion

        #region Private fields

        /// <summary>
        /// Number of the children of this object, at the last call of UpdateElements
        /// </summary>
        private int m_lastChildrenNum;

        /// <summary>
        /// Value of NumColumns, at the last call of UpdateElements
        /// </summary>
        private int m_lastNumColumns;

        /// <summary>
        /// Value of InterObjectSpace, at the last call of UpdateElements
        /// </summary>
        private Vector2 m_lastInterObjectSpace;

        #endregion

        #region Behaviour methods

        void OnEnable()
        {
            UpdateElements();
        }

        void Update()
        {
            //if conditions since last update have changed, re-compute children disposition (otherwise it's useless)
            if (m_lastChildrenNum != transform.childCount ||
               m_lastInterObjectSpace != InterObjectSpace ||
               m_lastNumColumns != NumColumns)
                UpdateElements();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Arranges the children of this object in a grid fashion
        /// </summary>
        private void UpdateElements()
        {
            if (NumColumns == 0)
                return;

            //find total number of rows and the elements that the last row contains 
            int rows = NumColumns == 1 ? transform.childCount : (transform.childCount - 1) / NumColumns + 1;
            int finalRowCols = transform.childCount % NumColumns;

            if (finalRowCols == 0 && transform.childCount != 0) //if last row has 0 elements, it does mean the the actual last row is full
                finalRowCols = NumColumns;

            //find vertical position of the upper row
            float initialYPos = InterObjectSpace.y * (rows - 1) * 0.5f;

            //for each row
            for (int r = 0; r < rows; r++)
            {
                //get number of elements in this row (can be different only in the last, maybe incomplete, row)
                int elementsInThisRow = r == rows - 1 ? finalRowCols : NumColumns;

                //get left position of this row (may be different in last incomplete row)
                float initialXPos = -InterObjectSpace.x * (elementsInThisRow - 1) * 0.5f;

                //for each column of this row, assign the local position of this element so that it stays inside the grid
                for (int c = 0; c < elementsInThisRow; c++)
                {
                    transform.GetChild(r * NumColumns + c).localPosition = new Vector3(initialXPos + c * InterObjectSpace.x, initialYPos - r * InterObjectSpace.y, 0);
                }
            }

            //save values used during this call
            m_lastChildrenNum = transform.childCount;
            m_lastInterObjectSpace = InterObjectSpace;
            m_lastNumColumns = NumColumns;
        }

        #endregion
    }

}