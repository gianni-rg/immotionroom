namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    public class ErrorResponse : BaseResponse
    {
        #region Properties

        public string StackTrace { get; set; }

        #endregion

        #region Constructor

        public ErrorResponse(string description, string stackTrace = null)
        {
            ErrorDescription = description;
            StackTrace = stackTrace;
        }

        #endregion
    }
}
