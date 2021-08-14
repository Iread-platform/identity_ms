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
using iread_identity_ms.Web.Dto.UserDto;
using iread_identity_ms;
using iread_identity_ms.DataAccess.Data.Entity;
using AutoMapper;
using iread_identity_ms.Web.Util;
using Microsoft.AspNetCore.Identity;
using iread_identity_ms.DataAccess;
using iread_identity_ms.DataAccess.Data.Type;
using iread_interaction_ms.Web.DTO.AttachmentDTO;

namespace M3allem.M3allem.Controller
{
    [Route("api/[controller]/")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly AppUsersService _usersService;
        private readonly IMapper _mapper;

        private readonly IConsulHttpClientService _consulHttpClient;
        private readonly UserManager<ApplicationUser> _userManager;


        public IdentityController(
            IPublicRepository repository,
            AppUsersService usersService,
             IMapper mapper,
             UserManager<ApplicationUser> userManager,
             IConsulHttpClientService consulHttpClient)
        {
            _usersService = usersService;
            _mapper = mapper;
            _userManager = userManager;
            _consulHttpClient = consulHttpClient;
        }

        [HttpGet("all")]
        [Authorize(Roles = Policies.Administrator)]
        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            return _mapper.Map<IEnumerable<UserDto>>(await _usersService.GetAll());
        }


        [HttpGet("get-by-email")]
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


        [HttpGet("{id}/get")]
        public async Task<IActionResult> GetById([FromRoute] string id)
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


        /*// POST: api/SysUsers/add
        [HttpPost("add")]
        public async Task<IActionResult> PostUser([FromBody] ApplicationUser user)
        {

            if (user == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            await UserFieldValidationAsync(user);

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            IActionResult response = BadRequest();
            if (!_usersService.Insert(user))
            {
                return BadRequest();
            }

            if (user != null)
            {
                // request token
                //     var tokenClient = new TokenClient("http://localhost:5000/connect/token", "ro.client", "secret");
                //     var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1");

                //     if (tokenResponse.IsError)
                //     {
                //         Console.WriteLine(tokenResponse.Error);
                //         return;
                //     }

                //     Console.WriteLine(tokenResponse.Json);
                //     Console.WriteLine("\n\n");

                response = Ok(user);
            }

            return response;
        }*/

        [HttpPost("RegisterAsStudent")]
        public IActionResult RegisterStudent([FromBody] RegisterAsStudentDto student)
        {

            if (student == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            ApplicationUser studentEntity = _mapper.Map<ApplicationUser>(student);
            RegisterValidation(studentEntity);

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }


            _usersService.CreateStudent(studentEntity);


            return CreatedAtAction("GetById", new { id = studentEntity.Id }, _mapper.Map<UserDto>(studentEntity));

        }


        [HttpPost("RegisterAsTeacher")]
        public IActionResult RegisterAsTeacher([FromBody] RegisterAsTeachertDto teacher)
        {

            if (teacher == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            ApplicationUser teacherEntity = _mapper.Map<ApplicationUser>(teacher);
            RegisterValidation(teacherEntity);

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }


            _usersService.CreateTeacher(teacherEntity);

            return CreatedAtAction("GetById", new { id = teacherEntity.Id }, _mapper.Map<UserDto>(teacherEntity));
        }

        [HttpPost("RegisterAsSchoolManager")]
        public IActionResult RegisterAsSchoolManager([FromBody] RegisterAsSchoolManager schoolManager)
        {

            if (schoolManager == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            ApplicationUser schoolManagerEntity = _mapper.Map<ApplicationUser>(schoolManager);
            RegisterValidation(schoolManagerEntity);

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            _usersService.CreateSchoolManager(schoolManagerEntity);

            return CreatedAtAction("GetById", new { id = schoolManagerEntity.Id }, _mapper.Map<UserDto>(schoolManagerEntity));
        }


        [HttpDelete("{id}/delete")]
        [Authorize(Roles = Policies.Administrator)]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
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


        private async Task UserFieldValidationAsync(ApplicationUser user)
        {

            var similarUser = await _usersService.GetByEmail(user.Email);
            if (similarUser != null)
            {
                ModelState.AddModelError("email", "Email already exist");
            }
        }

        private void RegisterValidation(ApplicationUser user)
        {

            var similarUser = _usersService.GetByEmail(user.Email).Result;
            if (similarUser != null)
            {
                ModelState.AddModelError("email", "Email already exist");
            }

            if (user.Avatar != null)
            {

                AttachmentDTO attachmentDto = _consulHttpClient.GetAsync<AttachmentDTO>("attachment_ms", $"/api/Attachment/get/{user.Avatar}").Result;

                if (attachmentDto == null || attachmentDto.Id < 1)
                {
                    ModelState.AddModelError("Avatar", "Avatar not found");
                }
                else
                {
                    if (!ImgExtensions.All.Contains(attachmentDto.Extension.ToLower()))
                    {
                        ModelState.AddModelError("Audio", "Avatar not have valid extension, should be one of [" + string.Join(",", ImgExtensions.All) + "]");
                    }
                }

            }
        }
    }
}