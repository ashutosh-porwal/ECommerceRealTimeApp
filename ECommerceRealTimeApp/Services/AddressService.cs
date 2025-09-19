using ECommerceRealTimeApp.Data;
using ECommerceRealTimeApp.DTOs;
using ECommerceRealTimeApp.DTOs.AddressDTOs;
using ECommerceRealTimeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceRealTimeApp.Services
{
    public class AddressService
    {
        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<AddressResponseDTO>> CreateAddressAsync(AddressCreateDTO addressCreateDTO)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(addressCreateDTO.CustomerId);

                if (customer == null)
                {
                    return new ApiResponse<AddressResponseDTO>(400, $"Customer not found.");
                }

                var address = new Address
                {
                    CustomerId = addressCreateDTO.CustomerId,
                    AddressLine1 = addressCreateDTO.AddressLine1,
                    AddressLine2 = addressCreateDTO.AddressLine2,
                    City = addressCreateDTO.City,
                    State = addressCreateDTO.State,
                    PostalCode = addressCreateDTO.PostalCode,
                    Country = addressCreateDTO.Country
                };

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                var addressResponse = new AddressResponseDTO
                {
                    Id = address.Id,
                    CustomerId = address.CustomerId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country
                };

                return new ApiResponse<AddressResponseDTO>(200, addressResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<AddressResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: " +
                    $"{ex.Message}");
            }
        }

        public async Task<ApiResponse<AddressResponseDTO>> GetAddressByIdAsync(int id)
        {
            try
            {
                var address = await _context.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                if (address == null)
                {
                    return new ApiResponse<AddressResponseDTO>(404, $"Address with ID {id} not found.");
                }

                var addressResponse = new AddressResponseDTO
                {
                    Id = address.Id,
                    CustomerId = address.CustomerId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country
                };

                return new ApiResponse<AddressResponseDTO>(200, addressResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<AddressResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateAddressAsync(AddressUpdateDTO addressDto)
        {
            try
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressDto.AddressId
                                                                            && a.CustomerId == addressDto.CustomerId);
                if (address == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, $"Address with ID {addressDto.AddressId} for Customer ID {addressDto.CustomerId} not found.");
                }

                address.AddressLine1 = addressDto.AddressLine1;
                address.AddressLine2 = addressDto.AddressLine2;
                address.City = addressDto.City;
                address.State = addressDto.State;
                address.PostalCode = addressDto.PostalCode;
                address.Country = addressDto.Country;
                await _context.SaveChangesAsync();

                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Address with ID {address.Id} updated successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteAddressAsync(AddressDeleteDTO addressDeleteDTO)
        {
            try
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressDeleteDTO.AddressId
                                                                            && a.CustomerId == addressDeleteDTO.CustomerId);

                if (address == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, $"Address with ID {addressDeleteDTO.AddressId} for " +
                        $"Customer ID {addressDeleteDTO.CustomerId} not found.");
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Address with ID {address.Id} deleted successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<AddressResponseDTO>>> GetAddressesByCustomerIdAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .Include(c => c.Addresses)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                {
                    return new ApiResponse<List<AddressResponseDTO>>(404, $"Customer not found.");
                }

                var addresses = customer.Addresses.Select(a => new AddressResponseDTO
                {
                    Id = a.Id,
                    CustomerId = a.CustomerId,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country
                }).ToList();

                return new ApiResponse<List<AddressResponseDTO>>(200, addresses);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<AddressResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }
    }
}
