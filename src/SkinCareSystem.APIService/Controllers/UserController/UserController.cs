using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers.UserController
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetUsersAsync(pageNumber, pageSize);
            return result.Status switch
            {
                Const.SUCCESS_READ_CODE => Ok(new { Data = result.Data, result.Message }),
                Const.WARNING_NO_DATA_CODE => NotFound(new { result.Message }),
                Const.ERROR_EXCEPTION => StatusCode(StatusCodes.Status500InternalServerError, new { result.Message }),
                _ => BadRequest(new { result.Message })
            };
        }
    }
}
