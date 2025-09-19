using ECommerceRealTimeApp.DTOs.ProductDTOs;
using ECommerceRealTimeApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceRealTimeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDTO productCreateDTO)
        {
            var response = await _productService.CreateProductAsync(productCreateDTO);

            if(response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpGet("GetProductById/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var response = await _productService.GetProductByIdAsync(id);

            if(response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateDTO productUpdateDTO)
        {
            var response = await _productService.UpdateProductAsync(productUpdateDTO);

            if(response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            var response = await _productService.DeleteProductAsync(id);
            if(response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var response = await _productService.GetAllProductsAsync();
            if(response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("GetProductsByCategory/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory([FromRoute] int categoryId)
        {
            var response = await _productService.GetAllProductsByCategoryAsync(categoryId);
            if(response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpPut("UpdateProductStatus")]
        public async Task<IActionResult> UpdateProductStatus([FromBody] ProductStatusUpdateDTO productStatusUpdateDTO)
        {
            var response = await _productService.UpdateProductStatusAsync(productStatusUpdateDTO);
            if(response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }
    }
}
