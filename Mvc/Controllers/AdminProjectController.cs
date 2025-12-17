using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Repositories.Models;

namespace Mvc.Controllers
{

    public class AdminProjectController : Controller
    {
        private readonly IProjectInterface _project;
        private readonly ILogger<AdminProjectController> _logger;

        public AdminProjectController(ILogger<AdminProjectController> logger, IProjectInterface project)
        {
            _logger = logger;
            _project = project;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
        
        public async Task<ActionResult> GetAll(string id)
        {
            var project = await _project.GetAll();
            if (project == null)
                return BadRequest(new { success = false, message = "There was no project found" });
            return Ok(project);
        }
        public async Task<ActionResult> GetProjectById(string id)
        {
            var project = await _project.GetAll("c_projectid=" + id);
            // var singleProject = project.FirstOrDefault();
            if (project == null)
                return BadRequest(new { success = false, message = "There was no project found" });
            return Ok(project);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] t_projects projects)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new {message=ModelState.ToDictionary(
                    kvp=>kvp.Key,
                    kvp=>kvp.Value.Errors.Select(e=>e.ErrorMessage).ToArray()
                )});
            }

            if (HttpContext.Session.GetInt32("EmployeeId") == null)
            {
                return Unauthorized(new { success = false, message = "Employee is not logged in" });
            }

            int result;

            if (projects.c_projectid == 0)
            {
                result = await _project.Add(projects);
            }
            else
            {
                result = await _project.Update(projects);
            }

            if (result == 0)
            {
                return BadRequest(new { success = false, message = "There was some error while saving the project" });
            }

            return Ok(new { success = true, message = "Project saved successfully" });
        }
        
        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
            if (HttpContext.Session.GetInt32("EmployeeId") == null)
            {
                return Unauthorized(new { success = false, message = "Employee is not logged in" });
            }

            int status = await _project.Delete(id);
            if (status == 1)
            {
                return Ok(new { success = true, message = "Project Deleted Successfully" });
            }
            else if (status == -1)
            {
                return BadRequest(new { success = false, message = "Project cannot be deleted as it consists of one or more tasks" });
            
            }
            else
            {
                return BadRequest(new { success = false, message = "There was some error while deleting Project" });
            }

        }


        

        
    }
}