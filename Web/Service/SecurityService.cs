using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using iread_identity_ms;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.Web.Util;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace iread_identity_ms.Web.Service
{
    public class SecurityService
    {
        public string GenerateJWTToken(SysUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.Configuration.GetSection("Jwt")["SecretKey"].ToString()));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(Double.Parse(Startup.Configuration.GetSection("Jwt")["ExpiredAfterInHour"].ToString())),
            signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public HashSalt EncryptPassword(string password)
        {
            byte[] salt = new byte[128 / 8]; // Generate a 128-bit salt using a secure PRNG
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string encryptedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));
            return new HashSalt { Hash = encryptedPassword, Salt = salt };
        }

        public bool VerifyPassword(string enteredPassword, byte[] salt, string storedPassword)
        {
            string encryptedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));
            return encryptedPassword == storedPassword;
        }
    }
}