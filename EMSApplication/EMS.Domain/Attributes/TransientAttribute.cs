namespace EMS.Domain.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TransientAttribute : Attribute
    {
        #region Constructor and Initialization

        public TransientAttribute()
        {
        }

        #endregion
    }
}
