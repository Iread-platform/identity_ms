
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using iread_identity_ms.DataAccess;


namespace iread_identity_ms.Web.Util
{

    // this is useful class to enable pass role into jwt
    public class ProfileService : IProfileService
    {
    //services
        private readonly IPublicRepository _userRepository;

        public ProfileService(IPublicRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var roleClaims = context.Subject.FindAll(JwtClaimTypes.Role);
            List<string> list = context.RequestedClaimTypes.ToList();
            context.IssuedClaims.AddRange(roleClaims);
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            // await base.IsActiveAsync(context);
            return Task.CompletedTask;
        }
    }
}