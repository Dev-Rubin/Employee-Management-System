using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.MicroService.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        [Authorize] 
        [HttpPost("TestAuthorize")]
        public IActionResult TestAuthorize()
        {
            return Ok("Only authenticated users can access");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("TestRole")]
        public IActionResult TestRole()
        {
            return Ok("Only Admin can access");
        }

        [AllowAnonymous]
        [HttpGet("TestAllowAnonymous")]
        public IActionResult TestAllowAnonymous()
        {
            return Ok("Any one can access");
        }
    }

}
