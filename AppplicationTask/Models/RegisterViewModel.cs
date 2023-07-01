using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppplicationTask.Models
{
    public class RegisterViewModel
    {
        [DisplayName("Enter your email: ")]
        [Required(ErrorMessage = "Enter your email!")]
        [DataType(DataType.EmailAddress, ErrorMessage = "It's not an email!")]
        public string Email { get; set; } = default!;
        [Required(ErrorMessage = "Enter your password!")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at a minimum of 8 symbols!")]
        [DisplayName("Enter your password: ")]
        public string Password { get; set; } = default!;
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords must be the same!")]
        [DisplayName("Confirm your password: ")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
