using AutoMapper;
using Exceptions;
using Microsoft.AspNetCore.Authorization;
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
        private readonly MessageService _messageService;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageController> _logger;
        public MessageController(MessageService messageService, IMapper mapper, ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var messages = await _messageService.GetAllAsync();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving messages: {ex.Message}");
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
                var message = await _messageService.GetByIdAsync(id);
                return Ok(message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving message: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] Message message)
        {
            if (message.ChatId <= 0 || message.UserId <= 0)
                return BadRequest("Invalid chat ID or user ID.");
            try
            {
                
                await _messageService.AddAsync(message);
                return StatusCode(201);
            }
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding message: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
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
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving messages: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }
    }
}
