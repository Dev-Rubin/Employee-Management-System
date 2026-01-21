using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Infrastructure.Common
{
    public class FailedTransaction : ITransactionResult
    {
        #region Constructor and Initialization
        public FailedTransaction(string message = "An error has occurred",
                                 Exception ex = null)
        {
            Message = message;
            if (ex != null)
            {
                Exception = ex;
                Message += ": " + ex.Message;
            }
        }
        public FailedTransaction(IOperationResult result, Exception ex = null)
        {
            Message = result.Message;
            if (ex != null)
            {
                Exception = ex;
                Message += ": " + ex.Message;
            }
        }

        #endregion

        #region Properties
        public string Message { get; set; }
        public Exception Exception { get; private set; }

        public bool IsSuccessful
        {
            get { return false; }
        }
        public string ReferenceNo { get; set; }
        public int ReferenceId { get; set; }

        #endregion
    }
}
