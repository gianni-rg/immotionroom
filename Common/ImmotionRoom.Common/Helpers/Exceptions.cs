namespace ImmotionAR.ImmotionRoom.Helpers
{
    using System;
    using System.Text;

    public static class Exceptions
    {
        public static string HandleException(Exception ex)
        {
            var aggregateException = ex as AggregateException;
            var message = new StringBuilder();
            if (aggregateException == null)
            {
                message.Append(ex.Message);
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    message.Append(string.Format(" ---> {0}", innerEx.Message));
                    innerEx = innerEx.InnerException;
                }
            }
            else
            {
                foreach (var agEx in aggregateException.Flatten().InnerExceptions)
                {
                    message.Append(string.Format(" ---> {0}", agEx.Message));
                }
            }

            return message.ToString();
        }
    }
}