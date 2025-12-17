using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IEmployeeInterface
    {   
        //ep part
         Task<t_employees> GetUser(int empId);
        Task<bool> UpdateProfile(int empId, t_employees emp);
        Task<bool> ChangePassword(int empId, vm_ChangePassword model);
        Task<int> Register(t_employees employee);
        Task<t_employees> Login(vm_Login employee);
    }
}