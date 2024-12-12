using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWebApp.Models.ViewModels
{
    public class ForgetPasswordVM
    {
        [EmailAddress]
        [Required(ErrorMessage = "Please enter a  valid email")]
        public string Email { get; set; } = default!;

        public string UserId { get; set; } = default!;

        public string Token { get; set; } = default!;

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Please enter confirm password")]
        [Compare("Password", ErrorMessage = "Password must match with confirm password.")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
