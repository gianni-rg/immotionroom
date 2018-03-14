namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;

    public class CommandResultRequest
    {
        private readonly Delegate m_Callback;

        public string RequestId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResultRequest" /> class.
        /// </summary>
        /// <param name="requestId">The request ID to retrieve status for</param>
        /// <param name="callback">The callback method that can be executed
        /// by the recipient to notify the sender that the message has been
        /// processed.</param>
        public CommandResultRequest(string requestId, Action<CommandResult<object>> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            m_Callback = callback;

            RequestId = requestId;
        }

        /// <summary>
        /// Executes the callback that was provided with the message with an
        /// arbitrary number of parameters.
        /// </summary>
        /// <param name="arguments">A  number of parameters that will
        /// be passed to the callback method.</param>
        /// <returns>The object returned by the callback method.</returns>
        public virtual object Execute(params object[] arguments)
        {
            return m_Callback.DynamicInvoke(arguments);
        }
    }
}
