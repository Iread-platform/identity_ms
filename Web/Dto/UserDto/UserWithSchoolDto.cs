using iread_identity_ms.Web.Dto.SchoolMemberDto;

namespace iread_identity_ms.Web.Dto.UserDto
{
    public class UserWithSchoolDto:UserDto
    {
        public InnerSchoolMemberDto SchoolMember { get; set; }
    }
}