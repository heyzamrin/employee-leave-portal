
using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeLeavePortal.Models
{
    public class EmployeeClass
    {
        public int EmpId { get; set; }

        [Required]
        public string EmpName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public string? Department { get; set; }

        public DateTime JoiningDate { get; set; }
    }
}
