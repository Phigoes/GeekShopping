using System.ComponentModel.DataAnnotations;

namespace GeekShopping.IdentityServer.Pages.Account.Registration
{
    public class InputModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public string RoleName { get; set; }
        public string Button { get; set; }
    }
}
