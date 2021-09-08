using System.ComponentModel.DataAnnotations;

namespace iread_identity_ms.Web.Dto.UserDto
{
    public class ResetPasswordDto
    {
        [Required(AllowEmptyStrings = true)]
        public string NewPassword { get; set; }
    }
}