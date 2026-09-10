using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;

namespace SmartTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IEnumerable<string> GetModelErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
        }

        protected IActionResult ValidationError()
        {
            return BadRequest(ApiResult<string>.Failure(GetModelErrors()));
        }
    }
}
