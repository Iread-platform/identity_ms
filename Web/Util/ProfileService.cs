
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using iread_identity_ms.DataAccess;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.Web.Dto.SchoolMemberDto;
using iread_identity_ms.Web.Dto.UserDto;
using iread_identity_ms.Web.Service;


namespace iread_identity_ms.Web.Util
{

    // this is useful class to enable pass role into jwt
    public class ProfileService : IProfileService
    {
        //services
        private readonly IPublicRepository _userRepository;
        private readonly IConsulHttpClientService _consulHttpClient;

        public ProfileService(IPublicRepository userRepository, IConsulHttpClientService consulHttpClient)
        {
            _userRepository = userRepository;
            _consulHttpClient = consulHttpClient;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            string memberId = context.Subject.Claims.Where(c => c.Type == "sub").Select(c => c.Value).SingleOrDefault();
            InnerSchoolMemberDto schoolMember = _consulHttpClient.GetAsync<InnerSchoolMemberDto>("school_ms", $"/api/School/getByMemberId/{memberId}").GetAwaiter().GetResult();
            ApplicationUser user = _userRepository.GetAppUsersRepository.GetById(memberId).GetAwaiter().GetResult();
            
            IEnumerable<Claim> roleClaims = context.Subject.FindAll(JwtClaimTypes.Role);
            List<Claim> claims = new List<Claim>(roleClaims);
           
            if (schoolMember == null)
            {
                claims.Add(new Claim("SchoolId", "-1"));
            }
            else
            {
                claims.Add(new Claim("SchoolId", schoolMember.SchoolId.ToString()));
                claims.Add(new Claim("SchoolTitle", schoolMember.SchoolTitle));
                string classIds = "";
                foreach (var c in schoolMember.Classes)
                {
                    classIds += c.ClassId + ",";
                }
                classIds = classIds.Remove(classIds.Length - 1);
                claims.Add(new Claim("ClassIds", classIds));
            }
           
            claims.Add(new Claim("NameIdentifier", memberId));
            claims.Add(new Claim("FirstName", user.FirstName));
            claims.Add(new Claim("LastName", user.LastName));



            List<string> list = context.RequestedClaimTypes.ToList();

            context.IssuedClaims.AddRange(claims);
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            // await base.IsActiveAsync(context);
            return Task.CompletedTask;
        }
    }
}