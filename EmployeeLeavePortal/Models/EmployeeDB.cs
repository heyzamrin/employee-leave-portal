
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace EmployeeLeavePortal.Models
{
    public class EmployeeDB
    {
        private readonly string connectionString =
            "Data Source=DESKTOP-GNDPCG1\\SQLEXPRESS;Initial Catalog=EmployeeLeavePortalDB;Integrated Security=True;TrustServerCertificate=True";

        // Register a new employee
        public string InsertEmployee(EmployeeClass obj)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_InsertEmployee", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@EmpName", SqlDbType.VarChar, 100).Value = obj.EmpName;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = obj.Email;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = obj.PasswordHash;
                cmd.Parameters.Add("@Department", SqlDbType.VarChar, 100).Value = (object?)obj.Department ?? DBNull.Value;
                cmd.Parameters.Add("@JoiningDate", SqlDbType.Date).Value = obj.JoiningDate;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return "Employee registered successfully";
        }

        // Check whether an email already exists
        public bool EmailExists(string email)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT CASE WHEN EXISTS
                (
                    SELECT 1 FROM EmployeeTable WHERE Email = @Email
                    UNION ALL
                    SELECT 1 FROM AdminTable WHERE Email = @Email
                    UNION ALL
                    SELECT 1
                    FROM AdminRegistrationRequestTable
                    WHERE Email = @Email AND Status = 'Pending'
                )
                THEN 1 ELSE 0 END", con))
            {
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email.Trim();

                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        // Save an Admin registration request
        public void InsertAdminRegistrationRequest(string name, string email, string passwordHash)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO AdminRegistrationRequestTable
                    (AdminName, Email, PasswordHash, Status)
                VALUES
                    (@AdminName, @Email, @PasswordHash, 'Pending')", con))
            {
                cmd.Parameters.Add("@AdminName", SqlDbType.VarChar, 100).Value = name.Trim();
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email.Trim();
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Get all employees
        public List<EmployeeClass> GetAllEmployees()
        {
            List<EmployeeClass> lst = new List<EmployeeClass>();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetAllEmployees", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        EmployeeClass obj = new EmployeeClass();

                        obj.EmpId = Convert.ToInt32(dr["EmpId"]);
                        obj.EmpName = dr["EmpName"].ToString() ?? "";
                        obj.Email = dr["Email"].ToString() ?? "";
                        obj.Department = dr["Department"] == DBNull.Value
                            ? null
                            : dr["Department"].ToString();
                        obj.JoiningDate = Convert.ToDateTime(dr["JoiningDate"]);

                        lst.Add(obj);
                    }
                }
            }

            return lst;
        }

        // Get login user by role and email
        public DataTable GetLoginUser(string role, string email)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetLoginUser", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Role", SqlDbType.VarChar, 20).Value = role;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // Update login password
        public void UpdateLoginPassword(string role, int userId, string newPasswordHash)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_UpdateLoginPassword", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Role", SqlDbType.VarChar, 20).Value = role;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = newPasswordHash;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Get all leave types
        public DataTable GetLeaveTypes()
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetLeaveTypes", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // Get leave balances for one employee
        public DataTable GetLeaveBalances(int empId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetLeaveBalances", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpId", SqlDbType.Int).Value = empId;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // Get leave applications for one employee
        public DataTable GetLeaveApplications(int empId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetLeaveApplications", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpId", SqlDbType.Int).Value = empId;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // Insert a leave application
        public void InsertLeaveApplication(int empId, int leaveTypeId, DateTime fromDate, DateTime toDate, string reason)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_InsertLeaveApplication", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpId", SqlDbType.Int).Value = empId;
                cmd.Parameters.Add("@LeaveTypeId", SqlDbType.Int).Value = leaveTypeId;
                cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;
                cmd.Parameters.Add("@Reason", SqlDbType.VarChar, 1000).Value = reason;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Create an Admin account
        public void InsertAdmin(string name, string email, string passwordHash)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO AdminTable (AdminName, Email, PasswordHash)
                VALUES (@AdminName, @Email, @PasswordHash)", con))
            {
                cmd.Parameters.Add("@AdminName", SqlDbType.VarChar, 100).Value = name.Trim();
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email.Trim();
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Get pending Admin registration requests
        public DataTable GetPendingAdminRequests()
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT RequestId, AdminName, Email, RequestedAt, Status
                FROM AdminRegistrationRequestTable
                WHERE Status = 'Pending'
                ORDER BY RequestedAt ASC", con))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // Approve an Admin registration request
        public void ApproveAdminRequest(int requestId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        string name;
                        string email;
                        string passwordHash;

                        using (SqlCommand cmd = new SqlCommand(@"
                            SELECT AdminName, Email, PasswordHash
                            FROM AdminRegistrationRequestTable
                            WHERE RequestId = @RequestId AND Status = 'Pending'", con, transaction))
                        {
                            cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = requestId;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new InvalidOperationException("This request is no longer pending.");
                                }

                                name = reader["AdminName"].ToString() ?? "";
                                email = reader["Email"].ToString() ?? "";
                                passwordHash = reader["PasswordHash"].ToString() ?? "";
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO AdminTable (AdminName, Email, PasswordHash)
                            VALUES (@AdminName, @Email, @PasswordHash)", con, transaction))
                        {
                            cmd.Parameters.Add("@AdminName", SqlDbType.VarChar, 100).Value = name;
                            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email;
                            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;

                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE AdminRegistrationRequestTable
                            SET Status = 'Approved'
                            WHERE RequestId = @RequestId AND Status = 'Pending'", con, transaction))
                        {
                            cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = requestId;

                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                throw new InvalidOperationException("The request could not be approved.");
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Reject an Admin registration request
        public void RejectAdminRequest(int requestId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE AdminRegistrationRequestTable
                SET Status = 'Rejected'
                WHERE RequestId = @RequestId AND Status = 'Pending'", con))
            {
                cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = requestId;

                con.Open();

                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("This request is no longer pending.");
                }
            }
        }

        // Get all employee leave applications for Admin
        public DataTable GetAllLeaveApplicationsForAdmin()
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT
                    L.LeaveId,
                    E.EmpName,
                    E.Email,
                    T.LeaveTypeName,
                    L.FromDate,
                    L.ToDate,
                    L.Reason,
                    L.AppliedDate,
                    L.Status
                FROM LeaveApplicationTable AS L
                INNER JOIN EmployeeTable AS E
                    ON L.EmpId = E.EmpId
                INNER JOIN LeaveTypeTable AS T
                    ON L.LeaveTypeId = T.LeaveTypeId
                ORDER BY
                    CASE WHEN L.Status = 'Pending' THEN 0 ELSE 1 END,
                    L.AppliedDate DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }

            return dt;
        }

        // Approve a pending employee leave request
        public bool ApproveLeaveRequest(int leaveId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE LeaveApplicationTable
                SET Status = 'Approved'
                WHERE LeaveId = @LeaveId AND Status = 'Pending'", con))
            {
                cmd.Parameters.Add("@LeaveId", SqlDbType.Int).Value = leaveId;

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Reject a pending employee leave request
        public bool RejectLeaveRequest(int leaveId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE LeaveApplicationTable
                SET Status = 'Rejected'
                WHERE LeaveId = @LeaveId AND Status = 'Pending'", con))
            {
                cmd.Parameters.Add("@LeaveId", SqlDbType.Int).Value = leaveId;

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
