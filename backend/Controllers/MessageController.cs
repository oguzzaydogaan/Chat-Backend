using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<IActionResult> AddMessageAsync([FromBody] Message message)
        {
            if (message == null)
                return BadRequest("Message cannot be null.");
            if (message.ChatId <= 0 || message.UserId <= 0)
                return BadRequest("Invalid chat ID or user ID.");
            try
            {
                await _messageService.AddMessageAsync(message);
                return StatusCode(201);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessageAsync(int messageId)
        {
            if (messageId <= 0)
                return BadRequest("Invalid message ID.");
            try
            {
                await _messageService.DeleteMessageAsync(messageId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
