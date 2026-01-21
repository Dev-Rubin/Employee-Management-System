using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Infrastructure.Common
{
    public class ValidationResult : IOperationResult
    {
        private readonly string _entityName;

        public ValidationResult(string entityName = "Entity")
        {
            Errors = new List<string>();
            _entityName = entityName;
        }

        public IList<string> Errors { get; set; }

        #region IOperationResult Members

        public string Message
        {
            get { return IsSuccessful ? _entityName + " is valid" : "Errors.ToSingleString()"; }
        }

        public bool IsSuccessful
        {
            get { return !Errors.Any(); }
        }

        #endregion

        public static IOperationResult Successful(string message = "")
        {
            return new SuccessfulValidation(message);
        }

        public static IOperationResult Failed(string message)
        {
            return new FailedValidation(message);
        }

        /// <summary>
        ///   Indicates a failed validation caused by a duplicate
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="duplicityConditions"> The message showing what has a duplicate and the conditions for it. (Defaults to class name) </param>
        /// <returns> </returns>
        public static IOperationResult AlreadyExists<T>(string duplicityConditions = "")
        {
            return new FailedValidation(
                String.Format("Messages.ALREADYEXISTS_1",
                              typeof(T).GetClassDescription() + " with " + duplicityConditions));
        }

        /// <summary>
        ///   Indicated a failed validation caused by a duplicate
        /// </summary>
        /// <param name="duplicatedField"> The field that has been duplicated. Format: Table Name + Field Name Example: Company Code </param>
        /// <returns> </returns>
        public static IOperationResult AlreadyExists(string duplicatedField)
        {
            return new FailedValidation(
                String.Format("Messages.UNIQUE_FIELD_DUPLICATED" + "Messages.ALREADYEXISTS_1", duplicatedField));
        }
    }
}
