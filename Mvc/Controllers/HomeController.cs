using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Mvc.Models;
using Repositories.Implementations;
using Repositories.Interfaces;
using Repositories.Models;

namespace Mvc.Controllers;

public class HomeController : Controller
{
    private readonly IEmployeeInterface _employeeRepo;

    public HomeController(IEmployeeInterface employeeRepo)
    {
        _employeeRepo = employeeRepo;
    }



    // [HttpGet]
    // public IActionResult SessionPing()
    // {
    //     var data = new
    //     {
    //         Keys = HttpContext.Session.Keys?.ToArray(),
    //         EmployeeId = HttpContext.Session.GetInt32("EmployeeId"),
    //         EmployeeName = HttpContext.Session.GetString("EmployeeName"),
    //         EmpId = HttpContext.Session.GetInt32("EmpId"),
    //         EmpName = HttpContext.Session.GetString("EmpName"),
    //         Role = HttpContext.Session.GetString("Role"),
    //         SessionId = HttpContext.Session.Id
    //     };
    //     return Json(data);
    // }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(t_employees employee)
    {
        if (ModelState.IsValid)
        {
            string profileFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads/profiles");

            Directory.CreateDirectory(profileFolder); // Ensure folder exists

            if (employee.profilepicture != null && employee.profilepicture.Length > 0)
            {
                var fileName = employee.c_email + Path.GetExtension(employee.profilepicture.FileName);
                var filePath = Path.Combine(profileFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await employee.profilepicture.CopyToAsync(stream);
                }

                employee.c_profileimage = Path.Combine(fileName);
            }

            var status = await _employeeRepo.Register(employee);


            if (status == 1)
                return Json(new { success = true, message = "Registration successful!", redirectUrl = Url.Action("Login", "Home") });

            if (status == 0)
            {
                Console.WriteLine(status);
                return Json(new { success = false, message = "Email already registered" });
            }

            return Json(new { success = false, message = "Database error occurred" });
        }

        // For Model error show
        var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

        return Json(new { success = false, errors });
    }

    public IActionResult Index()
    {
        return View();
    }


    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(vm_Login login)
    {
       
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return Json(new { success = false, errors });
        }

        t_employees empData = await _employeeRepo.Login(login);

        if (empData != null && empData.c_empid != 0)
        {
            // Store session data
            HttpContext.Session.SetInt32("EmployeeId", empData.c_empid);
            HttpContext.Session.SetString("EmployeeName", empData.c_ename);
            HttpContext.Session.SetString("Role", empData.c_role);


            Console.WriteLine(HttpContext.Session.GetInt32("EmployeeId"));
            Console.WriteLine(HttpContext.Session.GetString("EmployeeName"));
            Console.WriteLine(HttpContext.Session.GetString("Role"));


            // Redirect based on role
            if (empData.c_role == "Admin")
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "AdminTask") });

            }
            else if (empData.c_role == "Employee")
            {
                return Json(new { success = true, redirectUrl = Url.Action("HomePage", "Task") });
            }
            else
            {
                return Json(new { success = false, message = "Unauthorized Role" });

            }
        }

        return Json(new { success = false, message = "Invalid Email or Password" });
    }
    [HttpGet]
    public async Task<IActionResult> Logout()
    {

        HttpContext.Session.Clear();                                                            
        return RedirectToAction("Login", "Home");  
    }
}

