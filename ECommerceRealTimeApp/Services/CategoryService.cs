using ECommerceRealTimeApp.Data;
using ECommerceRealTimeApp.DTOs;
using ECommerceRealTimeApp.DTOs.CategoryDTOs;
using ECommerceRealTimeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceRealTimeApp.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CategoryResponseDTO>> CreateCategoryAsync(CategoryCreateDTO categoryCreateDTO)
        {
            try
            {
                if(await _context.Categories.AnyAsync(c => c.Name.ToLower() == categoryCreateDTO.Name.ToLower()))
                {
                    return new ApiResponse<CategoryResponseDTO>(400, "A category with the same name already exists.");
                }

                var category = new Category
                {
                    Name = categoryCreateDTO.Name,
                    Description = categoryCreateDTO.Description,
                    IsActive = true
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var categoryRespone = new CategoryResponseDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive
                };

                return new ApiResponse<CategoryResponseDTO>(200, categoryRespone);
            }
            catch (Exception ex)
            {
                return new ApiResponse<CategoryResponseDTO>(500, $"An error occurred while creating the category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryResponseDTO>> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return new ApiResponse<CategoryResponseDTO>(404, "Category not found.");
            }

            var categoryResponse = new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return new ApiResponse<CategoryResponseDTO>(200, categoryResponse);
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateCategoryAsync(CategoryUpdateDTO categoryUpdateDTO)
        {
            var category = await _context.Categories.FindAsync(categoryUpdateDTO.Id);

            if (category == null)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Category not found.");
            }

            if((category.Name.ToLower() == categoryUpdateDTO.Name.ToLower()) 
                && (category.Id != categoryUpdateDTO.Id))
            {
                return new ApiResponse<ConfirmationResponseDTO>(400, "A category with the same name already exists.");
            }

            category.Name = categoryUpdateDTO.Name;
            category.Description = categoryUpdateDTO.Description;

            await _context.SaveChangesAsync();

            var confirmationMessage = new ConfirmationResponseDTO
            {
                Message = "Category updated successfully."
            };

            return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Category not found.");
            }

            category.IsActive = false;
            await _context.SaveChangesAsync();

            var confirmationMessage = new ConfirmationResponseDTO
            {
                Message = $"Category with Id {category.Id} deleted successfully."
            };

            return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
        }

        public async Task<ApiResponse<List<CategoryResponseDTO>>> GetAllCategoryAsync()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();

            var categoryList = categories.Select(category => new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            }).ToList();

            return new ApiResponse<List<CategoryResponseDTO>>(200, categoryList);
        }
    }
}
