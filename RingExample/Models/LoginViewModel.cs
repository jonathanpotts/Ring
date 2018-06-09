using System.ComponentModel.DataAnnotations;

namespace RingExample.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }

        [Display(Name = "Stay logged in?")]
        public bool StayLoggedIn { get; set; }
    }
}
