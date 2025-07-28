using Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Services;

namespace backend.Controllers
{
    [Route("api/messages")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }
        private readonly MessageService _messageService;

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var messages = await _messageService.GetAllAsync();
                return Ok(messages);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while adding message");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid format");
            try
            {
                var message = await _messageService.GetByIdAsync(id);
                return Ok(message);
            }
            catch(KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while adding message");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] Message message)
        {
            if (message == null)
                return BadRequest("Message cannot be null.");
            if (message.ChatId <= 0 || message.UserId <= 0)
                return BadRequest("Invalid chat ID or user ID.");
            try
            {
                await _messageService.AddAsync(message);
                return StatusCode(201);
            }
            catch(ChatNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UserNotMemberOfChatException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while adding message");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid message ID.");
            try
            {
                await _messageService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
