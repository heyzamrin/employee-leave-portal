
using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeLeavePortal.Models;

namespace EmployeeLeavePortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminApprovalController : Controller
    {
        private readonly EmployeeDB dbobj = new EmployeeDB();

        // =========================================
        // ADMIN DASHBOARD
        // =========================================
        [HttpGet]
        public IActionResult AdminDashboardPageLoad()
        {
            try
            {
                // Total employees
                var employees = dbobj.GetAllEmployees();

                ViewBag.TotalEmployees = employees.Count;

                // All employee leave applications
                DataTable leaves =
                    dbobj.GetAllLeaveApplicationsForAdmin();

                // Pending leave applications
                ViewBag.PendingLeaves = leaves.AsEnumerable()
                    .Count(row =>
                        string.Equals(
                            Convert.ToString(row["Status"]),
                            "Pending",
                            StringComparison.OrdinalIgnoreCase));

                // Approved leave applications
                ViewBag.ApprovedLeaves = leaves.AsEnumerable()
                    .Count(row =>
                        string.Equals(
                            Convert.ToString(row["Status"]),
                            "Approved",
                            StringComparison.OrdinalIgnoreCase));

                // Pending admin registration requests
                DataTable requests =
                    dbobj.GetPendingAdminRequests();

                ViewBag.PendingAdminRequests = requests.Rows.Count;
                ViewBag.PendingAdminRequestsTable = requests;

                ViewBag.DashboardError = null;
            }
            catch (Exception ex)
            {
                // Set default values if an error occurs
                ViewBag.TotalEmployees = 0;
                ViewBag.PendingLeaves = 0;
                ViewBag.ApprovedLeaves = 0;
                ViewBag.PendingAdminRequests = 0;

                ViewBag.PendingAdminRequestsTable =
                    new DataTable();

                // Temporary diagnostic message
                ViewBag.DashboardError =
                    "Dashboard loading error: " + ex.Message;
            }

            return View(
                "~/Views/AdminDashboard/AdminDashboardPageLoad.cshtml");
        }

        // =========================================
        // EMPLOYEE LEAVE APPROVALS PAGE
        // =========================================
        [HttpGet]
        public IActionResult AdminLeaveApprovalPageLoad()
        {
            try
            {
                DataTable applications =
                    dbobj.GetAllLeaveApplicationsForAdmin();

                ViewBag.LeaveApplications = applications;

                ViewBag.DebugMessage =
                    "Rows returned from database: "
                    + applications.Rows.Count;

                ViewBag.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ViewBag.LeaveApplications = new DataTable();

                ViewBag.DebugMessage =
                    "Database error: " + ex.Message;

                ViewBag.ErrorMessage =
                    "Unable to load leave applications.";
            }

            return View(
                "~/Views/AdminApproval/AdminLeaveApprovalPageLoad.cshtml");
        }

        // =========================================
        // APPROVE EMPLOYEE LEAVE
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveLeave(int leaveId)
        {
            try
            {
                bool updated =
                    dbobj.ApproveLeaveRequest(leaveId);

                TempData["msg"] = updated
                    ? "Leave request approved successfully."
                    : "This request was not pending or could not be found.";
            }
            catch (Exception)
            {
                TempData["msg"] =
                    "Unable to approve the leave request.";
            }

            return RedirectToAction(
                nameof(AdminLeaveApprovalPageLoad));
        }

        // =========================================
        // REJECT EMPLOYEE LEAVE
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectLeave(int leaveId)
        {
            try
            {
                bool updated =
                    dbobj.RejectLeaveRequest(leaveId);

                TempData["msg"] = updated
                    ? "Leave request rejected successfully."
                    : "This request was not pending or could not be found.";
            }
            catch (Exception)
            {
                TempData["msg"] =
                    "Unable to reject the leave request.";
            }

            return RedirectToAction(
                nameof(AdminLeaveApprovalPageLoad));
        }

        // =========================================
        // PENDING ADMIN REGISTRATION REQUESTS
        // =========================================
        [HttpGet]
        public IActionResult AdminApprovalPageLoad()
        {
            try
            {
                ViewBag.PendingAdminRequestsTable =
                    dbobj.GetPendingAdminRequests();

                ViewBag.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ViewBag.PendingAdminRequestsTable =
                    new DataTable();

                ViewBag.ErrorMessage =
                    "Unable to load admin registration requests: "
                    + ex.Message;
            }

            return View();
        }

        // =========================================
        // APPROVE ADMIN REGISTRATION
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveRequest(int requestId)
        {
            try
            {
                dbobj.ApproveAdminRequest(requestId);

                TempData["msg"] =
                    "Admin registration request approved successfully.";
            }
            catch (Exception)
            {
                TempData["msg"] =
                    "Unable to approve the admin registration request.";
            }

            return RedirectToAction(
                nameof(AdminApprovalPageLoad));
        }

        // =========================================
        // REJECT ADMIN REGISTRATION
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectRequest(int requestId)
        {
            try
            {
                dbobj.RejectAdminRequest(requestId);

                TempData["msg"] =
                    "Admin registration request rejected.";
            }
            catch (Exception)
            {
                TempData["msg"] =
                    "Unable to reject the admin registration request.";
            }

            return RedirectToAction(
                nameof(AdminApprovalPageLoad));
        }


        // Show only pending leave requests
        [HttpGet]
        public IActionResult PendingLeaveRequestsPageLoad()
        {
            try
            {
                DataTable allLeaves =
                    dbobj.GetAllLeaveApplicationsForAdmin();

                DataView view = new DataView(allLeaves);
                view.RowFilter = "Status = 'Pending'";

                ViewBag.LeaveApplications = view.ToTable();
                ViewBag.PageTitle = "Pending Leave Requests";
            }
            catch (Exception ex)
            {
                ViewBag.LeaveApplications = new DataTable();
                ViewBag.ErrorMessage =
                    "Unable to load pending leave requests: " + ex.Message;
                ViewBag.PageTitle = "Pending Leave Requests";
            }

            return View(
                "~/Views/AdminApproval/LeaveRequestsPageLoad.cshtml");
        }

        // Show only approved leave requests
        [HttpGet]
        public IActionResult ApprovedLeaveRequestsPageLoad()
        {
            try
            {
                DataTable allLeaves =
                    dbobj.GetAllLeaveApplicationsForAdmin();

                DataView view = new DataView(allLeaves);
                view.RowFilter = "Status = 'Approved'";

                ViewBag.LeaveApplications = view.ToTable();
                ViewBag.PageTitle = "Approved Leave Requests";
            }
            catch (Exception ex)
            {
                ViewBag.LeaveApplications = new DataTable();
                ViewBag.ErrorMessage =
                    "Unable to load approved leave requests: " + ex.Message;
                ViewBag.PageTitle = "Approved Leave Requests";
            }

            return View(
                "~/Views/AdminApproval/LeaveRequestsPageLoad.cshtml");
        }

    }
}
