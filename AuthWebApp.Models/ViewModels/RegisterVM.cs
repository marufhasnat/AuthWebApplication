using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWebApp.Models.ViewModels
{
    public class RegisterVM
    {
        [EmailAddress]
        [Required(ErrorMessage = "Please enter a  valid email")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Please enter confirm password")]
        [Compare("Password", ErrorMessage = "Password must match with confirm password.")]
        public string ConfirmPassword { get; set; } = default!;

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Please enter first name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Please enter last name")]
        public string? LastName { get; set; }


        [Required(ErrorMessage = "Please enter a username")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Username must be at least 5 characters long")]
        public string? Username { get; set; }


        [Display(Name = "Terms and Conditions")]
        [MustBeTrue(ErrorMessage = "You must agree to the terms and conditions")]
        public bool Status { get; set; }
    }

    // Custom Validation Attribute
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool boolValue && boolValue;
        }
    }
}
