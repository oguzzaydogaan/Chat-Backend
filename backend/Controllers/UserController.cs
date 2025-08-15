using Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.DTOs;

namespace backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }     

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving users: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }

        [HttpGet("verifieds")]
        public async Task<IActionResult> GetAllVerifieds()
        {
            try
            {
                var users = await _userService.GetVerifiedsAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving verfied users: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding user: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying user: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user's chats: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging in: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error seaching user's chats: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }


    }
}
