
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeLeavePortal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeavePortal.Controllers
{
    public class LoginController : Controller
    {
        private readonly EmployeeDB dbobj = new EmployeeDB();

        private readonly PasswordHasher<LoginViewModel> passwordHasher =
            new PasswordHasher<LoginViewModel>();

        [HttpGet]
        public IActionResult LoginPageLoad()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginClick(LoginViewModel obj)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginPageLoad", obj);
            }

            try
            {
                DataTable userTable = dbobj.GetLoginUser(
                    obj.Role,
                    obj.Email.Trim()
                );

                if (userTable == null || userTable.Rows.Count == 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email, password, or account type."
                    );

                    return View("LoginPageLoad", obj);
                }

                DataRow user = userTable.Rows[0];

                string passwordHash =
                    Convert.ToString(user["PasswordHash"]);

                if (string.IsNullOrWhiteSpace(passwordHash))
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email, password, or account type."
                    );

                    return View("LoginPageLoad", obj);
                }

                PasswordVerificationResult passwordResult =
                    passwordHasher.VerifyHashedPassword(
                        obj,
                        passwordHash,
                        obj.Password
                    );

                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email, password, or account type."
                    );

                    return View("LoginPageLoad", obj);
                }

                string email = Convert.ToString(user["Email"]);
                string role = obj.Role;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                };

                // Add the employee ID to the claims.
                // EmployeeDashboardController uses this ID.
                if (role == "Employee")
                {
                    if (!userTable.Columns.Contains("EmpId") ||
                        user["EmpId"] == DBNull.Value)
                    {
                        ModelState.AddModelError(
                            "",
                            "Employee ID was not returned by the login query."
                        );

                        return View("LoginPageLoad", obj);
                    }

                    string employeeId =
                        Convert.ToString(user["EmpId"]);

                    claims.Add(
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            employeeId
                        )
                    );
                }

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal
                );

                if (role == "Admin")
                {
                    return RedirectToAction(
                        "AdminDashboardPageLoad",
                        "AdminApproval"
                    );
                }

                if (role == "Employee")
                {
                    return RedirectToAction(
                        "EmployeeDashboardPageLoad",
                        "EmployeeDashboard"
                    );
                }

                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                ModelState.AddModelError(
                    "",
                    "Please select a valid account type."
                );

                return View("LoginPageLoad", obj);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to log in right now. Please try again."
                );

                return View("LoginPageLoad", obj);
            }
        }

        [HttpGet]
        public IActionResult AccessDeniedPageLoad(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "LoginPageLoad",
                "Login"
            );
        }
    }
}
