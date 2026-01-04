using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Requests;
using Services.Interfaces;

namespace TicketEvent.Login.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                var newId = _authService.Register(request);
                return Ok(new
                {
                    NguoiDungID = newId,
                    request.TenDangNhap,
                    request.HoTen,
                    request.Email
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = _authService.Login(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = _authService.Refresh(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("revoke")]
        public IActionResult Revoke([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _authService.RevokeRefreshToken(request);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("revoke-all")]
        public IActionResult RevokeAll([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _authService.RevokeAllRefreshTokens(request);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
