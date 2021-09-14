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
using IdentityModel.Client;
using System.Threading;
using IdentityServer4.AccessTokenValidation;
using iread_identity_ms.Web.Dto.SchoolMemberDto;

namespace M3allem.M3allem.Controller
{
    
    [Route("api/[controller]/")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly AppUsersService _usersService;
        private readonly MailService _mailService;
        private readonly IMapper _mapper;
        private readonly string _schoolMs = "school_ms";
        private readonly IConsulHttpClientService _consulHttpClient;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityController(
            IPublicRepository repository,
            AppUsersService usersService,
             IMapper mapper,
             UserManager<ApplicationUser> userManager,
             IConsulHttpClientService consulHttpClient, MailService mailService)
        {
            _usersService = usersService;
            _mapper = mapper;
            _userManager = userManager;
            _consulHttpClient = consulHttpClient;
            _mailService = mailService;
        }

        [HttpGet("all")]
        [Authorize(Roles = Policies.Administrator)]
        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            return _mapper.Map<IEnumerable<UserDto>>(await _usersService.GetAll());
        }


        [HttpGet("get-by-role")]
        public async Task<IActionResult> GetByRole([FromQuery(Name = "role")] string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            if (String.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError("role", "role is required");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            List<string> validRoles = new List<string>(){
                RoleTypes.Student.ToString(),
                RoleTypes.Teacher.ToString()
            };
            if (!(validRoles.Contains(role)))
            {
                ModelState.AddModelError("role", $"role is not valid should be one of [{String.Join(",", validRoles.ToArray())}].");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            List<ApplicationUser> users = await _usersService.GetByRole(role);

            if (users == null || !users.Any())
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<UserDto>>(users));
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

            UserDto res = _mapper.Map<UserDto>(user);
            res.AvatarAttachment = user.Avatar != null ? await _consulHttpClient.GetAsync<AttachmentDTO>("attachment_ms", $"/api/Avatar/get/{user.Avatar}") : null;
            res.CustomPhotoAttachment = user.CustomPhoto != null ? await _consulHttpClient.GetAsync<AttachmentDTO>("attachment_ms", $"/api/Attachment/get/{user.CustomPhoto}") : null;

            return Ok(res);
        }

        [HttpGet]
        [Route("myProfile")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> MyProfileAsync()
        {
            string myId = User.Claims.Where(c => c.Type == "sub")
                .Select(c => c.Value).SingleOrDefault();

            var user = await _usersService.GetById(myId);
            UserDto res = _mapper.Map<UserDto>(user);
            res.AvatarAttachment = user.Avatar != null ? await _consulHttpClient.GetAsync<AttachmentDTO>("attachment_ms", $"/api/Avatar/get/{user.Avatar}") : null;
            res.CustomPhotoAttachment = user.CustomPhoto != null ? await _consulHttpClient.GetAsync<AttachmentDTO>("attachment_ms", $"/api/Attachment/get/{user.CustomPhoto}") : null;

            return Ok(res);
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

        [Authorize(Roles = Policies.SchoolManager,AuthenticationSchemes = "Bearer")]
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

            studentEntity.PasswordHash = student.Password;
            _usersService.CreateStudent(studentEntity);

            //Add student to school members
            int schoolId = int.Parse(User.Claims.Where(c => c.Type == "SchoolId").Select(c => c.Value).SingleOrDefault());
            IActionResult res = null;
          
            StudentDto studentDto = new StudentDto()
            {
                MemberId = studentEntity.Id
            };
            try
            {
                res = _consulHttpClient.PostBodyAsync<IActionResult>(_schoolMs, $"api/School/{schoolId}/student/add", studentDto).Result;
            }
            catch (Exception e)
            {
                ModelState.AddModelError("School", e.Message);
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            return CreatedAtAction("GetById", new { id = studentEntity.Id }, _mapper.Map<UserDto>(studentEntity));

        }

        [Authorize(Roles = Policies.SchoolManager,AuthenticationSchemes = "Bearer")]
        [HttpPut("UpdateStudentInfo/{studentId}")]
        public  IActionResult UpdateStudentInfo([FromRoute] string studentId, [FromBody] UpdateStudentDto student)
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
            studentEntity.Id = studentId;
            
            ApplicationUser oldStudent =  _usersService.GetById(studentId).GetAwaiter().GetResult();
            if (oldStudent == null)
            {
                return NotFound();
            }

            if (oldStudent.Role != Policies.Student)
            {
                ModelState.AddModelError("Role", "This account is not a student account.");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            _usersService.Update(oldStudent, studentEntity);

            IActionResult res = null;
            UpdateStudentMemberDto studentMemberDto = new UpdateStudentMemberDto()
            {
                FirstName = studentEntity.FirstName,
                LastName = studentEntity.LastName
            };
            try
            {
                res = _consulHttpClient.PutBodyAsync<IActionResult>(_schoolMs, $"api/School/UpdateMemberInfo/{studentId}", studentMemberDto).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("School", e.Message);
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            
            return NoContent();
        }
        
        [Authorize(Roles = Policies.SchoolManager,AuthenticationSchemes = "Bearer")]
        [HttpPut("UpdateTeacherInfo/{teacherId}")]
        public  IActionResult UpdateTeacherInfo([FromRoute] string teacherId, [FromBody] UpdateTeacherDto teacher)
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
            teacherEntity.Id = teacherId;
            
            ApplicationUser oldTeacher =  _usersService.GetById(teacherId).GetAwaiter().GetResult();
            if (oldTeacher == null)
            {
                return NotFound();
            }

            if (oldTeacher.Role != Policies.Teacher)
            {
                ModelState.AddModelError("Role", "This account is not a student account.");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            _usersService.Update(oldTeacher, teacherEntity);

            IActionResult res = null;
            UpdateTeacherDto teacherMemberDto = new UpdateTeacherDto()
            {
                FirstName = teacherEntity.FirstName,
                LastName = teacherEntity.LastName
            };
            try
            {
                res = _consulHttpClient.PutBodyAsync<IActionResult>(_schoolMs, $"api/School/UpdateMemberInfo/{teacherId}", teacherMemberDto).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("School", e.Message);
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            
            return NoContent();
        }

        [HttpPost("RegisterAsTeacher")]
        [Authorize(Roles = Policies.SchoolManager,AuthenticationSchemes = "Bearer")]
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

            teacherEntity.PasswordHash = teacher.Password;
            _usersService.CreateTeacher(teacherEntity);
            
            //Add teacher to school members
            int schoolId = int.Parse(User.Claims.Where(c => c.Type == "SchoolId").Select(c => c.Value).SingleOrDefault());
           
            IActionResult res = null;
            TeacherDto teacherDto = new TeacherDto()
            {
                MemberId = teacherEntity.Id
            };
            try
            {
                res = _consulHttpClient.PostBodyAsync<IActionResult>(_schoolMs, $"api/School/{schoolId}/teacher/add", teacherDto).Result;
            }
            catch (Exception e)
            {
                ModelState.AddModelError("School", e.Message);
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            

            return CreatedAtAction("GetById", new { id = teacherEntity.Id }, _mapper.Map<UserDto>(teacherEntity));
        }

        [HttpPut("ResetPasswordForTeacher/{id}")]
        [Authorize(Roles = Policies.SchoolManager,AuthenticationSchemes = "Bearer")]
        public IActionResult ResetPasswordForTeacher([FromBody] ResetPasswordDto resetPasswordDto, [FromRoute] string id)
        {

            if (resetPasswordDto == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            ApplicationUser user =  _usersService.GetById(id).Result;

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role != Policies.Teacher)
            {
                ModelState.AddModelError("Role", "This account is not a teacher account");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            _usersService.ResetPassword(user, resetPasswordDto.NewPassword);
                
            string body = "Hello, here is your new password, We made it easy to remember :D \n The new password is : "+ resetPasswordDto.NewPassword;
            
            _mailService.SendEmail(user.Email , "New password", body);

            return Ok(resetPasswordDto);
        }

        [HttpPut("ResetPasswordForStudent/{id}")]
        [Authorize(Roles = Policies.SchoolManager,AuthenticationSchemes = "Bearer")]
        public IActionResult ResetPasswordForStudent([FromBody] ResetPasswordDto resetPasswordDto, [FromRoute] string id)
        {

            if (resetPasswordDto == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            ApplicationUser user =  _usersService.GetById(id).Result;

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role != Policies.Student)
            {
                ModelState.AddModelError("Role", "This account is not a student account");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            _usersService.ResetPassword(user, resetPasswordDto.NewPassword);
                
            string body = "Hello, here is your new password, We made it easy to remember :D  \n Please make sure you keep it in a safe place.\n The new password is: " + resetPasswordDto.NewPassword;
            
            _mailService.SendEmail(user.Email , "New password", body);

            return Ok(resetPasswordDto);
        }

        [HttpPut("ResetPasswordForManager/{id}")]
        [Authorize(Roles = Policies.Administrator,AuthenticationSchemes = "Bearer")]
        public IActionResult ResetPasswordForManager([FromBody] ResetPasswordDto resetPasswordDto, [FromRoute] string id)
        {

            if (resetPasswordDto == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }
            ApplicationUser user =  _usersService.GetById(id).Result;

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role != Policies.SchoolManager)
            {
                ModelState.AddModelError("Role", "This account is not a school manager account");
                return BadRequest(Startup.GetErrorsFromModelState(ModelState));
            }

            _usersService.ResetPassword(user, resetPasswordDto.NewPassword);
                
            string body = "Hello, here is your new password, We made it easy to remember :D \n The new password is : "+ resetPasswordDto.NewPassword;
            
            _mailService.SendEmail(user.Email , "New password", body);

            return Ok(resetPasswordDto);
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

            schoolManagerEntity.PasswordHash = schoolManager.Password;
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

                AttachmentDTO attachmentDto = _consulHttpClient.GetAsync<AttachmentDTO>("attachment_ms", $"/api/Avatar/get/{user.Avatar}").Result;

                if (attachmentDto == null || attachmentDto.Id < 1)
                {
                    ModelState.AddModelError("Avatar", "Avatar not found");
                }
                else
                {
                    if (!ImgExtensions.All.Contains(attachmentDto.Extension.ToLower()))
                    {
                        ModelState.AddModelError("Avatar", "Avatar not have valid extension, should be one of [" + string.Join(",", ImgExtensions.All) + "]");
                    }
                }

            }

            if (user.CustomPhoto != null)
            {

                AttachmentDTO attachmentDto = _consulHttpClient.GetAsync<AttachmentDTO>("attachment_ms", $"/api/Attachment/get/{user.CustomPhoto}").Result;

                if (attachmentDto == null || attachmentDto.Id < 1)
                {
                    ModelState.AddModelError("CustomPhoto", "CustomPhoto not found");
                }
                else
                {
                    if (!ImgExtensions.All.Contains(attachmentDto.Extension.ToLower()))
                    {
                        ModelState.AddModelError("CustomPhoto", "CustomPhoto not have valid extension, should be one of [" + string.Join(",", ImgExtensions.All) + "]");
                    }
                }

            }
        }
    }
}