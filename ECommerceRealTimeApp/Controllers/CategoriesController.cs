using ECommerceRealTimeApp.DTOs;
using ECommerceRealTimeApp.DTOs.CategoryDTOs;
using ECommerceRealTimeApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ECommerceRealTimeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("CreateCategory")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDTO>>>
            CreateCategory([FromBody] CategoryCreateDTO categoryCreateDTO)
        {
            var response = await _categoryService.CreateCategoryAsync(categoryCreateDTO);

            if (response.StatusCode != Convert.ToInt32(HttpStatusCode.OK))
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpGet("GetCategoryById/{id}")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDTO>>>
            GetCategoryById([FromRoute] int id)
        {
            var response = await _categoryService.GetCategoryByIdAsync(id);

            if (response.StatusCode != Convert.ToInt32(HttpStatusCode.OK))
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpPut("UpdateCategory")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>>
            UpdateCategory([FromBody] CategoryUpdateDTO categoryUpdateDTO)
        {
            var response = await _categoryService.UpdateCategoryAsync(categoryUpdateDTO);

            if (response.StatusCode != Convert.ToInt32(HttpStatusCode.OK))
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpDelete("DeleteCategory/{id}")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>>
            DeleteCategory([FromRoute] int id)
        {
            var response = await _categoryService.DeleteCategoryAsync(id);

            if (response.StatusCode != Convert.ToInt32(HttpStatusCode.OK))
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpGet("GetAllCategories")]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDTO>>>> GetAllCategories()
        {
            var response = await _categoryService.GetAllCategoryAsync();

            if (response.StatusCode != Convert.ToInt32(HttpStatusCode.OK))
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
    }
}
