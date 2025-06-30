using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Entities;
using Services;

namespace backend.Controllers
{
    [Route("api/users")]
    [ApiController]
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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUsersChatsAsync(int userId)
        {
            try
            {              
                return Ok(await _userService.GetUsersChatsAsync(userId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
