using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using Server.Managers.Interfaces;
using ServerLibrary.Repositories.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationManager _authManager;

        public AuthenticationController(IAuthenticationManager authManager)
        {
            _authManager = authManager;
        }


        /// <summary>
        /// /api/authentication/register
        /// Async method to create an account
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest("Model cannot be null.");
            } 

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var response = await _authManager.CreateAccountByManagerAsync(registerDto);
            if(!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
