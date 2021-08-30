using AutoMapper;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.Web.Dto.UserDto;

namespace iread_identity_ms.Web.Dto
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto.UserDto>().ReverseMap();
            CreateMap<ApplicationUser, RegisterAsStudentDto>().ForMember(dest =>
            dest.AvatarId,
            opt => opt.MapFrom(src => src.Avatar)).ForMember(dest =>
            dest.CustomPhotoId,
            opt => opt.MapFrom(src => src.CustomPhoto)).ReverseMap();
            CreateMap<ApplicationUser, RegisterAsTeachertDto>().ForMember(dest =>
            dest.AvatarId,
            opt => opt.MapFrom(src => src.Avatar)).ForMember(dest =>
            dest.CustomPhotoId,
            opt => opt.MapFrom(src => src.CustomPhoto)).ReverseMap();
            CreateMap<ApplicationUser, RegisterAsSchoolManager>().ForMember(dest =>
            dest.AvatarId,
            opt => opt.MapFrom(src => src.Avatar)).ForMember(dest =>
            dest.CustomPhotoId,
            opt => opt.MapFrom(src => src.CustomPhoto)).ReverseMap();

        }
    }
}