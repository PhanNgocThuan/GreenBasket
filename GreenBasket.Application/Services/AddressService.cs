using GreenBasket.Application.DTOs.Address;
using GreenBasket.Application.Interfaces;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBasket.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AddressDTO>> GetUserAddressesAsync(string userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var result = new List<AddressDTO>();
            foreach (var a in addresses)
            {
                result.Add(MapToDTO(a));
            }
            return result;
        }

        public async Task<AddressDTO> GetAddressByIdAsync(int addressId, string userId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return null!;

            return MapToDTO(address);
        }

        public async Task<AddressDTO> CreateAddressAsync(string userId, CreateAddressDTO model)
        {
            if (model.IsDefault)
            {
                var existingAddresses = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();

                foreach (var addr in existingAddresses)
                {
                    addr.IsDefault = false;
                }
            }

            var address = new Address
            {
                UserId = userId,
                ReceiverName = model.ReceiverName,
                PhoneNumber = model.PhoneNumber,
                StreetAddress = model.StreetAddress,
                City = model.City,
                District = model.District,
                Ward = model.Ward,
                IsDefault = model.IsDefault
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return MapToDTO(address);
        }

        public async Task<AddressDTO> UpdateAddressAsync(int addressId, string userId, UpdateAddressDTO model)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return null!;

            if (model.IsDefault && !address.IsDefault)
            {
                var existingAddresses = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();

                foreach (var addr in existingAddresses)
                {
                    addr.IsDefault = false;
                }
            }

            address.ReceiverName = model.ReceiverName;
            address.PhoneNumber = model.PhoneNumber;
            address.StreetAddress = model.StreetAddress;
            address.City = model.City;
            address.District = model.District;
            address.Ward = model.Ward;
            address.IsDefault = model.IsDefault;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return MapToDTO(address);
        }

        public async Task<bool> DeleteAddressAsync(int addressId, string userId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

        private AddressDTO MapToDTO(Address address)
        {
            return new AddressDTO
            {
                Id = address.Id,
                ReceiverName = address.ReceiverName,
                PhoneNumber = address.PhoneNumber,
                StreetAddress = address.StreetAddress,
                City = address.City,
                District = address.District,
                Ward = address.Ward,
                IsDefault = address.IsDefault
            };
        }
    }
}