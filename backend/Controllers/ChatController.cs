using AutoMapper;
using Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.DTOs;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/chats")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatController> _logger;
        public ChatController(ChatService chatService, IMapper mapper, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var chats = await _chatService.GetAllAsync();
                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving chats: {ex.Message}");
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
                var chat = await _chatService.GetByIdAsync(id);
                return Ok(chat);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving chats: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }

        [HttpGet("{chatId}/users/{userId}")]
        public async Task<IActionResult> GetChatWithMessagesAsync(int userId, int chatId)
        {
            if (chatId <= 0 || userId <= 0)
                return BadRequest("Invalid chat ID");
            try
            {
                var chat = await _chatService.GetChatWithMessagesAsync(chatId, userId);
                return Ok(JsonSerializer.Serialize(chat));
            }
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving chat: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] CreateChatWithCreatorDTO dto)
        {
            try
            {
                var chat = await _chatService.AddAsync(dto.Chat, dto.Creator);
                return Ok(_mapper.Map<CreateChatResponseDTO>(chat));
            }
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding chat: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }

        [HttpPost("{chatId}/users/{userId}")]
        public async Task<IActionResult> AddUserAsync(int chatId, int userId, [FromBody] UserDTO sender)
        {
            if (chatId <= 0 || userId <= 0)
                return BadRequest("Invalid chat ID or user ID");
            try
            {
                var updatedChat = await _chatService.AddUserAsync(chatId, userId, sender);
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

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteAsync(int chatId)
        {
            if (chatId <= 0)
                return BadRequest("Invalid chat ID.");
            try
            {
                await _chatService.DeleteAsync(chatId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting chat: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }

        [HttpGet("{chatId}/users/search")]
        public async Task<IActionResult> SearchUsersAsync(int chatId, [FromQuery] string searchTerm)
        {
            try
            {
                var users = await _chatService.SearchUsersAsync(chatId, searchTerm);
                return Ok(JsonSerializer.Serialize(users));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching chat's users: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }
    }
}