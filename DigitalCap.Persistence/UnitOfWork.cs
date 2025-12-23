using DigitalCap.Core.Interfaces.Repository;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DigitalCap.Persistence
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly string _connectionString;
        private bool _disposed = false;
        public IDbConnection? Connection { get; private set; }
        public IDbTransaction? Transaction { get; private set; }

        /// <summary>
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public UnitOfWork(string connectionString)
        {
            _connectionString = connectionString;
            Begin();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Rollback()
        {
            Transaction?.Rollback();
            Begin();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Commit()
        {
            if (Transaction?.Connection != null)
                Transaction.Commit();
            Begin();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Begin()
        {
            if (Connection == null)
                Connection = new SqlConnection(_connectionString);

            if (Connection.State == ConnectionState.Closed)
                Connection.Open();

            if (Transaction != null)
                Transaction.Dispose();

            Transaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Transaction?.Dispose();
                Transaction = null;
                Connection?.Close();
                Connection?.Dispose();
                Connection = null;
            }
            _disposed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
