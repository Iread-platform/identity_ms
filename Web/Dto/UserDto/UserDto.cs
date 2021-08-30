
using System;
using iread_interaction_ms.Web.DTO.AttachmentDTO;

namespace iread_identity_ms.Web.Dto.UserDto
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Id { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public int Level { get; set; }
        public DateTime BirthDay { get; set; }
        public AttachmentDTO AvatarAttachment { get; set; }
        public AttachmentDTO CustomPhotoAttachment { get; set; }


    }
}