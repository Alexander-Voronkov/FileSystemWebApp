using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppplicationTask.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter your email!")]
        [DisplayName("Enter your email: ")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = default!;
        [Required(ErrorMessage = "Enter your password!")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at a minimum of 8 symbols!")]
        [DisplayName("Enter your password: ")]
        public string Password { get; set; } = default!;
    }
}
