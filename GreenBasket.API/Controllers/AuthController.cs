using GreenBasket.Application.DTOs.Auth;
using GreenBasket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GreenBasket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Tiêm IAuthService thông qua Dependency Injection
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                // ModelState tự động kiểm tra các Data Annotations (Email, Password length...)
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RegisterAsync(model);
                return Ok(new { IsSuccess = true, Message = "Registration successful!", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.LoginAsync(model);
                return Ok(new { IsSuccess = true, Message = "Login successful!", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] ResetPasswordDTO model) // Reusing DTO for convenience, normally would have a VerifyEmailDTO with Email, Otp
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(model.Email, model.Otp);
                return Ok(new { IsSuccess = true, Message = "Email verified successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] LoginDTO model) // Using LoginDTO just to get the Email
        {
            try
            {
                await _authService.ResendOtpAsync(model.Email);
                return Ok(new { IsSuccess = true, Message = "A new OTP has been sent to your email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] LoginDTO model)
        {
            try
            {
                await _authService.ForgotPasswordAsync(model.Email);
                return Ok(new { IsSuccess = true, Message = "If that email exists, an OTP has been sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _authService.ResetPasswordAsync(model);
                return Ok(new { IsSuccess = true, Message = "Password has been reset successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
        }
    }
}