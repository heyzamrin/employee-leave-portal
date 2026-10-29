
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using EmployeeLeavePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeLeavePortal.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeDashboardController : Controller
    {
        private readonly EmployeeDB db = new EmployeeDB();

        public IActionResult EmployeeDashboardPageLoad()
        {
            // Get the logged-in employee's ID.
            string employeeIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(employeeIdClaim, out int empId))
            {
                return Forbid();
            }

            // Get this employee's leave balances.
            DataTable leaveBalances = db.GetLeaveBalances(empId);

            // Get this employee's leave requests.
            DataTable leaveApplications = db.GetLeaveApplications(empId);

            // Pass database data to the Dashboard view.
            ViewBag.LeaveBalances = leaveBalances;
            ViewBag.LeaveApplications = leaveApplications;

            return View();
        }
        public IActionResult MyLeaveBalance()
        {
            string employeeIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(employeeIdClaim, out int empId))
            {
                return Forbid();
            }

            DataTable leaveBalances = db.GetLeaveBalances(empId);

            ViewBag.LeaveBalances = leaveBalances;

            return View();
        }

        [HttpGet]
        public IActionResult ApplyForLeave()
        {
            LoadLeaveTypes();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyForLeave(
            int LeaveTypeId,
            DateTime FromDate,
            DateTime ToDate,
            string Reason)
        {
            string employeeIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(employeeIdClaim, out int empId))
            {
                return Forbid();
            }

            if (FromDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FromDate",
                    "The start date cannot be in the past.");
            }

            if (ToDate.Date < FromDate.Date)
            {
                ModelState.AddModelError(
                    "ToDate",
                    "The end date must be on or after the start date.");
            }

            if (string.IsNullOrWhiteSpace(Reason))
            {
                ModelState.AddModelError(
                    "Reason",
                    "Please enter a reason for your leave.");
            }

            if (!string.IsNullOrWhiteSpace(Reason) &&
                Reason.Trim().Length > 500)
            {
                ModelState.AddModelError(
                    "Reason",
                    "The reason cannot exceed 500 characters.");
            }

            DataTable leaveTypes = db.GetLeaveTypes();
            bool validLeaveType = false;

            foreach (DataRow row in leaveTypes.Rows)
            {
                if (Convert.ToInt32(row["LeaveTypeId"]) == LeaveTypeId)
                {
                    validLeaveType = true;
                    break;
                }
            }

            if (!validLeaveType)
            {
                ModelState.AddModelError(
                    "LeaveTypeId",
                    "Please select a valid leave type.");
            }

            // Preserve the submitted values if validation fails.
            ViewBag.FromDate = FromDate.ToString("yyyy-MM-dd");
            ViewBag.ToDate = ToDate.ToString("yyyy-MM-dd");
            ViewBag.Reason = Reason;
            ViewBag.SelectedLeaveTypeId = LeaveTypeId;

            if (!ModelState.IsValid)
            {
                LoadLeaveTypes();
                return View();
            }

            try
            {
                db.InsertLeaveApplication(
                    empId,
                    LeaveTypeId,
                    FromDate.Date,
                    ToDate.Date,
                    Reason.Trim());

                TempData["SuccessMessage"] =
                    "Your leave application has been submitted successfully.";

                return RedirectToAction(nameof(ApplyForLeave));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to submit your leave application. Please check the database connection and try again.");

                LoadLeaveTypes();
                return View();
            }
        }


        public IActionResult MyLeaveRequests()
        {
            string employeeIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(employeeIdClaim, out int empId))
            {
                return Forbid();
            }

            DataTable applications = db.GetLeaveApplications(empId);

            ViewBag.LeaveApplications = applications;

            return View();
        }


        public IActionResult MyProfile()
        {
            return View();
        }

        private void LoadLeaveTypes()
        {
            DataTable table = db.GetLeaveTypes();

            List<SelectListItem> items = new List<SelectListItem>();

            foreach (DataRow row in table.Rows)
            {
                items.Add(new SelectListItem
                {
                    Value = Convert.ToString(row["LeaveTypeId"]),
                    Text = Convert.ToString(row["LeaveTypeName"])
                });
            }

            ViewBag.LeaveTypes = items;
        }
    }
}

