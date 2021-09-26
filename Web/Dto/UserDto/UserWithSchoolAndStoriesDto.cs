using System.Collections.Generic;
using iread_identity_ms.Web.Dto.SchoolMemberDto;
using iread_identity_ms.Web.Dto.StoryDto;

namespace iread_identity_ms.Web.Dto.UserDto
{
    public class UserWithSchoolAndStoriesDto:UserDto
    {
        public InnerSchoolMemberDto SchoolMember { get; set; }
        
        public List<ViewStoryDto> ViewStories { get; set; }
        
    }
}