using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface ItaskInterface
    {
        Task<List<t_taskaccurarcy>> GetTaskAccuracyAsync(int empid);
        Task<List<t_taskaccurarcy>> OverallAccuracyAsync();
        //ep part
        
        Task<List<t_tasks>> GetAllTasksAsync(int empId);
        Task<t_tasks?> GetTaskByIdAsync(int taskId, int empId);
        Task<int> AddTaskAsync(t_tasks task);
        Task<int> UpdateTaskAsync(t_tasks task, int empId);
        Task<int> DeleteTaskAsync(int taskId, int empId);
    }
}