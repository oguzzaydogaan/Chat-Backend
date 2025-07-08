using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Repositories.Entities;
using Services;

namespace backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        private readonly UserService _userService;

        [HttpPost]
        public async Task<IActionResult> AddUserAsync([FromBody] User user)
        {
            if (user == null)
                return BadRequest("User cannot be null.");
            try
            {
                await _userService.AddUserAsync(user);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{userId}/chats")]
        public async Task<IActionResult> GetUsersChatsAsync(int userId)
        {
            try
            {
                var chats = await _userService.GetUsersChatsAsync(userId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDTO loginRequest)
        {
            try
            {
                var response = await _userService.LoginAsync(loginRequest.Email!, loginRequest.Password!);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDTO registerRequest)
        {
            try
            {
                await _userService.RegisterAsync(registerRequest);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
