using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Repositories.Interfaces;
using Repositories.Models;

namespace Mvc.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeInterface _employee;
        private readonly ILogger<EmployeeController> _logger;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(
            ILogger<EmployeeController> logger,
            IEmployeeInterface employee,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _employee = employee;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            try
            {

                //testing 
                var sessionEmpId = HttpContext.Session.GetInt32("EmployeeId");
                if (!sessionEmpId.HasValue)
                    return Unauthorized(new { error = "You must be logged in." });

                if (sessionEmpId.Value != id)
                    return Forbid();
                var emp = await _employee.GetUser(id);
                if (emp == null) return NotFound();
                return Json(new { data = emp });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile get error");
                return BadRequest(new { error = "Failed to fetch employee profile." });
            }
        }

        // [HttpPost]
        // public async Task<IActionResult> UpdateProfile(int id, [FromForm] string c_ename, IFormFile? profilepicture)
        // {
        //     // if (id <= 0 || string.IsNullOrWhiteSpace(c_ename))
        //     //     return BadRequest(new { error = "Invalid employee id or name." });
        //     // Validate session
        //     var sessionEmpId = HttpContext.Session.GetInt32("EmployeeId")
        //                      ?? HttpContext.Session.GetInt32("EmpId");

        //     if (!sessionEmpId.HasValue)
        //         return Unauthorized(new { error = "You must be logged in." });

        //     if (sessionEmpId.Value != id)
        //         return Forbid();

        //     if (id <= 0 || string.IsNullOrWhiteSpace(c_ename))
        //         return BadRequest(new { error = "Invalid employee id or name." });

        //     try
        //     {
        //         string savedRelativePath = null;

        //         // if (profilepicture != null && profilepicture.Length > 0)
        //         // {
        //         //     var root = _env.WebRootPath ?? "wwwroot";
        //         //     var folder = Path.Combine(root, "uploads", "profiles");
        //         //     Directory.CreateDirectory(folder);

        //         //     // var fileName = $"{Guid.NewGuid()}{Path.GetExtension(profilepicture.FileName)}";
        //         //       var fileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads/profiles");
        //         //     var fullPath = Path.Combine(folder, fileName);

        //         //     await using (var fs = System.IO.File.Create(fullPath))
        //         //         await profilepicture.CopyToAsync(fs);

        //         //     savedRelativePath = $"/uploads/profiles/{fileName}";
        //         // }

        //         if (profilepicture != null && profilepicture.Length > 0)
        //         {
        //             var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        //             var folder = Path.Combine(root, "uploads", "profiles");
        //             Directory.CreateDirectory(folder);

        //             // ✅ Generate a unique file name only
        //             var fileName = $"{Guid.NewGuid()}{Path.GetExtension(profilepicture.FileName)}";

        //             // ✅ Combine folder and file name to get full absolute path
        //             var fullPath = Path.Combine(folder, fileName);

        //             // ✅ Save file to the path
        //             await using (var fs = new FileStream(fullPath, FileMode.Create))
        //                 await profilepicture.CopyToAsync(fs);

        //             // ✅ Store relative path for database / frontend use
        //             savedRelativePath = $"/uploads/profiles/{fileName}";
        //         }


        //         var model = new t_employees
        //         {
        //             c_ename = c_ename.Trim(),
        //             c_profileimage = savedRelativePath
        //         };

        //         var ok = await _employee.UpdateProfile(id, model);
        //         if (!ok) return NotFound(new { error = "Employee not found or update failed." });

        //         var updated = await _employee.GetUser(id);
        //         return Json(new { data = updated, message = "Profile updated successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "UpdateProfile error");
        //         return BadRequest(new { error = "Failed to update profile." });
        //     }
        // }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(
    int id,
    [FromForm] string c_ename,
    IFormFile? profilepicture)
        {
            var sessionEmpId = HttpContext.Session.GetInt32("EmployeeId")
                              ?? HttpContext.Session.GetInt32("EmpId");
            if (!sessionEmpId.HasValue)
                return Unauthorized(new { error = "You must be logged in." });

            if (sessionEmpId.Value != id)
                return Forbid();

            if (id <= 0 || string.IsNullOrWhiteSpace(c_ename))
                return BadRequest(new { error = "Invalid employee id or name." });

            try
            {
                string? savedRelativePath = null;

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var profileFolder = Path.Combine(webRoot, "uploads", "profiles");
                Directory.CreateDirectory(profileFolder);

                if (profilepicture is not null && profilepicture.Length > 0)
                {
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(profilepicture.FileName).ToLowerInvariant();
                    if (!allowedExts.Contains(ext))
                        return BadRequest(new { error = "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed." });

                    var userEmail = HttpContext.Session.GetString("email");
                    var safeEmail = string.IsNullOrWhiteSpace(userEmail)
                        ? null
                        : string.Concat(userEmail.Where(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-'));

                    var baseName = safeEmail ?? $"emp_{id}";
                    var fileName = $"{baseName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";

                    var fullPath = Path.Combine(profileFolder, fileName);
                    await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await profilepicture.CopyToAsync(fs);
                    }

                    savedRelativePath = $"{fileName}";
                }

                var model = new t_employees
                {
                    c_ename = c_ename.Trim(),
                    c_profileimage = savedRelativePath
                };

                var updatedOk = await _employee.UpdateProfile(id, model);
                if (!updatedOk)
                    return NotFound(new { error = "Employee not found or update failed." });

                var updated = await _employee.GetUser(id);

                return Ok(new
                {
                    data = updated,
                    message = "Profile updated successfully.",
                    imagePath = savedRelativePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProfile error");
                return BadRequest(new { error = "Failed to update profile." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(vm_ChangePassword model)
        {
            // Read the key actually stored at login; fall back to old key if present
            var empId = HttpContext.Session.GetInt32("EmployeeId")
                       ?? HttpContext.Session.GetInt32("EmpId");

            if (!(empId > 0))
                return Unauthorized(new { error = "You must be logged in." });

            if (model == null ||
                string.IsNullOrWhiteSpace(model.OldPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
                return BadRequest(new { error = "All password fields are required." });

            if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
                return BadRequest(new { error = "New and confirm password do not match." });

            try
            {
                var ok = await _employee.ChangePassword(empId.Value, model);
                if (!ok) return BadRequest(new { error = "Old password is incorrect or employee not found." });

                return Json(new { success = true, message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePassword error");
                return BadRequest(new { error = "Failed to change password." });
            }
        }

    }
}
