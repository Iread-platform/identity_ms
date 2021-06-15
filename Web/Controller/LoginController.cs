using AutoMapper;
using iread_identity_ms.Web.Service;
using Microsoft.AspNetCore.Mvc;

namespace iread_identity_ms.Web.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    { 
        private readonly AppUsersService _usersService;
        private readonly IMapper _mapper;

        public LoginController(AppUsersService usersService,
             IMapper mapper)
        {
            _usersService = usersService;
            _mapper = mapper;
        }



    // public void Test(){

    //         // request token
    //     var tokenClient = new TokenClient("http://localhost:5000/connect/token", "ro.client", "secret");
    //     var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1");

    //     if (tokenResponse.IsError)
    //     {
    //         Console.WriteLine(tokenResponse.Error);
    //         return;
    //     }

    //     Console.WriteLine(tokenResponse.Json);
    //     Console.WriteLine("\n\n");
    //     }

    }
}