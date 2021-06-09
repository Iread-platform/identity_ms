using AutoMapper;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.Web.Dto{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SysUser, UserDto>();
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<UserDto, SysUser>();
            CreateMap<UserDto, ApplicationUser>();
            CreateMap<UserCreateDto, SysUser>();
            
        }
    }
}