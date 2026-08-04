using GreenBasket.Application.DTOs.Address;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenBasket.Application.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDTO>> GetUserAddressesAsync(string userId);
        Task<AddressDTO> GetAddressByIdAsync(int addressId, string userId);
        Task<AddressDTO> CreateAddressAsync(string userId, CreateAddressDTO model);
        Task<AddressDTO> UpdateAddressAsync(int addressId, string userId, UpdateAddressDTO model);
        Task<bool> DeleteAddressAsync(int addressId, string userId);
    }
}