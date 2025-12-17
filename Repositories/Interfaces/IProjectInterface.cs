using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IProjectInterface
    {
        //ep part
        Task<List<t_projects>> GetAll(string? filter = null);
        Task<int> Add(t_projects projectData);
        Task<int> Update(t_projects projectData);
        Task<int> Delete(string projectid);
        //Task<List<t_projects>> GetAllProjectsAsync();
    
    }
}