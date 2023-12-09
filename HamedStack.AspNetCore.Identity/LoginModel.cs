using System.ComponentModel.DataAnnotations;

namespace HamedStack.AspNetCore.Identity
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; } = string.Empty;
    }
}
