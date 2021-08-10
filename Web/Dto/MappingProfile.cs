using AutoMapper;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.Web.Dto
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<ApplicationUser, RegisterStudentDto>().ForMember(dest =>
            dest.AvatarId,
            opt => opt.MapFrom(src => src.Avatar)).ReverseMap();

        }
    }
}