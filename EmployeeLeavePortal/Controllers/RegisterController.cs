
using System;
using EmployeeLeavePortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeavePortal.Controllers
{
    public class RegisterController : Controller
    {
        private readonly EmployeeDB dbobj = new EmployeeDB();

        private readonly PasswordHasher<LoginViewModel> passwordHasher =
            new PasswordHasher<LoginViewModel>();

        // Display registration page
        [HttpGet]
        public IActionResult RegisterPageLoad()
        {
            return View(new RegisterViewModel());
        }

        // Process registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterClick(RegisterViewModel obj)
        {
            // Validate account type
            if (obj.AccountType != "Employee" &&
                obj.AccountType != "AdminRequest")
            {
                ModelState.AddModelError(
                    "AccountType",
                    "Please select a valid account type.");
            }

            // Department is not required for Admin requests
            if (obj.AccountType == "AdminRequest")
            {
                ModelState.Remove(nameof(obj.Department));
            }

            // Stop if any required validation fails
            if (!ModelState.IsValid)
            {
                return View("RegisterPageLoad", obj);
            }

            try
            {
                string email = obj.Email.Trim();

                // Check whether the email already exists
                if (dbobj.EmailExists(email))
                {
                    ModelState.AddModelError(
                        "Email",
                        "An account or pending request already uses this email.");

                    return View("RegisterPageLoad", obj);
                }

                // Hash the password
                LoginViewModel loginModel = new LoginViewModel
                {
                    Email = email,
                    Password = obj.Password,
                    Role = "Employee"
                };

                string passwordHash =
                    passwordHasher.HashPassword(
                        loginModel,
                        obj.Password);

                if (obj.AccountType == "Employee")
                {
                    // Create employee account
                    EmployeeClass employee = new EmployeeClass
                    {
                        EmpName = obj.Name.Trim(),
                        Email = email,
                        PasswordHash = passwordHash,
                        Department = string.IsNullOrWhiteSpace(obj.Department)
                            ? null
                            : obj.Department.Trim(),
                        JoiningDate = DateTime.Today
                    };

                    dbobj.InsertEmployee(employee);

                    TempData["RegisterMessage"] =
                        "Employee registration successful. You can now log in.";
                }
                else
                {
                    // Save Admin request for approval
                    dbobj.InsertAdminRegistrationRequest(
                        obj.Name.Trim(),
                        email,
                        passwordHash);

                    TempData["RegisterMessage"] =
                        "Your Admin registration request has been submitted. " +
                        "You can log in after your request is verified and approved.";
                }

                return RedirectToAction("LoginPageLoad", "Login");
            }
            catch (Exception ex)
            {
                // Temporary debugging for local development
                ViewBag.Error = ex.Message;

                return View("RegisterPageLoad", obj);
            }
        }
    }
}
