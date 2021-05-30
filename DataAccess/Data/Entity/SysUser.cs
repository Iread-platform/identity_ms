using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iread_identity_ms.DataAccess.Data.Type;

namespace iread_identity_ms.DataAccess.Data.Entity
{
    [Table("Users")]
    public class SysUser
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int UserId { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Password { get; set; }
  
        [Required]
        [Column(TypeName = "varbinary(128)")]
        public byte[] StoredSalt { get; set; }
        
        [Required]
        [EnumDataType(typeof(RoleTypes), ErrorMessage = "Role type value doesn't exist within enum should be one of [Administrator, Teacher, Student]")]
        public string Role { get; set; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }


    }
}
