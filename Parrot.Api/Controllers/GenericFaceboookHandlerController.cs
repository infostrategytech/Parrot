using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Parrot.Api.Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericFaceboookHandlerController : ControllerBase
    {
        [HttpPost("handle/{messagingPlatform}")]
        public IActionResult HandleIncomingMessage(string messagingPlatform, [FromBody] dynamic request)
        {
            // In a real implementation, you would process the incoming message based on the messaging platform
            // For example, you might route the message to a service that handles messages for that platform
            return Ok(new { Message = $"Received message from {messagingPlatform}", Content = request.Content });
        }
    }
}
