using GreenBasket.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace GreenBasket.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO model);
        Task<AuthResponseDTO> LoginAsync(LoginDTO model);
    }
}