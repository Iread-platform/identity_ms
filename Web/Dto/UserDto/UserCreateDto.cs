using iread_identity_ms.DataAccess.Data.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iread_identity_ms.Web.Dto.UserDto
{

    public class UserCreateDto
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string Password { get; set; }
        [Required]
        [EnumDataType(typeof(RoleTypes), ErrorMessage = "Role type value doesn't exist within enum should be one of [Administrator, Teacher, Student]")]
        public string Role { get; set; }
        [Required(AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

    }

}
