namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Records a history of certain data elements
    /// </summary>
    /// <typeparam name="DataType">Type of data to be histored</typeparam>
    internal class DataHistoryList<DataType>
    {
        /// <summary>
        ///     Actual implementation of data history
        /// </summary>
        private readonly List<DataType> m_dataHistory;

        /// <summary>
        ///     Desired size of the history
        /// </summary>
        private readonly int m_desiredLength;

        #region Protected properties

        /// <summary>
        ///     Get actual implementation of history
        /// </summary>
        protected List<DataType> ActualList
        {
            get { return m_dataHistory; }
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Get n-th element of the data history
        /// </summary>
        /// <param name="index">Index of the desired element. 0 represents most recent element</param>
        /// <returns>N-th element of the history</returns>
        public DataType this[int index]
        {
            get
            {
                if (index < 0 || index >= m_dataHistory.Count)
                    throw new ArgumentOutOfRangeException("Index of DataHistoryList is out of range");

                return m_dataHistory[index];
            }
        }

        /// <summary>
        ///     Get actual number of elements stored inside this history
        /// </summary>
        public int Count
        {
            get { return m_dataHistory.Count; }
        }

        #endregion

        /// <summary>
        ///     Creates a new instance of the <see cref="DataHistoryList" /> class.
        /// </summary>
        /// <param name="desiredLength">Desired length for the history list</param>
        public DataHistoryList(int desiredLength)
        {
            m_desiredLength = desiredLength;

            //creation and initialization of list
            m_dataHistory = new List<DataType>();
        }

        /// <summary>
        ///     Add a new value to the history. The new value is added to the tail of the list, while the value at the
        ///     top gets removed, if it is necessary to respect the history size
        /// </summary>
        /// <param name="newMatrix">New joint man</param>
        public void PushNewValue(DataType newData)
        {
            if (m_dataHistory.Count >= m_desiredLength)
                m_dataHistory.RemoveAt(m_dataHistory.Count - 1);

            m_dataHistory.Insert(0, newData);
        }
    }
}