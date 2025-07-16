using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> AddAsync([FromBody] RegisterRequestDTO registerRequest)
        {
            try
            {
                await _userService.AddAsync(registerRequest);
                return Ok();
            }
            catch(DbUpdateException)
            {
                return StatusCode(500, "An error occured");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [HttpGet("{userId}/chats")]
        public async Task<IActionResult> GetUsersChatsAsync(int userId)
        {
            try
            {
                var chats = await _userService.GetChatsAsync(userId);
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
            catch (DbUpdateException)
            {
                return StatusCode(500, "An error occured");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        
    }
}
