
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EmployeeLeavePortal.Models;
using Microsoft.AspNetCore.Identity;

namespace EmployeeLeavePortal.Controllers
{
    public class EmployeeController : Controller
    {
        EmployeeDB dbobj = new EmployeeDB();

        // Load Employee Registration Page
        public IActionResult EmployeeInsertPageLoad()
        {
            return View();
        }

        // Insert Employee Details
        [HttpPost]
        public IActionResult EmployeeInsertClick(EmployeeClass objcls)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Hash the password before saving it.
                    PasswordHasher<EmployeeClass> passwordHasher =
                        new PasswordHasher<EmployeeClass>();

                    objcls.PasswordHash = passwordHasher.HashPassword(
                        objcls,
                        objcls.PasswordHash
                    );

                    // Save the employee using the existing database method.
                    string resp = dbobj.InsertEmployee(objcls);

                    TempData["msg"] = resp;

                    return RedirectToAction("EmployeeInsertPageLoad");
                }
                else
                {
                    return View("EmployeeInsertPageLoad", objcls);
                }
            }
            catch (Exception)
            {
                TempData["msg"] = "Unable to register employee. Please try again.";
                return View("EmployeeInsertPageLoad", objcls);
            }
        }


        // Display All Employees
        public IActionResult EmployeeList()
        {
            List<EmployeeClass> lst = dbobj.GetAllEmployees();

            return View(lst);
        }
    }
}
