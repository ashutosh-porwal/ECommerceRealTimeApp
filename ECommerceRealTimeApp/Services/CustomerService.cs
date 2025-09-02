using ECommerceRealTimeApp.Data;
using ECommerceRealTimeApp.DTOs;
using ECommerceRealTimeApp.DTOs.CustomerDTOs;
using ECommerceRealTimeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceRealTimeApp.Services
{
    public class CustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CustomerResponseDTO>> RegisterCustomerAsync(CustomerRegistrationDTO customerRegistrationDTO)
        {
            try
            {
                //check if email already exists
                var existingCustomer = await _context.Customers
                                            .AnyAsync(x => x.Email.ToLower() == customerRegistrationDTO.Email.ToLower());

                if (existingCustomer)
                {
                    return new ApiResponse<CustomerResponseDTO>(400, "Email is already in use.");
                }

                var customer = new Customer
                {
                    FirstName = customerRegistrationDTO.FirstName,
                    LastName = customerRegistrationDTO.LastName,
                    Email = customerRegistrationDTO.Email,
                    PhoneNumber = customerRegistrationDTO.PhoneNumber,
                    DateOfBirth = customerRegistrationDTO.DateOfBirth,
                    IsActive = true,
                    // Hash the password using BCryp
                    Password = BCrypt.Net.BCrypt.HashPassword(customerRegistrationDTO.Password)
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                //prepare response dto
                var customerResponseDTO = new CustomerResponseDTO
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    DateOfBirth = customer.DateOfBirth
                };

                return new ApiResponse<CustomerResponseDTO>(201, customerResponseDTO);
            }
            catch (Exception ex)
            {
                // Log the exception (implementation depends on your logging setup)
                return new ApiResponse<CustomerResponseDTO>(500, $"An unexpected error occurred while processing your request, " +
                    $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Email == loginDTO.Email);

                if (customer == null)
                    return new ApiResponse<LoginResponseDTO>(401, "Invalid email or password.");

                //varify password using BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDTO.Password, customer.Password);

                if (!isPasswordValid)
                    return new ApiResponse<LoginResponseDTO>(401, "Invalid email or password.");

                //prepare response dto
                var loginResponseDTO = new LoginResponseDTO
                {
                    Message = "Login successful",
                    CustomerId = customer.Id,
                    CustomerName = $"{customer.FirstName} {customer.LastName}"
                };

                return new ApiResponse<LoginResponseDTO>(200, loginResponseDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<LoginResponseDTO>(500,
                    $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CustomerResponseDTO>> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    return new ApiResponse<CustomerResponseDTO>(404, "Customer not found.");

                //prepare response dto
                var customerResponseDTO = new CustomerResponseDTO
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    DateOfBirth = customer.DateOfBirth
                };

                return new ApiResponse<CustomerResponseDTO>(200, customerResponseDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<CustomerResponseDTO>(500,
                    $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateCustomerAsync(CustomerUpdateDTO customerUpdateDTO)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerUpdateDTO.CustomerId);

                if (customer == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found.");
                }

                if (customer.Email.ToLower() != customerUpdateDTO.Email.ToLower()
                    && await _context.Customers.AnyAsync(c => c.Email.ToLower() == customerUpdateDTO.Email.ToLower()))
                {
                    return new ApiResponse<ConfirmationResponseDTO>(400, "Email is already in use.");
                }

                //update customer properties manually
                customer.FirstName = customerUpdateDTO.FirstName;
                customer.LastName = customerUpdateDTO.LastName;
                customer.Email = customerUpdateDTO.Email;
                customer.PhoneNumber = customerUpdateDTO.PhoneNumber;
                customer.DateOfBirth = customerUpdateDTO.DateOfBirth;
                await _context.SaveChangesAsync();

                var confirmationResponseDTO = new ConfirmationResponseDTO
                {
                    Message = $"Customer with Id {customerUpdateDTO.CustomerId} updated successfully"
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationResponseDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<ConfirmationResponseDTO>(500,
                    $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId);
                if (customer == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found.");
                }
                //Soft Delete
                customer.IsActive = false;
                await _context.SaveChangesAsync();
                // Prepare confirmation message
                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Customer with Id {customerId} deleted successfully."
                };
                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> ChangePasswordAsync(ChangePasswordDTO changePasswordDto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(changePasswordDto.CustomerId);

                if(customer == null || !customer.IsActive)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found or inactive.");
                }

                //varify current password
                bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, customer.Password);

                if(!isCurrentPasswordValid)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(400, "Current password is incorrect.");
                }

                //Hash the new password using BCrypt
                customer.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                await _context.SaveChangesAsync();

                var confirmationResponseDTO = new ConfirmationResponseDTO
                {
                    Message = "Password changed successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationResponseDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<ConfirmationResponseDTO>(500,
                    $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }
    }
}
