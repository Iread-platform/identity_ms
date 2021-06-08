using AutoMapper;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.Web.Dto{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SysUser, UserDto>();
            CreateMap<UserDto, SysUser>();
            CreateMap<UserCreateDto, SysUser>();
            
        }
    }
}