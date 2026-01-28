using EMS.Infrastructure.Common;
using EMS.Infrastructure.Persistence.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EMS.Infrastructure.Persistence.Service
{
    public abstract class CrudService
    {
        private const string GenericErrorMessage = "Transaction failed. Please try again. If the problem occurs again, please contact your system administrator.";
        protected CrudService() { }
        protected internal virtual IUnitOfWork UnitOfWork { get; protected set; }
        protected internal virtual IRepository Repository { get; set; }
        protected internal virtual IQueries Queries { get; set; }

        private TransactionChain _transactionChain;
        protected CrudService(IUnitOfWork unitOfWork, IRepository repository, IQueries queries)
        {
            UnitOfWork = unitOfWork;
            Repository = repository;
            Queries = queries;

        }
        protected TransactionChain Transact
        {
            get
            {
                if (!UnitOfWork.IsActiveTransaction())
                    _transactionChain = new TransactionChain(UnitOfWork);

                return _transactionChain;
            }
        }
        protected ITransactionResult WriteToDatabase(Action writeAction)
        {
            return WriteToDatabase(
                writeAction,
                "MessageHelper.SuccessfulTransaction()",
                "MessageHelper.FailedTransaction()");
        }
        protected async Task<ITransactionResult> WriteToDatabaseAsync(Action writeAction)
        {
            return await WriteToDatabaseAsync(
                writeAction,
                "MessageHelper.SuccessfulTransaction()",
                "MessageHelper.FailedTransaction()").ConfigureAwait(false);
        }
        protected async Task<ITransactionResult> WriteToDatabaseWithoutCommitAsync(Action writeAction)
        {
            return await WriteToDatabaseWithoutCommitAsync(
                writeAction,
                "MessageHelper.SuccessfulTransaction()",
                "MessageHelper.FailedTransaction()").ConfigureAwait(false);
        }

        /// <summary>
        ///   Performs the write operation with the specified entity but validates it first against the database and then according to the entity's own business rules.
        /// </summary>
        /// <param name="writeAction"> The write action. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        protected ITransactionResult WriteToDatabase(Action writeAction, string successMessage, string failureMessage)
        {
            return Action(UnitOfWork, writeAction, successMessage, failureMessage);
        }
        protected async Task<ITransactionResult> WriteToDatabaseAsync(Action writeAction, string successMessage, string failureMessage)
        {
            return await ActionAsync(UnitOfWork, writeAction, successMessage, failureMessage).ConfigureAwait(false);
        }
        protected async Task<ITransactionResult> WriteToDatabaseWithoutCommitAsync(Action writeAction, string successMessage, string failureMessage)
        {
            return await ActionWithoutCommitAsync(UnitOfWork, writeAction, successMessage, failureMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs an action against the database
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="writeAction">The write action.</param>
        /// <param name="successMessage">The success message.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private static ITransactionResult Action(IUnitOfWork unitOfWork, Action writeAction, string successMessage, string errorMessage)
        {
            ITransactionResult transactionResult = new FailedTransaction(GenericErrorMessage);

            for (var retryCounter = 1; retryCounter <= ApplicationSettings.TransactionRetryCount; retryCounter++)
            {
                using (var transaction = unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        writeAction();
                    }
                    catch (Exception ex)
                    {
                        transactionResult = unitOfWork.RollbackTransaction();

                        return new FailedTransaction(GenericErrorMessage, ex);
                    }

                    try
                    {
                        transactionResult = unitOfWork.CommitTransaction();

                        if (!transactionResult.IsSuccessful)
                            return transactionResult;

                        return new SuccessfulTransaction(successMessage);
                    }
                    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint") == true)
                    {
                        return new FailedTransaction("Messages.CANT_DELETE_USED_BY_OTHER_PROCESS");
                    }
                    catch (Exception ex)
                    {
                        transactionResult = unitOfWork.RollbackTransaction();

                        if (retryCounter < ApplicationSettings.TransactionRetryCount)
                        {
                            continue;
                        }

                        throw new Exception(errorMessage + " because of the following errors: " + ex.Message, ex);
                    }
                }
            }

            return transactionResult;
        }

        /// <summary>
        /// Performs an action against the database
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="writeAction">The write action.</param>
        /// <param name="successMessage">The success message.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private static async Task<ITransactionResult> ActionAsync(IUnitOfWork unitOfWork, Action writeAction, string successMessage, string errorMessage)
        {
            ITransactionResult transactionResult = new FailedTransaction(GenericErrorMessage);

            for (var retryCounter = 1; retryCounter <= ApplicationSettings.TransactionRetryCount; retryCounter++)
            {
                using (var transaction = await unitOfWork.BeginTransactionAsync().ConfigureAwait(false))
                {
                    try
                    {
                        writeAction();
                    }
                    catch (Exception ex)
                    {
                        transactionResult = await unitOfWork.RollbackTransactionAsync().ConfigureAwait(false);

                        return new FailedTransaction(GenericErrorMessage, ex);
                    }

                    try
                    {
                        transactionResult = await unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                        if (!transactionResult.IsSuccessful)
                            return transactionResult;

                        return new SuccessfulTransaction(successMessage);
                    }

                    catch (Exception ex)
                    {
                        transactionResult = await unitOfWork.RollbackTransactionAsync().ConfigureAwait(false);

                        if (retryCounter < ApplicationSettings.TransactionRetryCount)
                        {
                            continue;
                        }

                        throw new Exception(errorMessage + " because of the following errors: " + ex.Message, ex);
                    }
                }
            }

            return transactionResult;
        }

        private static async Task<ITransactionResult> ActionWithoutCommitAsync(IUnitOfWork unitOfWork, Action writeAction, string successMessage, string errorMessage)
        {
            ITransactionResult transactionResult = new FailedTransaction(GenericErrorMessage);

            for (var retryCounter = 0; retryCounter < ApplicationSettings.TransactionRetryCount; retryCounter++)
            {
                try
                {
                    writeAction();
                    return new SuccessfulTransaction(successMessage);
                }
                catch (Exception ex)
                {
                    transactionResult = await unitOfWork.RollbackTransactionAsync().ConfigureAwait(false);
                    return new FailedValidation(ex.Message).ToTransactionResult();
                }
            }
            return transactionResult;
        }

        public class TransactionChain
        {
            public virtual IDbContextTransaction Transaction { get; set; }

            public int ReferenceId;
            public string ReferenceNo;
            public TransactionChain(IUnitOfWork unitOfWork)
            {
                UnitOfWork = unitOfWork;
                Result = new SuccessfulTransaction();
            }

            public IUnitOfWork UnitOfWork { get; private set; }

            public ITransactionResult Result { get; set; }

            [DebuggerStepThrough]
            public TransactionChain Validate(Func<bool> func, string failureMessage)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = func() ? new SuccessfulTransaction() : (ITransactionResult)new FailedTransaction(failureMessage);

                if (!Result.IsSuccessful && UnitOfWork.IsActiveTransaction())
                    UnitOfWork.RollbackTransaction();

                return this;
            }

            public async Task<TransactionChain> ValidateAsync(Func<bool> func, string failureMessage)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = func() ? new SuccessfulTransaction() : (ITransactionResult)new FailedTransaction(failureMessage);

                if (!Result.IsSuccessful && UnitOfWork.IsActiveTransaction())
                    await UnitOfWork.RollbackTransactionAsync().ConfigureAwait(false);

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain Validate<T>(T entity, ValidationRule<T> rule)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = DataValidator.Validate(entity, rule).ToTransactionResult();

                return this;
            }

            [DebuggerStepThrough]
            public async Task<TransactionChain> ValidateAsync<T>(T entity, ValidationRule<T> rule)
            {
                if (!Result.IsSuccessful)
                    return this;

                var validateResult = await DataValidator.ValidateAsync(entity, rule).ConfigureAwait(false);

                Result = validateResult.ToTransactionResult();

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain Validate<T>(T entity, IEnumerable<ValidationRule<T>> rules)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = DataValidator.Validate(entity, rules).ToTransactionResult();

                return this;
            }

            [DebuggerStepThrough]
            public async Task<TransactionChain> ValidateAsync<T>(T entity, IEnumerable<ValidationRule<T>> rules)
            {
                if (!Result.IsSuccessful)
                    return this;

                var validateResult = await DataValidator.ValidateAsync(entity, rules).ConfigureAwait(false);

                Result = validateResult.ToTransactionResult();

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain Validate(Func<IOperationResult> func)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = func().ToTransactionResult();

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ValidateAsync(Func<IOperationResult> func)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = func().ToTransactionResult();

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain ValidateForEach<T>(IEnumerable<T> entities, Func<T, IOperationResult> func)
            {
                if (!Result.IsSuccessful)
                    return this;

                foreach (var entity in entities)
                {
                    Validate(func(entity).ToTransactionResult);
                }

                return this;
            }
            public async Task<TransactionChain> ValidateForEachAsync<T>(IEnumerable<T> entities, Func<T, IOperationResult> func)
            {
                if (!Result.IsSuccessful)
                    return this;

                foreach (var entity in entities)
                {
                    await ValidateAsync(func(entity).ToTransactionResult).ConfigureAwait(false);
                }

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain ValidateForEach<T>(IEnumerable<T> entities, Func<T, bool> rule, string message)
            {
                if (!Result.IsSuccessful)
                    return this;

                var invalidRecords = new List<T>();
                foreach (var entity in entities)
                {
                    var isValid = rule(entity);

                    if (!isValid)
                    {
                        invalidRecords.Add(entity);
                    }
                }

                if (invalidRecords.Any())
                {
                    var errorMessage = message + Environment.NewLine;

                    foreach (var invalidRecord in invalidRecords)
                    {
                        var identifier = invalidRecord.ToString();
                        errorMessage += Environment.NewLine + "- " + identifier;
                    }

                    Result = new FailedTransaction(errorMessage);
                }

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ValidateForEachAsync<T>(IEnumerable<T> entities, Func<T, bool> rule, string message)
            {
                if (!Result.IsSuccessful)
                    return this;

                var invalidRecords = new List<T>();
                foreach (var entity in entities)
                {
                    var isValid = rule(entity);

                    if (!isValid)
                    {
                        invalidRecords.Add(entity);
                    }
                }

                if (invalidRecords.Any())
                {
                    var errorMessage = message + Environment.NewLine;

                    foreach (var invalidRecord in invalidRecords)
                    {
                        var identifier = invalidRecord.ToString();
                        errorMessage += Environment.NewLine + "- " + identifier;
                    }

                    Result = new FailedTransaction(errorMessage);
                }

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain ValidateForEach<T>(IEnumerable<T> entities, ValidationRule<T> rule)
            {
                if (!Result.IsSuccessful)
                    return this;

                ExecuteForEach(entities, x => DataValidator.Validate(x, new[] { rule }).ToTransactionResult());

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ValidateForEachAsync<T>(IEnumerable<T> entities, ValidationRule<T> rule)
            {
                if (!Result.IsSuccessful)
                    return this;

                var executeForEachResult =

                await ExecuteForEachAsync(entities, x => DataValidator.Validate(x, new[] { rule }).ToTransactionResult()).ConfigureAwait(false);

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain ValidateForEach<T>(IEnumerable<T> entities, IEnumerable<ValidationRule<T>> rules)
            {
                if (!Result.IsSuccessful)
                    return this;

                ExecuteForEach(entities, x => DataValidator.Validate(x, rules).ToTransactionResult());

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ValidateForEachAsync<T>(IEnumerable<T> entities, IEnumerable<ValidationRule<T>> rules)
            {
                if (!Result.IsSuccessful)
                    return this;

                await ExecuteForEachAsync(entities, x => DataValidator.Validate(x, rules).ToTransactionResult()).ConfigureAwait(false);

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain ValidateForEach<T>(IEnumerable<T> entities, Func<T, IEnumerable<ValidationRule<T>>> func)
            {
                if (!Result.IsSuccessful)
                    return this;

                ExecuteForEach(entities, x => DataValidator.Validate(x, func(x)).ToTransactionResult());

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ValidateForEachAsync<T>(IEnumerable<T> entities, Func<T, IEnumerable<ValidationRule<T>>> func)
            {
                if (!Result.IsSuccessful)
                    return this;

                await ExecuteForEachAsync(entities, x => DataValidator.Validate(x, func(x)).ToTransactionResult()).ConfigureAwait(false);

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain Execute(Action action)
            {
                if (!Result.IsSuccessful)
                    return this;

                action();

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ExecuteAsync(Action action)
            {
                if (!Result.IsSuccessful)
                    return this;

                action();

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain Execute(Func<ITransactionResult> action)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = action();

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ExecuteAsync(Func<ITransactionResult> action)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = action();

                return this;
            }

            [DebuggerStepThrough]
            public TransactionChain ExecuteForEach<T>(IEnumerable<T> entities, params Func<T, ITransactionResult>[] transactions)
            {
                if (!Result.IsSuccessful)
                    return this;

                var failedTransactions = new Dictionary<T, string>();
                foreach (var entity in entities)
                {
                    foreach (var transaction in transactions)
                    {
                        var result = transaction(entity);

                        if (!result.IsSuccessful)
                        {
                            failedTransactions.Add(entity, result.Message);
                        }
                    }
                }

                if (failedTransactions.Any())
                {
                    var errorMessages = Environment.NewLine + "Failed to process the following records: ";

                    foreach (var failedSave in failedTransactions)
                    {
                        var identifier = failedSave.Key.ToString();
                        if (!string.IsNullOrWhiteSpace(identifier))
                        {
                            errorMessages += Environment.NewLine + identifier + ": ";
                        }
                        errorMessages += Environment.NewLine + failedSave.Value;
                    }

                    Result = new FailedTransaction(errorMessages);
                }

                return this;
            }
            [DebuggerStepThrough]
            public async Task<TransactionChain> ExecuteForEachAsync<T>(IEnumerable<T> entities, params Func<T, ITransactionResult>[] transactions)
            {
                if (!Result.IsSuccessful)
                    return this;

                var failedTransactions = new Dictionary<T, string>();
                foreach (var entity in entities)
                {
                    foreach (var transaction in transactions)
                    {
                        var result = transaction(entity);

                        if (!result.IsSuccessful)
                        {
                            failedTransactions.Add(entity, result.Message);
                        }
                    }
                }

                if (failedTransactions.Any())
                {
                    var errorMessages = Environment.NewLine + "Failed to process the following records: ";

                    foreach (var failedSave in failedTransactions)
                    {
                        var identifier = failedSave.Key.ToString();
                        if (!string.IsNullOrWhiteSpace(identifier))
                        {
                            errorMessages += Environment.NewLine + identifier + ": ";
                        }
                        errorMessages += Environment.NewLine + failedSave.Value;
                    }

                    Result = new FailedTransaction(errorMessages);
                }

                return this;
            }

            public TransactionChain ExecuteWithTransaction(Action action, string successMessage = null, string failureMessage = null)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = Action(
                    unitOfWork: UnitOfWork,
                    writeAction: action,
                    successMessage: successMessage ?? string.Empty,
                    errorMessage: failureMessage ?? string.Empty);

                return this;
            }

            public async Task<TransactionChain> ExecuteWithTransactionAsync(Action action, string successMessage = null, string failureMessage = null)
            {
                if (!Result.IsSuccessful)
                    return this;

                Result = await ActionAsync(
                    unitOfWork: UnitOfWork,
                    writeAction: action,
                    successMessage: successMessage ?? string.Empty,
                    errorMessage: failureMessage ?? string.Empty)
                    .ConfigureAwait(false);

                return this;
            }

            public async Task<TransactionChain> ExecuteWithoutCommitAsync(Action action, string successMessage = null, string failureMessage = null)
            {
                if (!Result.IsSuccessful)
                    return this;

                if (!UnitOfWork.IsActiveTransaction())
                {
                    await UnitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                }

                Result = await ActionWithoutCommitAsync(
                    unitOfWork: UnitOfWork,
                    writeAction: action,
                    successMessage: successMessage ?? string.Empty,
                    errorMessage: failureMessage ?? string.Empty)
                    .ConfigureAwait(false);

                return this;
            }

            public async Task<ITransactionResult> ExecuteCommitAsync(string successMessage = null, string failureMessage = null)
            {
                if (!Result.IsSuccessful)
                {
                    if (UnitOfWork.IsActiveTransaction())
                    {
                        await UnitOfWork.RollbackTransactionAsync().ConfigureAwait(false);
                        return Result;
                    }

                }

                if (successMessage == null) successMessage ="";

                if (failureMessage == null) failureMessage = "";

                return Result = await CommitTransaction(
                    unitOfWork: UnitOfWork,
                    successMessage: successMessage ?? string.Empty,
                    errorMessage: failureMessage ?? string.Empty)
                    .ConfigureAwait(false);
            }

            private static async Task<ITransactionResult> CommitTransaction(IUnitOfWork unitOfWork, string successMessage, string errorMessage)
            {
                ITransactionResult transactionResult = new FailedTransaction(GenericErrorMessage);

                for (var retryCounter = 1; retryCounter <= ApplicationSettings.TransactionRetryCount; retryCounter++)
                {
                    try
                    {
                        transactionResult = await unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                        if (!transactionResult.IsSuccessful)
                            return transactionResult;

                        return new SuccessfulTransaction(successMessage);
                    }
                    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint") == true)
                    {
                        return new FailedTransaction("");
                    }
                    catch (Exception ex)
                    {
                        transactionResult = await unitOfWork.RollbackTransactionAsync().ConfigureAwait(false);
                        return new FailedValidation(ex.Message).ToTransactionResult();
                    }
                }
                return transactionResult;
            }

            public async Task<ITransactionResult> RollbackTransactionAsync()
            {
                if (UnitOfWork.IsActiveTransaction())
                {
                    await UnitOfWork.RollbackTransactionAsync().ConfigureAwait(false);
                }

                return Result;
            }

            public async Task<TransactionChain> FailTransactionAsync(string errorMessage)
            {
                if (UnitOfWork.IsActiveTransaction())
                {
                    await UnitOfWork.RollbackTransactionAsync().ConfigureAwait(false);
                }

                Result = new FailedTransaction(errorMessage);

                return this;
            }


        }
    }
}
