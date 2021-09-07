using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iread_identity_ms.DataAccess.Data.Type;
using Microsoft.AspNetCore.Identity;

namespace iread_identity_ms.DataAccess.Data.Entity
{
    public class ApplicationUser : IdentityUser
    {

        
        [Required]
        [EnumDataType(typeof(RoleTypes), ErrorMessage = "Role type value doesn't exist within enum should be one of [Administrator, Teacher, Student]")]
        public string Role { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        public Nullable<int> Avatar { get; set; }
        public Nullable<int> CustomPhoto { get; set; }


        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }
        
        public int? Level { get; set; }
        public DateTime? BirthDay { get; set; }
    }
}