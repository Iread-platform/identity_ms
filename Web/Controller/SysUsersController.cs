using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using iread_identity_ms.DataAccess.Repo;
using iread_identity_ms.Web.Service;
using iread_identity_ms.Web.Dto;
using iread_identity_ms;
using iread_identity_ms.DataAccess.Data.Entity;
using AutoMapper;
using iread_identity_ms.Web.Util;

namespace M3allem.M3allem.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SysUsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly AppUsersService _appUsersService;
        private readonly SecurityService _securityService;
        private readonly IMapper _mapper;

        public SysUsersController(AppUsersService appUsersService, UsersService usersService, SecurityService securityService,
             IMapper mapper)
        {
            _usersService = usersService;
            _securityService = securityService;
            _appUsersService = appUsersService;
            _mapper = mapper;
        }

        // GET: api/SysUsers
        [HttpGet]
        [Authorize(Roles = Policies.Administrator)]
        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            return _mapper.Map<IEnumerable<UserDto>>(await _usersService.GetAll());
        }
        
        
        // GET: api/SysUsers/ByEmail
        [HttpGet("ByEmail")]
        public async Task<IActionResult> GetUserByEmail([FromQuery(Name = "email")] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            if (String.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "Email is required");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            if (!(new EmailAddressAttribute().IsValid(email)))
            {
                ModelState.AddModelError("Email", "Email is not valid");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            var user = await _usersService.GetByEmail(email);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<UserDto>(user));
        }


        // GET: api/SysUsers/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            var user = await _usersService.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<UserDto>(user));
        }

        
        // POST: api/SysUsers
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserCreateDto user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            var userEntity = _mapper.Map<SysUser>(user);
            await UserFieldValidationAsync(userEntity);

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            IActionResult response = BadRequest();
            if (!_usersService.Insert(userEntity))
            {
                return BadRequest();
            }

            if (user != null)
            {
                var tokenString = _securityService.GenerateJWTToken(userEntity);

                response = Ok(new
                {
                    token = tokenString,
                    userDetails = _mapper.Map<UserDto>(userEntity),
                });
            }

            return response;
        }



        // POST: api/SysUsers
        [HttpPost("App")]
        public async Task<IActionResult> PostAppUser([FromBody] ApplicationUser userEntity)
        {
            if (userEntity == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            //var userEntity = _mapper.Map<ApplicationUser>(user);
            await AppUserFieldValidationAsync(userEntity);

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            IActionResult response = BadRequest();
            if (!_appUsersService.Insert(userEntity))
            {
                return BadRequest();
            }

            if (userEntity != null)
            {
                response = Ok(new
                {
                    userDetails = _mapper.Map<UserDto>(userEntity),
                });
            }

            return response;
        }




        

        // DELETE: api/SysUsers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Policies.Administrator)]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            var user = await _usersService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!_usersService.Delete(user))
            {
                return BadRequest();
            }

            return NoContent();
        }


        private async Task UserFieldValidationAsync(SysUser user)
        {
 
            var similarUser = await _usersService.GetByEmail(user.Email);
            if (similarUser != null)
            {
                ModelState.AddModelError("email", "Email already exist");
            }
        }

         private async Task AppUserFieldValidationAsync(ApplicationUser user)
        {
 
            var similarUser = await _appUsersService.GetByEmail(user.Email);
            if (similarUser != null)
            {
                ModelState.AddModelError("email", "Email already exist");
            }
        }
    }
}