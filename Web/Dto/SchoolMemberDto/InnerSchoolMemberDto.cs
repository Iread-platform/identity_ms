using System.Collections.Generic;
using iread_identity_ms.Web.Dto.ClassDto;

namespace iread_identity_ms.Web.Dto.SchoolMemberDto
{
    public class InnerSchoolMemberDto
    {
        public int SchoolId { get; set; }
        public string SchoolTitle { get; set; }
        public IEnumerable<InnerClassDto> Classes { get; set; }
    }
}