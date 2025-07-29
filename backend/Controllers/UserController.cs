using Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.DTOs;

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
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while retrieving users");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid format");
            try
            {
                var user = await _userService.GetByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occured while retrieving user");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> AddAsync([FromBody] RegisterRequestDTO registerRequest)
        {
            try
            {
                await _userService.RegisterAsync(registerRequest);
                return Ok();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occured while adding user");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("verify")]
        public async Task<IActionResult> VerifyAsync([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                var isConfirmed = await _userService.VerifyAsync(email, token);
                return Ok("Email verified successfully.");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }          
        }

        [HttpGet("{userId}/chats")]
        public async Task<IActionResult> GetChatsAsync(int userId)
        {
            try
            {
                var chats = await _userService.GetChatsAsync(userId);
                return Ok(chats);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occured while getting chats");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
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
            catch(EmailVerificationException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occured");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var response = await _userService.DeleteAsync(id);
                return Ok(response);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occured");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [HttpGet("{userId}/chats/search")]
        public async Task<IActionResult> SearchAsync(int userId, [FromQuery] string searchTerm)
        {
            try
            {
                var chats = await _userService.SearchChatsAsync(userId, searchTerm);
                return Ok(chats);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while searching chats");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
