using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Infrastructure.Common
{
    public class FailedValidation : IOperationResult
    {
        public FailedValidation(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        public bool IsSuccessful
        {
            get { return false; }
        }
    }
}
