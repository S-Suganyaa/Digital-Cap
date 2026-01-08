using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class FileStorageRepository : IFileStorageRepository
    {
        private readonly IBlobStorageRepository _blobStorageRepository;
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;

        public async Task<IEnumerable<ProjectFile>> GetProjectFiles(int projectId)
        {
            var result = await Connection.QueryAsync<ProjectFile>(
                sql: "CAP.Read_Files_ByProjectId",
                param: new
                {
                    ProjectId = projectId
                },
                transaction: Transaction,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task DeleteFile(int fileId, string currentUser)
        {
            await Connection.ExecuteAsync(
                sql: "CAP.Delete_File",
                param: new
                {
                    Id = fileId,
                    User = currentUser
                },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );
        }


        public async Task<T> GetFile<T>(int id)
    where T : ProjectFile, new()
        {
            var file = (await Connection.QueryAsync<T>(
                "CAP.Read_File_ById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            )).SingleOrDefault();

            return file;
        }

    }
}
