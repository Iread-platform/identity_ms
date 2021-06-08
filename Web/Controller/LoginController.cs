using AutoMapper;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.DataAccess.Repo;
using iread_identity_ms.Web.Dto;
using iread_identity_ms.Web.Service;
using iread_identity_ms.Web.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace iread_identity_ms.Web.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    { 
        private readonly UsersService _usersService;
        private readonly SecurityService _securityService;
        private readonly IMapper _mapper;

        public LoginController(UsersService usersService,
            SecurityService securityService, IMapper mapper)
        {
            _securityService = securityService;
            _usersService = usersService;
            _mapper = mapper;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            if(login == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            IActionResult response = Unauthorized();

            SysUser user = await _usersService.GetByEmail(login.Email);

            if (user == null)
            {
                return NotFound();
            }

            if (!_securityService.VerifyPassword(login.Password, user.StoredSalt, user.Password))
            {
                ModelState.AddModelError("Password", "Password is incorrect");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            
            var jwtToken = _securityService.GenerateJWTToken(user);

            response = Ok(new
            {
                token = jwtToken,
                userDetails = _mapper.Map<UserDto>(user),
            });

            return response;
        }

    }
}