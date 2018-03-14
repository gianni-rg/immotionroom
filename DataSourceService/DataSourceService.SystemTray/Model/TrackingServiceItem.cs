namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    public class TrackingServiceItem : BaseModel
    {
        private string m_StatusIcon;
        private string m_Name;
        private string m_StatusDescription;

        #region Properties

        public string StatusIcon
        {
            get { return m_StatusIcon; }
            set { Set(ref m_StatusIcon, value); }
        }

        public string Name
        {
            get { return m_Name; }
            set { Set(ref m_Name, value); }
        }

        public string StatusDescription
        {
            get { return m_StatusDescription; }
            set { Set(ref m_StatusDescription, value); }
        }

    #endregion
    }
}
