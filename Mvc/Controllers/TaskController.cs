using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Repositories.Models;
using System;
using System.Threading.Tasks;

namespace Mvc.Controllers
{
    public class TaskController : Controller
    {
        private readonly ItaskInterface _task;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ILogger<TaskController> logger, ItaskInterface itask)
        {
            _logger = logger;
            _task = itask;
        }

        // Renders the Kendo page
     [HttpGet]
public IActionResult HomePage()
{
    var name = HttpContext.Session.GetString("EmployeeName");
    var empId = HttpContext.Session.GetInt32("EmployeeId");  // ✅ match Login()

    if (empId == null)
        return RedirectToAction("Login", "Home");

    ViewBag.EmpName = name;
    ViewBag.EmpId = empId.Value;   // ✅ now it works (int, not nullable)

    return View(); // Views/Task/HomePage.cshtml
}


        // ===== Grid API =====

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var empId = GetCurrentEmpId();
                var items = await _task.GetAllTasksAsync(empId);
                return Json(new { data = items, total = items?.Count ?? 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "List error");
                return BadRequest(new { error = "Failed to fetch tasks." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var empId = GetCurrentEmpId();
                var item = await _task.GetTaskByIdAsync(id, empId);
                if (item == null) return NotFound();
                return Json(new { data = item });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get error");
                return BadRequest(new { error = "Failed to fetch task." });
            }
        }

      [HttpPost]
[Consumes("application/json")]
public async Task<IActionResult> Create([FromBody] t_tasks model)
{
    if (model == null)
        return BadRequest(new { error = "Empty request body." });

    // ensure logged-in user
    var empId = HttpContext.Session.GetInt32("EmployeeId");
    if (empId == null || empId <= 0)
        return Unauthorized(new { error = "You must be logged in." });

    // enforce current user + reasonable defaults to satisfy DB constraints
    model.c_empid        = empId.Value;
    model.c_title        = model.c_title        ?? string.Empty;
    model.c_description  = model.c_description  ?? string.Empty;
    model.c_status       = string.IsNullOrWhiteSpace(model.c_status) ? "Pending" : model.c_status;
    if (model.c_estimateddays <= 0) model.c_estimateddays = 1;  // avoid 0 if DB requires > 0

    // Return validation problems clearly if any
    if (!ModelState.IsValid)
        return BadRequest(new { error = "Validation failed.", details = ModelState });

    try
    {
        var newId  = await _task.AddTaskAsync(model);
        var saved  = await _task.GetTaskByIdAsync(newId, empId.Value);
        return Json(new { data = saved });
    }
    catch (Exception ex)
    {
        // log if you want: _logger.LogError(ex, "Create error");
        return BadRequest(new { error = "Failed to create task.", details = ex.Message });
    }
}


        [HttpPost]
        public async Task<IActionResult> Update([FromBody] t_tasks model)
        {
        
            if (model == null || model.c_taskid <= 0) return BadRequest();

            try
            {
                var empId = GetCurrentEmpId();
                model.c_empid = empId; // enforce current user

                var rows = await _task.UpdateTaskAsync(model, empId);
                if (rows <= 0) return NotFound();

                var updated = await _task.GetTaskByIdAsync(model.c_taskid, empId);
                return Json(new { data = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update error");
                return BadRequest(new { error = "Failed to update task." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] t_tasks model)
        {
            if (model == null || model.c_taskid <= 0) return BadRequest();

            try
            {
                var empId = GetCurrentEmpId();
                var rows = await _task.DeleteTaskAsync(model.c_taskid, empId);
                if (rows <= 0) return NotFound();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete error");
                return BadRequest(new { error = "Failed to delete task." });
            }
        }

        // derive emp id from auth/session
        private int GetCurrentEmpId()
        {
            // Claims example: add this claim at login
            var claim = User?.FindFirst("empid")?.Value;
            if (int.TryParse(claim, out var fromClaim) && fromClaim > 0)
                return fromClaim;

            // Session fallback (make sure Session is enabled in Program.cs)
            var fromSession = HttpContext?.Session?.GetInt32("EmployeeId");
            if (fromSession.HasValue && fromSession.Value > 0)
                return fromSession.Value;

            // For dev only; in production redirect to login instead
        throw new UnauthorizedAccessException("Not logged in.");
        }
    }
}
