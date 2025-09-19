using ECommerceRealTimeApp.Data;
using ECommerceRealTimeApp.DTOs;
using ECommerceRealTimeApp.DTOs.ProductDTOs;
using ECommerceRealTimeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceRealTimeApp.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ProductResponseDTO>> CreateProductAsync(ProductCreateDTO productCreateDTO)
        {
            if(await _context.Products.AnyAsync(x => x.Name.ToLower() == productCreateDTO.Name.ToLower()))
            {
                return new ApiResponse<ProductResponseDTO>(400, "Product name already exists");
            }

            if(!await _context.Categories.AnyAsync(x => x.Id == productCreateDTO.CategoryId))
            {
                return new ApiResponse<ProductResponseDTO>(400, "Specified category doesn't exist");
            }

            var product = new Product
            {
                Name = productCreateDTO.Name,
                CategoryId = productCreateDTO.CategoryId,
                Description = productCreateDTO.Description,
                Price = productCreateDTO.Price,
                ImageUrl = productCreateDTO.ImageUrl,
                StockQuantity = productCreateDTO.StockQuantity,
                DiscountPercentage = productCreateDTO.DiscountPercentage,
                IsAvailable = true
            };

            _context.Add(product);
            await _context.SaveChangesAsync();

            var addedProduct = await _context.Products.Where(x => x.Name.ToLower() == productCreateDTO.Name.ToLower())
                                                                                                    .FirstOrDefaultAsync();

            var productResponse = new ProductResponseDTO
            {
                Id = addedProduct.Id,
                Name = addedProduct.Name,
                Description = addedProduct.Description,
                Price = addedProduct.Price,
                StockQuantity = addedProduct.StockQuantity,
                ImageUrl = addedProduct.ImageUrl,
                DiscountPercentage = addedProduct.DiscountPercentage,
                CategoryId = addedProduct.CategoryId,
                IsAvailable = addedProduct.IsAvailable
            };

            return new ApiResponse<ProductResponseDTO>(200, productResponse);
        }

        public async Task<ApiResponse<ProductResponseDTO>> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return new ApiResponse<ProductResponseDTO>(404, "Product not found.");
            }

            var productResponse = new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                DiscountPercentage = product.DiscountPercentage,
                CategoryId = product.CategoryId,
                IsAvailable = product.IsAvailable
            };

            return new ApiResponse<ProductResponseDTO>(200, productResponse);
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateProductAsync(ProductUpdateDTO productUpdateDTO)
        {
            var product = await _context.Products.FindAsync(productUpdateDTO.Id);

            if (product == null)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");
            }

            if(await _context.Products.AnyAsync(x => x.Name.ToLower() == productUpdateDTO.Name.ToLower()
            && x.Id != productUpdateDTO.Id))
            {
                return new ApiResponse<ConfirmationResponseDTO>(400, "Another product with the same name already exists.");
            }

            if(!await _context.Categories.AnyAsync(x => x.Id == productUpdateDTO.CategoryId))
            {
                return new ApiResponse<ConfirmationResponseDTO>(400, "Specified category doesn't exist");
            }

            product.Name = productUpdateDTO.Name;
            product.Description = productUpdateDTO.Description;
            product.Price = productUpdateDTO.Price;
            product.StockQuantity = productUpdateDTO.StockQuantity;
            product.ImageUrl = productUpdateDTO.ImageUrl;
            product.DiscountPercentage = productUpdateDTO.DiscountPercentage;
            product.CategoryId = productUpdateDTO.CategoryId;
            await _context.SaveChangesAsync();

            var confirmationResponse = new ConfirmationResponseDTO
            {
                Message = "Product updated successfully."
            };

            return new ApiResponse<ConfirmationResponseDTO>(200, confirmationResponse);
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if(product == null)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");
            }

            product.IsAvailable = false;
            await _context.SaveChangesAsync();

            var confirmationResponse = new ConfirmationResponseDTO
            {
                Message = $"Product with Id {id} deleted successfully."
            };

            return new ApiResponse<ConfirmationResponseDTO>(200, confirmationResponse);
        }

        public async Task<ApiResponse<List<ProductResponseDTO>>> GetAllProductsAsync()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();

            var productList = products.Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                DiscountPercentage = p.DiscountPercentage,
                CategoryId = p.CategoryId,
                IsAvailable = p.IsAvailable
            }).ToList();

            return new ApiResponse<List<ProductResponseDTO>>(200, productList);
        }

        public async Task<ApiResponse<List<ProductResponseDTO>>> GetAllProductsByCategoryAsync(int categoryId)
        {
            var products = await _context.Products
                                .Include(p => p.Category)
                                .Where(p => p.CategoryId == categoryId)
                                .AsNoTracking()
                                .ToListAsync();

            if(products == null || products.Count == 0)
            {
                return new ApiResponse<List<ProductResponseDTO>>(404, "No products found for the specified category.");
            }

            var productList = products.Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                DiscountPercentage = p.DiscountPercentage,
                CategoryId = p.CategoryId,
                IsAvailable = p.IsAvailable
            }).ToList();

            return new ApiResponse<List<ProductResponseDTO>>(200, productList);
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateProductStatusAsync
            (ProductStatusUpdateDTO productStatusUpdateDTO)
        {
            var product = await _context.Products.FindAsync(productStatusUpdateDTO.ProductId);

            if(product == null)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");
            }

            product.IsAvailable = productStatusUpdateDTO.IsAvailable;
            await _context.SaveChangesAsync();

            var confirmationResponse = new ConfirmationResponseDTO
            {
                Message = $"Product status with Id {productStatusUpdateDTO.ProductId} updated successfully."
            };

            return new ApiResponse<ConfirmationResponseDTO>(200, confirmationResponse);
        }
    }
}
