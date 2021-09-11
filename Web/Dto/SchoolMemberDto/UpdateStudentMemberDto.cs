using System.ComponentModel.DataAnnotations;

namespace iread_identity_ms.Web.Dto.SchoolMemberDto
{
    public class UpdateStudentMemberDto
    {
        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }
    }
}