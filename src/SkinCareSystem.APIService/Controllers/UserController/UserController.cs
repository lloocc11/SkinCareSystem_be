using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Services.InternalServices.IServices;

namespace SkinCareSystem.APIService.Controllers.UserController
{
    [ApiController]
    [Route("api/[controller]")]
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var result = await _userService.GetAllUsers();
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
