using System;
using System.ComponentModel.DataAnnotations;

namespace iread_identity_ms.Web.Dto.UserDto
{
    public class UpdateStudentDto
    {
        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        public Nullable<int> AvatarId { get; set; }
        
        public Nullable<int> CustomPhotoId { get; set; }
        
        public DateTime? BirthDay { get; set; }
        
        public Nullable<int> Level { get; set; }
    }
}