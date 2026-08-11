using GreenBasket.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace GreenBasket.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO model);
        Task<AuthResponseDTO> LoginAsync(LoginDTO model);
        Task<bool> VerifyEmailAsync(string email, string otp);
        Task<bool> ResendOtpAsync(string email);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO model);
    }
}