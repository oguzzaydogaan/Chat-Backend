using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backend.Controllers
{
    [Route("/calls")]
    [Authorize]
    public class CallController : ControllerBase
    {
        private readonly CallService _callService;
        private readonly ILogger<CallController> _logger;
        public CallController(ILogger<CallController> logger, CallService callService)
        {
            _logger = logger;
            _callService = callService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync() {
            try
            {
                var calls = await _callService.GetAllAsync();
                return Ok(calls);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving users: {ex.Message}");
                return StatusCode(500, "Something went wrong on the server. Please try again later.");
            }
        }

    }
}
