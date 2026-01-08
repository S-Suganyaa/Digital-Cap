using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IFileStorageRepository
    {
        Task<IEnumerable<ProjectFile>> GetProjectFiles(int projectId);
        Task DeleteFile(int fileId,string currentUser);
    }
}
