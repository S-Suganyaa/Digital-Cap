using Dapper;
using DapperExtensions;
using DigitalCap.Core.Helpers;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace DigitalCap.Persistence.Repositories
{
    public abstract class RepositoryBase<TEntity, TKey>
        : IRepositoryBase<TEntity, TKey>
        where TEntity : EntityBase<TKey>
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger _logger;
        protected readonly IDbProvider? _dbProvider;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        protected RepositoryBase(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        protected RepositoryBase(IUnitOfWork unitOfWork, IDbProvider dbProvider)
        {
            _unitOfWork = unitOfWork;
            _dbProvider = dbProvider;
        }
        protected RepositoryBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected RepositoryBase(IDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public void Rollback()
        {
            _unitOfWork?.Rollback();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Commit()
        {
            _unitOfWork?.Commit();
        }

        public virtual async Task<TEntity?> GetAsync(TKey id, bool includeDeleted = false, bool useTransaction = true)
        {
            try
            {
                var result = await Connection.GetAsync<TEntity?>(id, useTransaction ? Transaction : null);
                if (result == null || !includeDeleted && result.Deleted) return null;
                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="useTransaction"></param>
        /// <returns></returns>
        public virtual TEntity Get(TKey id, bool includeDeleted = false, bool useTransaction = true)
        {
            var result = Connection.Get<TEntity?>(id, useTransaction ? Transaction : null);
            if (result == null || !includeDeleted && result.Deleted) return null!;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useTransaction"></param>
        /// <returns></returns>
        public virtual async Task<IQueryable<TEntity>> GetAllAsync(bool useTransaction = true)
        {
            var whereClause = "(Deleted = 0)";
            var tableName = typeof(TEntity).CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name.Equals("TableAttribute"))!
                .ConstructorArguments[0].Value;
            var schemaName = typeof(TEntity).CustomAttributes
               .First(x => x.AttributeType.Name.Equals("TableAttribute"))
               .NamedArguments;
            if (schemaName.Count > 0)
            {
                tableName = schemaName[0].TypedValue.Value + "." + tableName;
            }
            return await Task.Run(() => ExecuteQuery($"SELECT * FROM {tableName} WHERE {whereClause}"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useTransaction"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetAll(bool useTransaction = true)
        {
            var whereClause = "(Deleted = 0)";
            var tableName = typeof(TEntity).CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name.Equals("TableAttribute"))!
                .ConstructorArguments[0].Value;
            var schemaName = typeof(TEntity).CustomAttributes
               .First(x => x.AttributeType.Name.Equals("TableAttribute"))
               .NamedArguments;
            if (schemaName.Count > 0)
                tableName = $"[{schemaName[0].TypedValue.Value}] . [{tableName}]";
            return ExecuteQuery($"SELECT * FROM {tableName} WHERE {whereClause}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter)
        {
            var whereClause = LinqHelper.ToWhereClause(filter) + " AND (Deleted = 0)";
            var tableName = typeof(TEntity).CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name.Equals("TableAttribute"))!
                .ConstructorArguments[0].Value;
            var schemaName = typeof(TEntity).CustomAttributes
               .First(x => x.AttributeType.Name.Equals("TableAttribute"))
               .NamedArguments;
            if (schemaName.Count > 0)
            {
                tableName = $"[{schemaName[0].TypedValue.Value}] . [{tableName}]";
            }
            return ExecuteQuery($"SELECT * FROM {tableName} WHERE {whereClause}");
        }

        public virtual async Task<IQueryable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, bool IncludeDeleted = false)
        {
            var whereClause = LinqHelper.ToWhereClause(filter) + (IncludeDeleted ? "" : " AND (Deleted = 0)");
            var tableName = typeof(TEntity).CustomAttributes
                .First(x => x.AttributeType.Name.Equals("TableAttribute"))
                .ConstructorArguments[0].Value;
            var schemaName = typeof(TEntity).CustomAttributes
                .First(x => x.AttributeType.Name.Equals("TableAttribute"))
                .NamedArguments;
            if (schemaName.Count > 0)
                tableName = $"[{schemaName[0].TypedValue.Value}] . [{tableName}]";

            return await ExecuteQueryAsync($"SELECT * FROM {tableName} WHERE {whereClause}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> filter) => GetFiltered(filter).FirstOrDefault()!;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual TEntity First(Expression<Func<TEntity, bool>> filter) => GetFiltered(filter).First();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> ExecuteQuery(string sql)
        {
            var result = Connection.Query<TEntity>(
                sql: sql,
                commandType: CommandType.Text,
                transaction: Transaction);
            return result.AsQueryable();
        }

        public virtual async Task<IQueryable<TEntity>> ExecuteQueryAsync(string sql)
        {
            var result = await Connection.QueryAsync<TEntity>(
                sql: sql,
                commandType: CommandType.Text,
                transaction: Transaction);
            return result.AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> ExecuteQuery(string sql, object[] parameters)
        {
            var result = Connection.Query<TEntity>(
                sql: sql,
                commandType: CommandType.Text,
                param: parameters,
                transaction: Transaction);
            return result.AsQueryable();
        }
        public void SoftDeleteById(string spname, Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(spname, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add the parameter for the stored procedure
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                    // Execute the stored procedure
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual IQueryable<T> ExecuteStoreProc<T>(string sql, object parameters)
        {
            var result = Connection.Query<T>(
                sql: sql,
                commandType: CommandType.StoredProcedure,
                param: parameters,
                transaction: Transaction);
            return result.AsQueryable();
        }

        public virtual async Task<IQueryable<T>> ExecuteStoreProcAsync<T>(string sql, object parameters)
        {
            var result = await Connection.QueryAsync<T>(
                sql: sql,
                commandType: CommandType.StoredProcedure,
                param: parameters,
                transaction: Transaction);
            return result.AsQueryable();
        }

        public Task<TKey> InsertAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TKey Insert(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void InsertMany(IEnumerable<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        public int BulkInsert(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public bool Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateMany(List<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(TKey id)
        {
            throw new NotImplementedException();
        }

        public bool Delete(TKey id)
        {
            throw new NotImplementedException();
        }

        public void DeleteFiltered(Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetClassNumbers()
        {
            throw new NotImplementedException();
        }

        public void RemoveCacheKeysByValue(string[] keyValue, string uniqueId, string contextInfo = "")
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateQueryExecutionAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
