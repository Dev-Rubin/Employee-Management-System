using EMS.Domain.Entities.Common;
using EMS.Infrastructure.Common;
using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Infrastructure.Persistence.Service
{
    public abstract class BasicCrudService<T, TId> : CrudService, IBasicCrudService<T, TId>  where T : BaseEntity<TId>
    {
        protected BasicCrudService(IUnitOfWork unitOfWork, IRepository repository, IQueries queries) : base(unitOfWork, repository, queries)
        {
            OnInit();
        }

        protected BasicCrudService(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        protected virtual void OnInit()
        {
        }
        protected virtual IEnumerable<ValidationRule<T>> SaveRules
        {
            get { yield break; }
        }

        protected virtual IEnumerable<ValidationRule<T>> UpdateRules
        {
            get { yield break; }
        }

        protected virtual IEnumerable<ValidationRule<T>> DeleteRules
        {
            get { yield break; }
        }

        public virtual ITransactionResult Delete(IEnumerable<TId> ids)
        {


            ids = ids.ToList();
            var entities =
                ids.Select(id => UnitOfWork.Set<T>().FirstOrDefault(x => x.Id.Equals(id)))
                    .Where(x => x != null)
                    .ToList();

            return Delete(entities);
        }
        public virtual async Task<ITransactionResult> DeleteAsync(IEnumerable<TId> ids)
        {


            ids = ids.ToList();
            var entities =
                ids.Select(id => UnitOfWork.Set<T>().FirstOrDefault(x => x.Id.Equals(id)))
                    .Where(x => x != null)
                    .ToList();

            return await DeleteAsync(entities).ConfigureAwait(false);
        }

        public virtual ITransactionResult Delete(IEnumerable<T> entities)
        {
            var recordCount = entities.Count();

            return Transact
                .ExecuteForEach(
                    entities,
                    entity => DataValidator.Validate(entity, DeleteRules).ToTransactionResult(),
                    entity => OnBeforeDelete(entity))
                .ExecuteWithTransaction(
                    () =>
                    {
                        foreach (var entity in entities)
                        {
                            OnDeleteAction(entity);
                        }
                    },
                    "MessageHelper.SuccessfulDelete(recordCount)",
                    "MessageHelper.FailedDelete(recordCount)")
                .Result;
        }
        public virtual async Task<ITransactionResult> DeleteAsync(IEnumerable<T> entities)
        {
            var recordCount = entities.Count();

            var executeForEachResult = Transact
                .ExecuteForEach(
                    entities,
                    entity => DataValidator.Validate(entity, DeleteRules).ToTransactionResult(),
                    entity => OnBeforeDelete(entity));

            if (!executeForEachResult.Result.IsSuccessful)
                return new FailedTransaction("MessageHelper.FailedDelete(recordCount)");

            var executeWithTransactionResult = await executeForEachResult
                .ExecuteWithTransactionAsync(
                    () =>
                    {
                        foreach (var entity in entities)
                        {
                            OnDeleteAction(entity);
                        }
                    },
                    "MessageHelper.SuccessfulDelete(recordCount)",
                    "MessageHelper.FailedDelete(recordCount)")
                .ConfigureAwait(false);

            return executeWithTransactionResult.Result;
        }
        public virtual ITransactionResult Delete(T entity)
        {
            return Transact.Validate(entity, DeleteRules)
                .Execute(() => OnBeforeDelete(entity))
                .ExecuteWithTransaction(
                    () => Repository.Delete(entity),
                   " MessageHelper.SuccessfulDelete(1)",
                   " MessageHelper.FailedDelete(1)")
                .Result;
        }
        public virtual async Task<ITransactionResult> DeleteAsync(T entity)
        {

            var validateResult =
                await Transact.ValidateAsync(entity, DeleteRules)
                .ConfigureAwait(false);

            if (!validateResult.Result.IsSuccessful)
                return validateResult.Result;

            var executeResult =
                await validateResult
                .ExecuteAsync(() => OnBeforeDelete(entity))
                .ConfigureAwait(false);

            if (!executeResult.Result.IsSuccessful)
                return executeResult.Result;

            var executeWithTransactionResult =
                await executeResult.ExecuteWithTransactionAsync(
                    () => Repository.Delete(entity),
                    "MessageHelper.SuccessfulDelete(1)",
                   " MessageHelper.FailedDelete(1)")
                .ConfigureAwait(false);

            if (!executeWithTransactionResult.Result.IsSuccessful)
                return executeWithTransactionResult.Result;

            return executeWithTransactionResult.Result;
        }

        /// <summary>
        ///   Saves the specified entity but validates it first against the database and then according to the type's own business rules. Uses the default messages for saving.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Save(T entity)
        {
            return Save(
                entity,
                "MessageHelper.SuccessfulSave<T>()",
              "  MessageHelper.FailedSave<T>()");
        }

        /// <summary>
        ///   Saves the specified entity but validates it first against the database and then according to the type's own business rules. Uses the default messages for saving.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        public virtual async Task<ITransactionResult> SaveAsync(T entity)
        {
            return await SaveAsync(
                entity,
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()")
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Saves the specified entity but validates it first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Save(T entity, string successMessage, string failureMessage)
        {
            return Transact.Validate(entity, SaveRules)
                .Execute(() => OnBeforeSave(entity))
                .ExecuteWithTransaction(() => OnSaveAction(entity), successMessage, failureMessage)
                .Result;
        }

        /// <summary>
        ///   Saves the specified entity but validates it first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        public virtual async Task<ITransactionResult> SaveAsync(T entity, string successMessage, string failureMessage)
        {
            var validateResult = await Transact.ValidateAsync(entity, SaveRules).ConfigureAwait(false);

            if (!validateResult.Result.IsSuccessful)
                return validateResult.Result;

            var executeResult = await validateResult.ExecuteAsync(() => OnBeforeSave(entity)).ConfigureAwait(false);

            if (!executeResult.Result.IsSuccessful)
                return validateResult.Result;

            var executeWithTransactionResult = await
                executeResult
                .ExecuteWithTransactionAsync(() => OnSaveAction(entity), successMessage, failureMessage).ConfigureAwait(false);

            return executeWithTransactionResult.Result;
        }

        /// <summary>
        ///   Saves the specified entities but validates it first against the database and then according to the type's own business rules. Uses the default messages for saving
        /// </summary>
        /// <param name="entities"> The entities. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Save(IEnumerable<T> entities)
        {
            return Save(
                entities,
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()");
        }
        /// <summary>
        ///   Saves the specified entities but validates it first against the database and then according to the type's own business rules. Uses the default messages for saving
        /// </summary>
        /// <param name="entities"> The entities. </param>
        /// <returns> </returns>
        public virtual async Task<ITransactionResult> SaveAsync(IEnumerable<T> entities)
        {
            return await SaveAsync(
                entities,
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()")
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Saves the specified entities but validates it first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entities"> The entities. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Save(IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            return Transact
                .ExecuteForEach(
                    entities,
                    entity => DataValidator.Validate(entity, SaveRules).ToTransactionResult(),
                    OnBeforeSave)
                .ExecuteWithTransaction(
                    () =>
                    {
                        foreach (var entity in entities)
                        {
                            OnSaveAction(entity);
                        }
                    },
                    successMessage,
                    failureMessage)
                .Result;
        }
        public virtual async Task<ITransactionResult> SaveAsync(IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            var executeForEachResult = await Transact
                .ExecuteForEachAsync(
                    entities,
                    entity => DataValidator.Validate(entity, SaveRules).ToTransactionResult(),
                    OnBeforeSave).ConfigureAwait(false);

            if (!executeForEachResult.Result.IsSuccessful)
                return executeForEachResult.Result;

            var executeWithTransactionResult = await executeForEachResult.ExecuteWithTransactionAsync(
                    () =>
                    {
                        foreach (var entity in entities)
                        {
                            OnSaveAction(entity);
                        }
                    },
                    successMessage,
                    failureMessage).ConfigureAwait(false);

            return executeWithTransactionResult.Result;
        }

        /// <summary>
        ///   Saves new entries and Updates old ones but validates it first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        public virtual ITransactionResult SaveOrUpdate(T entity)
        {
            return SaveOrUpdate(
                entity,
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()");
        }
        public virtual async Task<ITransactionResult> SaveOrUpdateAsync(T entity)
        {
            return await SaveOrUpdateAsync(
                entity,
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()")
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Saves new entries and Updates old ones but validates it first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        public virtual ITransactionResult SaveOrUpdate(T entity, string successMessage, string failureMessage)
        {
            if (entity.IsTransient())
            {
                return Save(entity, successMessage, failureMessage);
            }

            return Update(entity, successMessage, failureMessage);
        }

        public virtual async Task<ITransactionResult> SaveOrUpdateAsync(T entity, string successMessage, string failureMessage)
        {
            if (entity.IsTransient())
            {
                return await SaveAsync(entity, successMessage, failureMessage).ConfigureAwait(false);
            }

            return await UpdateAsync(entity, successMessage, failureMessage).ConfigureAwait(false);
        }

        /// <summary>
        ///   Saves new entries and Updates old ones but validates them first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entities"> The entities. </param>
        /// <returns> </returns>
        public virtual ITransactionResult SaveOrUpdate(IEnumerable<T> entities)
        {
            return SaveOrUpdate(
                entities,
                String.Format("Messages.SUCCESS_SAVE_1", typeof(T).GetClassDescription()),
                String.Format("Messages.FAILED_SAVE_1", typeof(T).GetClassDescription()));
        }
        public virtual async Task<ITransactionResult> SaveOrUpdateAsync(IEnumerable<T> entities)
        {
            return await SaveOrUpdateAsync(
                entities,
                String.Format("Messages.SUCCESS_SAVE_1", typeof(T).GetClassDescription()),
                String.Format("Messages.FAILED_SAVE_1", typeof(T).GetClassDescription()))
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Saves new entries and Updates old ones but validates them first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entities"> The entities. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        public virtual ITransactionResult SaveOrUpdate(IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            return Transact
                .ExecuteForEach(
                    entities,
                    entity => DataValidator.Validate(entity, entity.IsTransient() ? SaveRules : UpdateRules).ToTransactionResult(),
                    entity => entity.IsTransient() ? OnBeforeSave(entity) : OnBeforeUpdate(entity))
                .ExecuteWithTransaction(
                    () =>
                    {
                        foreach (var x in entities)
                        {
                            if (x.IsTransient())
                            {
                                OnSaveAction(x);
                            }
                            else
                            {
                                OnUpdateAction(x);
                            }
                        }
                    },
                    successMessage,
                    failureMessage)
                .Result;
        }

        private void OnUpdateAction(T x)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<ITransactionResult> SaveOrUpdateAsync(IEnumerable<T> entities, string successMessage, string failureMessage)
        {

            var executeForEachResult = await Transact
                .ExecuteForEachAsync(
                    entities,
                    entity => DataValidator.Validate(entity, entity.IsTransient() ? SaveRules : UpdateRules).ToTransactionResult(),
                    entity => entity.IsTransient() ? OnBeforeSave(entity) : OnBeforeUpdate(entity)).ConfigureAwait(false);

            if (!executeForEachResult.Result.IsSuccessful)
                return executeForEachResult.Result;

            var executeWithTransactionResult = await executeForEachResult.ExecuteWithTransactionAsync(
                    () =>
                    {
                        foreach (var x in entities)
                        {
                            if (x.IsTransient())
                            {
                                OnSaveAction(x);
                            }
                            else
                            {
                                OnUpdateAction(x);
                            }
                        }
                    },
                    successMessage,
                    failureMessage).ConfigureAwait(false);

            return executeWithTransactionResult.Result;
        }
        /// <summary>
        ///   Updates the specified entity but validates it first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Update(T entity)
        {
            return Update(
                entity,
                String.Format("Messages.SUCCESS_UPDATE_1", typeof(T).GetClassDescription()),
                String.Format("Messages.FAILED_UPDATE", typeof(T).GetClassDescription()));
        }
        public virtual async Task<ITransactionResult> UpdateAsync(T entity)
        {
            return await UpdateAsync(
                entity,
                String.Format("Messages.SUCCESS_UPDATE_1", typeof(T).GetClassDescription()),
                String.Format("Messages.FAILED_UPDATE", typeof(T).GetClassDescription()))
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Updates the specified entity but validates it first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Update(T entity, string successMessage, string failureMessage)
        {
            return Transact.Validate(entity, UpdateRules)
                .Execute(() => OnBeforeUpdate(entity))
                .ExecuteWithTransaction(() => OnUpdateAction(entity), successMessage, failureMessage)
                .Result;
        }
        public virtual async Task<ITransactionResult> UpdateAsync(T entity, string successMessage, string failureMessage)
        {
            var validateResult = await Transact.ValidateAsync(entity, UpdateRules).ConfigureAwait(false);

            if (!validateResult.Result.IsSuccessful)
                return validateResult.Result;

            var executeResult = await validateResult.ExecuteAsync(() => OnBeforeUpdate(entity)).ConfigureAwait(false);

            if (!executeResult.Result.IsSuccessful)
                return executeResult.Result;

            var executeWithTransactionResult = await
                executeResult
                .ExecuteWithTransactionAsync(() => OnUpdateAction(entity), successMessage, failureMessage)
                .ConfigureAwait(false);

            return executeWithTransactionResult.Result;
        }

        /// <summary>
        ///   Updates the specified entities but validates them first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entities"> The entities. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Update(IEnumerable<T> entities)
        {
            return Update(entities, "MessageHelper.SuccessfulSave<T>()", "MessageHelper.FailedUpdate<T>()");
        }
        public virtual async Task<ITransactionResult> UpdateAsync(IEnumerable<T> entities)
        {
            return await UpdateAsync(entities, "MessageHelper.SuccessfulSave<T>()", "MessageHelper.FailedUpdate<T>()").ConfigureAwait(false);
        }

        /// <summary>
        ///   Updates the specified entities but validates them first against the database and then according to the type's own business rules.
        /// </summary>
        /// <param name="entities"> The entities. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        public virtual ITransactionResult Update(IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            return Transact
                .ExecuteForEach(
                    entities,
                    entity => DataValidator.Validate(entity, UpdateRules).ToTransactionResult(),
                    OnBeforeUpdate)
                .ExecuteWithTransaction(
                    () =>
                    {
                        foreach (var entity in entities)
                        {
                            OnUpdateAction(entity);
                        }
                    },
                    successMessage,
                    failureMessage)
                .Result;
        }
        public virtual async Task<ITransactionResult> UpdateAsync(IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            var executeForEachResult =
                await
                Transact
                .ExecuteForEachAsync(
                    entities,
                    entity => DataValidator.Validate(entity, UpdateRules).ToTransactionResult(),
                    OnBeforeUpdate)
                .ConfigureAwait(false);

            if (!executeForEachResult.Result.IsSuccessful)
                return executeForEachResult.Result;

            var executeWithTransactionResult =
                await
                executeForEachResult.ExecuteWithTransactionAsync(
                    () =>
                    {
                        foreach (var entity in entities)
                        {
                            OnUpdateAction(entity);
                        }
                    },
                    successMessage,
                    failureMessage)
                .ConfigureAwait(false);

            return executeWithTransactionResult.Result;
        }

        /// <summary>
        ///   Called when [before save or update] for extra validation.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        protected virtual ITransactionResult OnBeforeSave(T entity)
        {
            var legacySaveOrUpdateResult = OnBeforeSaveOrUpdate(entity);

            return legacySaveOrUpdateResult;
        }

        /// <summary>
        ///   Called when [before save or update] for extra validation.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        protected virtual ITransactionResult OnBeforeUpdate(T entity)
        {
            var legacySaveOrUpdateResult = OnBeforeSaveOrUpdate(entity);

            return legacySaveOrUpdateResult;
        }


        /// <summary>
        ///   Called when [before delete] for extra validation.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        protected virtual ITransactionResult OnBeforeDelete(T entity)
        {
            return new SuccessfulTransaction();
        }

        /// <summary>
        ///   Called when [before save or update] for extra validation.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        [Obsolete("Marked for removal. Call .OnBeforeSave() or .OnBeforeUpdate() for actions done outside a transaction. Override .SaveRules or .UpdateRules for validations.")]
        protected virtual ITransactionResult OnBeforeSaveOrUpdate(T entity)
        {
            return new SuccessfulTransaction();
        }

        protected virtual void OnDeleteAction(T entity)
        {
            Repository.Delete(entity);


        }

        /// <summary>
        ///   Called when [save action].
        /// </summary>
        /// <param name="entity"> The entity. </param>
        protected virtual void OnSaveAction(T entity)
        {
            Repository.Save(entity);
        }

        /// <summary>
        ///   Called when [update action].
        /// </summary>
        /// <param name="entity"> The x. </param>
        /// <returns> </returns>
        //protected virtual void OnUpdateAction(T entity)
        //{
        //    var entityLoggingCreation = entity as ITrackedEntity;
        //    if (entityLoggingCreation != null)
        //    {
        //        entityLoggingCreation.LastUpdateLog = TransactionLogger.CreateLog();
        //    }
        //    Repository.Update(entity);
        //}

        //protected void PreventDeletionOfImmutableEntity(T entity, OperationResultEventArgs e)
        //{
        //    var immutableEntity = entity as IImmutableEntity;
        //    if (immutableEntity != null && immutableEntity.IsImmutable == true)
        //    {
        //        e.OperationResults.Add(new FailedTransaction("Cannot delete system-generated record."));
        //    }
        //}

        //protected void PreventUpdateOfImmutableEntity(T entity, OperationResultEventArgs e)
        //{
        //    if (!entity.IsTransient())
        //    {
        //        var immutableEntity = entity as IImmutableEntity;
        //        if (immutableEntity != null && immutableEntity.IsImmutable == true)
        //        {
        //            e.OperationResults.Add(new FailedTransaction("Cannot edit system-generated record."));
        //        }
        //    }
        //}

        /// <summary>
        ///   Performs the write operation with the specified entity but validates it first against the database and then according to the entity's own business rules.
        /// </summary>
        /// <param name="writeAction"> The write action. </param>
        /// <param name="entities"> The entities. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        protected ITransactionResult WriteBatchToDatabase(
            Action<T> writeAction, IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            var entitiesList = entities.ToList();

            return WriteToDatabase(
                () =>
                {
                    foreach (var entity in entitiesList)
                    {
                        writeAction(entity);
                    }
                },
                successMessage,
                failureMessage);
        }

        protected async Task<ITransactionResult> WriteBatchToDatabaseAsync(
           Action<T> writeAction, IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            var entitiesList = entities.ToList();

            return await WriteToDatabaseAsync(
                () =>
                {
                    foreach (var entity in entitiesList)
                    {
                        writeAction(entity);
                    }
                },
                successMessage,
                failureMessage)
                .ConfigureAwait(false);
        }

        protected async Task<ITransactionResult> WriteToDatabaseWithoutCommitAsync(
           Action<T> writeAction, IEnumerable<T> entities, string successMessage, string failureMessage)
        {
            var entitiesList = entities.ToList();

            return await WriteToDatabaseWithoutCommitAsync(
                () =>
                {
                    foreach (var entity in entitiesList)
                    {
                        writeAction(entity);
                    }
                },
                successMessage,
                failureMessage)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Performs the write operation with the specified entity but validates it first against the database and then according to the entity's own business rules. Uses the default messages for saving.
        /// </summary>
        /// <param name="writeAction"> The write action. </param>
        /// <param name="entity"> The entity. </param>
        /// <returns> </returns>
        protected ITransactionResult WriteToDatabase(Action<T> writeAction, T entity)
        {
            return WriteToDatabase(
                () => writeAction(entity),
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()");
        }

        protected async Task<ITransactionResult> WriteToDatabaseAsync(Action<T> writeAction, T entity)
        {
            return await WriteToDatabaseAsync(
                () => writeAction(entity),
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()")
                .ConfigureAwait(false);
        }

        protected async Task<ITransactionResult> WriteToDatabaseWithoutCommitAsync(Action<T> writeAction, T entity)
        {
            return await WriteToDatabaseWithoutCommitAsync(
                () => writeAction(entity),
                "MessageHelper.SuccessfulSave<T>()",
                "MessageHelper.FailedSave<T>()")
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Performs the write operation with the specified entity but validates it first against the database and then according to the entity's own business rules.
        /// </summary>
        /// <param name="writeAction"> The write action. </param>
        /// <param name="entity"> The entity. </param>
        /// <param name="successMessage"> The success message. </param>
        /// <param name="failureMessage"> The failure message. </param>
        /// <returns> </returns>
        [Obsolete("Use .WriteToDatabase(writeAction, successMessage, failureMessage)")]
        protected ITransactionResult WriteToDatabase(Action<T> writeAction, T entity, string successMessage, string failureMessage)
        {
            return WriteToDatabase(() => writeAction(entity), successMessage, failureMessage);
        }

        //protected delegate void BasicCrudEventHandler(T entity, OperationResultEventArgs e);

    }
}
