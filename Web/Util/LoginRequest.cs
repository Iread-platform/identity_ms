using System.ComponentModel.DataAnnotations;

namespace iread_identity_ms.Web.Util
{
    public class LoginRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
    }
}