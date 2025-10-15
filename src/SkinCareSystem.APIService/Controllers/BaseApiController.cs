using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.APIService.Controllers
{
    /// <summary>
    /// Base controller with helper methods for RESTful API responses
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Converts ServiceResult to appropriate RESTful HTTP response
        /// </summary>
        protected IActionResult ToHttpResponse(IServiceResult result, string? locationUri = null)
        {
            return result.Status switch
            {
                // 2xx Success
                200 => Ok(new { success = true, data = result.Data, message = result.Message }),
                201 => Created(locationUri ?? string.Empty, new { success = true, data = result.Data, message = result.Message }),
                204 => NoContent(),

                // 4xx Client Errors
                400 => BadRequest(new { success = false, message = result.Message }),
                401 => Unauthorized(new { success = false, message = result.Message }),
                403 => StatusCode(403, new { success = false, message = result.Message }),
                404 => NotFound(new { success = false, message = result.Message }),
                409 => Conflict(new { success = false, message = result.Message }),

                // 5xx Server Errors
                500 => StatusCode(500, new { success = false, message = result.Message }),

                // Default
                _ => StatusCode(result.Status, new { success = false, message = result.Message ?? "Unknown error" })
            };
        }
    }
}
