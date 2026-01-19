using EMS.Application.Command.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EMS.MicroService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenCommand cmd)
            => Ok(await _mediator.Send(cmd));
    }

}
