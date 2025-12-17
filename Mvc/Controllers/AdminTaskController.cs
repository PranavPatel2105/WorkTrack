using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;

namespace Mvc.Controllers
{
    
    public class AdminTaskController : Controller
    {
        private readonly ItaskInterface _task;
        private readonly ILogger<AdminTaskController> _logger;

        public AdminTaskController(ILogger<AdminTaskController> logger, ItaskInterface itask)
        {
            _logger = logger;
            _task = itask;
        }

        public async Task<IActionResult> Index()
        { 
            return View();
        }
      
        public async Task<IActionResult> GetAllOverAccuracy()
        {
            var tasks = await _task.OverallAccuracyAsync();
            foreach(var task in tasks)
             if (tasks == null)
                return BadRequest(new { success = false, message = "There was no Tasks found" });
            return Ok(tasks);
        }

        public async Task<IActionResult> GetAllTaskByEmpId(int Empid)
        {
            var tasks = await _task.GetTaskAccuracyAsync(Empid);
            if (tasks == null)
                return BadRequest(new { success = false, message = "There was no Tasks found" });
            return Ok(tasks);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}