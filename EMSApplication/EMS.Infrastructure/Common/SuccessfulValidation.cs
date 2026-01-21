using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Infrastructure.Common
{
    public class SuccessfulValidation : IOperationResult
    {
        public SuccessfulValidation(string message = "Entity is valid")
        {
            Message = message;
        }

        #region IOperationResult Members

        public string Message { get; private set; }

        public bool IsSuccessful
        {
            get { return true; }
        }

        #endregion
    }
}
