using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using iread_identity_ms.DataAccess;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.Web.Util{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
    private readonly IPublicRepository _userRepository;

    public ResourceOwnerPasswordValidator(IPublicRepository userRepository)
    {
        _userRepository = userRepository; //DI
    }

    //this is used to validate your user account with provided grant at /connect/token
    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        try
        {
            //get your user model from db (by username - in my case its email)
            var user = await _userRepository.GetAppUsersRepository.GetByName(context.UserName);
            if (user != null)
            {
                //check if password match - remember to hash password if stored as hash in db
                if (user.Password == context.Password) {
                    //set the result
                    context.Result = new GrantValidationResult(
                        subject: user.Id.ToString(),
                        authenticationMethod: "custom", 
                        claims: GetUserClaims(user));

                    return;
                } 

                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Incorrect password");
                return;
            }
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User does not exist.");
            return;
        }
        catch (Exception)
            {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid username or password");
        }
    }

    //build claims array from user data
    public static Claim[] GetUserClaims(ApplicationUser user)
    {
        return new Claim[]
        {
            new Claim("user_id", user.Id.ToString() ?? ""),
            new Claim(JwtClaimTypes.GivenName, user.Name  ?? ""),
            new Claim(JwtClaimTypes.Email, user.Email  ?? ""),
            //roles
            new Claim(JwtClaimTypes.Role, user.Role)
        };
    }   
    }
}