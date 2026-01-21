using EMS.Infrastructure.Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMS.Infrastructure.Common
{
    public static class TransactionResultExtensions
    {
        public static ITransactionResult ToTransactionResult(this IOperationResult result)
        {
            if (result.IsSuccessful)
            {
                return new SuccessfulTransaction(result.Message);
            }
            return new FailedTransaction(result);
        }

        public static ITransactionResult WithMessage(this ITransactionResult transactionResult, string successMessage, string failureMessage)
        {
            if (transactionResult.IsSuccessful)
            {
                return new SuccessfulTransaction(successMessage);
            }
            return new FailedTransaction(failureMessage + "; " + transactionResult.Message, transactionResult.Exception);
        }
    }
}
