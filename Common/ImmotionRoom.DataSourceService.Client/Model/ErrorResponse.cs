namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model
{
    public class ErrorResponse : BaseResponse
    {
        #region Properties

        public string StackTrace { get; set; }

        #endregion

        #region Constructor

        public ErrorResponse(string description, string stackTrace = null)
        {
            Error = description;
            StackTrace = stackTrace;
        }

        #endregion
    }
}
