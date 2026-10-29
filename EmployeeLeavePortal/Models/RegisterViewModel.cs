
using System.ComponentModel.DataAnnotations;

namespace EmployeeLeavePortal.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter your password.")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password must contain at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "Password and confirm password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        public string Department { get; set; } = "";

        [Required(ErrorMessage = "Please select an account type.")]
        public string AccountType { get; set; } = "Employee";
    }
}
