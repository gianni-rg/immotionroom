namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Handles a queue of calibration matrices
    /// </summary>
    internal class MatricesQueue
    {
        #region Private fields

        /// <summary>
        ///     Actual list of last matrices
        /// </summary>
        private readonly List<Matrix4x4> m_Matrices;

        #endregion

        #region Properties

        public List<Matrix4x4> Matrices
        {
            get { return m_Matrices; }
        }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="MatricesQueue" /> class.
        /// </summary>
        /// <param name="desiredLength">Desired length for the calibration matrices queue</param>
        public MatricesQueue(int desiredLength)
        {
            //creation and initialization of matrix list
            m_Matrices = new List<Matrix4x4>();

            for (var i = 0; i < desiredLength; i++)
                m_Matrices.Add(Matrix4x4.Identity);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Add a new value to the matrix queue. The new value is added to the tail of the list, while the value at the
        ///     top gets removed
        /// </summary>
        /// <param name="newMatrix">New calibration matrix</param>
        public void PushNewValue(Matrix4x4 newMatrix)
        {
            m_Matrices.RemoveAt(0);
            m_Matrices.Add(newMatrix);
        }

        /// <summary>
        ///     Gets the oldest value contained in this collection
        /// </summary>
        public Matrix4x4 GetOldestValue()
        {
            return m_Matrices.First();
        }

        #endregion
    }
}