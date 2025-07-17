using Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Services;
using Services.DTOs;
using Services.Mappers;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/chats")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }
        private readonly ChatService _chatService;

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var chats = await _chatService.GetAllAsync();
                return Ok(chats);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while retrieving chat");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while retrieving chat");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] CreateChatRequestDTO dto)
        {
            try
            {
                var chat = await _chatService.AddAsync(dto);
                return Ok(chat.ToCreateChatResponseDTO());
            }
            catch (ChatAlreadyExistException ex)
            {
                return Conflict(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while creating chat");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("{chatId}/users/{userId}")]
        public async Task<IActionResult> AddUserAsync(int chatId, int userId)
        {
            if (chatId <= 0 || userId <= 0)
                return BadRequest("Invalid chat ID or user ID");
            try
            {
                var updatedChat = await _chatService.AddUserAsync(chatId, userId);
                return Ok();
            }
            catch (UsersNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ChatNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UserAlreadyExistException ex)
            {
                return Conflict(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while adding user to chat");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error occurred while deleting chat");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
