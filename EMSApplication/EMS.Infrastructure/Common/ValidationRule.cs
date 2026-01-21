using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Infrastructure.Common
{
    public abstract class ValidationRule<T>
    {
        protected ValidationRule()
        {
            DependentRules = new List<ValidationRule<T>>();
        }
        public bool IsCritical { get; protected set; }

        public IList<ValidationRule<T>> DependentRules { get; set; }

        public abstract IOperationResult Check(T entity);
        public abstract Task<IOperationResult> CheckAsync(T entity);
    }
}
