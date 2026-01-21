using EMS.Domain.Entities.Common;
using EMS.Infrastructure.Persistence.Interface;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Persistence.Service
{
    public class Repository : IRepository, IDisposable
    {
        private string _errorMessage = string.Empty;
        private bool _isDisposed;
        private readonly IUnitOfWork _unitOfWork;


        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public void Dispose()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
            _isDisposed = true;
        }
        public virtual void Save<T>(T entity) where T : class
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("Entity");
                }

                _unitOfWork.Set<T>().Add(entity);
                _unitOfWork.Entry(entity).State = EntityState.Added;
            }
            catch (DbUpdateException dbEx)
            {
                HandleUnitOfWorkException(dbEx);
                throw new Exception(_errorMessage, dbEx);
            }
        }

        public virtual void SaveUpdate<T>(T entity) where T : BaseEntity<int>
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("Entity");
                }

                if (entity.Id <= 0)
                {
                    entity.AddedDate = DateTime.Now;
                    _unitOfWork.Set<T>().Add(entity);
                    _unitOfWork.Entry(entity).State = EntityState.Added;
                }
                else
                {
                    entity.UpdatedDate = DateTime.Now;
                    _unitOfWork.Set<T>().Update(entity);
                    _unitOfWork.Entry(entity).State = EntityState.Modified;
                }
            }
            catch (DbUpdateException dbEx)
            {
                HandleUnitOfWorkException(dbEx);
                throw new Exception(_errorMessage, dbEx);
            }
        }
        public virtual void Update<T>(T entity) where T : class
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("Entity");
                }

                _unitOfWork.Set<T>().Update(entity); 
                _unitOfWork.Entry(entity).State = EntityState.Modified;
            }
            catch (DbUpdateException dbEx)
            {
                HandleUnitOfWorkException(dbEx);
                throw new Exception(_errorMessage, dbEx);
            }
        }
        public virtual void Delete<T>(T entity)  where T : class
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("Entity");
                }
                _unitOfWork.Set<T>().Remove(entity);
                _unitOfWork.Entry(entity).State = EntityState.Deleted;
            }
            catch (DbUpdateException dbEx)
            {
                HandleUnitOfWorkException(dbEx);
                throw new Exception(_errorMessage, dbEx);
            }
        }
        private void HandleUnitOfWorkException(DbUpdateException dbEx)
        {

        }
    }
}
