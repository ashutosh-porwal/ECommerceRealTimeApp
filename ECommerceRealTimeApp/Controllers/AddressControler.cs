using ECommerceRealTimeApp.DTOs;
using ECommerceRealTimeApp.DTOs.AddressDTOs;
using ECommerceRealTimeApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceRealTimeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressControler : ControllerBase
    {
        private readonly AddressService _addressService;

        public AddressControler(AddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPost("CreateAddress")]
        public async Task<ActionResult<ApiResponse<AddressResponseDTO>>> CreateAddressAsync(
            [FromBody] AddressCreateDTO addressCreateDTO)
        {
            var response = await _addressService.CreateAddressAsync(addressCreateDTO);

            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpGet("GetAddressById/{id}")]
        public async Task<ActionResult<ApiResponse<AddressResponseDTO>>> GetAddressByIdAsync(int id)
        {
            var response = await _addressService.GetAddressByIdAsync(id);

            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpPut("UpdateAddress")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> UpdateAddress
            ([FromBody] AddressUpdateDTO addressUpdateDTO)
        {
            var response = await _addressService.UpdateAddressAsync(addressUpdateDTO);

            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpDelete("DeleteAddress")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> DeleteAddress
            ([FromBody] AddressDeleteDTO addressDeleteDTO)
        {
            var response = await _addressService.DeleteAddressAsync(addressDeleteDTO);

            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpGet("GetAddressesByCustomerId/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<AddressResponseDTO>>>> GetAddressesByCustomerIdAsync(int customerId)
        {
            var response = await _addressService.GetAddressesByCustomerIdAsync(customerId);

            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
    }
}
