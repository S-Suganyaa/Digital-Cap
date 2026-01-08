using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface ITaskRepository 
    {
        Task<byte> UpdateProjectPercentComplete(int projectId);
        Task<IEnumerable<SingleTask>> GetCap2Tasks(int projectId);
        Task<List<TaskStatusGroup>> GetStatusGroups(string v);
        Task<IEnumerable<SingleTask>> GetTasks(int projectId);
    }
}
