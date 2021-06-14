using AutoMapper;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.Web.Dto{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<UserDto, ApplicationUser>();
            
        }
    }
}