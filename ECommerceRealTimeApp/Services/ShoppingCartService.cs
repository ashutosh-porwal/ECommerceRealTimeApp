using ECommerceRealTimeApp.Data;
using ECommerceRealTimeApp.DTOs;
using ECommerceRealTimeApp.DTOs.ShoppingCartDTOs;
using ECommerceRealTimeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceRealTimeApp.Services
{
    public class ShoppingCartService
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CartResponseDTO>> GetCartByCustomerIdAsync(int customerId)
        {
            var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(p => p.ProductId)
                        .FirstOrDefaultAsync(x => x.CustomerId == customerId && !x.IsCheckedOut);

            if (cart == null)
            {
                var emptyCartDto = new CartResponseDTO
                {
                    CustomerId = customerId,
                    IsCheckedOut = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CartItems = new List<CartItemResponseDTO>(),
                    TotalBasePrice = 0,
                    TotalDiscount = 0,
                    TotalAmount = 0
                };

                return new ApiResponse<CartResponseDTO>(200, emptyCartDto);
            }

            var cartDto = MapCartToDTO(cart);

            return new ApiResponse<CartResponseDTO>(200, cartDto);
        }

        public async Task<ApiResponse<CartResponseDTO>> AddToCartAsync(AddToCartDTO addToCartDTO)
        {
            var product = await _context.Products.FindAsync(addToCartDTO.ProductId);

            if (product == null || !product.IsAvailable)
            {
                return new ApiResponse<CartResponseDTO>(404, "Product not found or is not available.");
            }

            if (addToCartDTO.Quantity > product.StockQuantity)
            {
                return new ApiResponse<CartResponseDTO>(400, $"Only {product.StockQuantity} units of {product.Name} are available.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == addToCartDTO.CustomerId && !c.IsCheckedOut);

            if (cart == null)
            {
                var cartItems = new List<CartItem>
                {
                    new CartItem
                    {
                        ProductId = addToCartDTO.ProductId,
                        Quantity = addToCartDTO.Quantity,
                        UnitPrice = product.Price,
                        Discount = (product.Price * product.DiscountPercentage) / 100,
                        TotalPrice = (product.Price - (product.Price * product.DiscountPercentage) / 100) * addToCartDTO.Quantity
                    }
                };

                cart = new Cart
                {
                    CustomerId = addToCartDTO.CustomerId,
                    IsCheckedOut = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CartItems = cartItems
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                return new ApiResponse<CartResponseDTO>(200, MapCartToDTO(cart));
            }

            // Check if the product is already in the cart.
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addToCartDTO.ProductId);
            if (existingCartItem != null)
            {
                // If the new total quantity exceeds stock, return an error.
                if (existingCartItem.Quantity + addToCartDTO.Quantity > product.StockQuantity)
                {
                    return new ApiResponse<CartResponseDTO>(400, $"Adding {addToCartDTO.Quantity} exceeds available stock.");
                }
                // Update the quantity and recalculate the total price for this cart item.
                existingCartItem.Quantity += addToCartDTO.Quantity;
                existingCartItem.TotalPrice = (existingCartItem.UnitPrice - existingCartItem.Discount) * existingCartItem.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
                // Mark the cart item as modified.
                _context.CartItems.Update(existingCartItem);
            }
            else
            {
                // Calculate discount per unit, if applicable.
                var discount = product.DiscountPercentage > 0 ? product.Price * product.DiscountPercentage / 100 : 0;
                // Create a new CartItem with the product and quantity details.
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = addToCartDTO.Quantity,
                    UnitPrice = product.Price,
                    Discount = discount,
                    TotalPrice = (product.Price - discount) * addToCartDTO.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                // Add the new cart item to the database context.
                _context.CartItems.Add(cartItem);
            }
            // Update the cart's last updated timestamp.
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            // Reload the cart with the latest details (including related CartItems and Products).
            cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cart.Id) ?? new Cart();
            // Map the cart entity to the DTO, which includes price calculations.
            var cartDTO = MapCartToDTO(cart);
            return new ApiResponse<CartResponseDTO>(200, cartDTO);
        }

        public async Task<ApiResponse<CartResponseDTO>> UpdateCartItemAsync(UpdateCartItemDTO updateCartItemDTO)
        {
            try
            {
                // Retrieve the active cart for the customer along with cart items and product details.
                var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == updateCartItemDTO.CustomerId && !c.IsCheckedOut);
                // Return 404 if no active cart is found.
                if (cart == null)
                {
                    return new ApiResponse<CartResponseDTO>(404, "Active cart not found.");
                }
                // Find the specific cart item that needs to be updated.
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == updateCartItemDTO.CartItemId);
                if (cartItem == null)
                {
                    return new ApiResponse<CartResponseDTO>(404, "Cart item not found.");
                }
                // Ensure the updated quantity does not exceed the available product stock.
                if (updateCartItemDTO.Quantity > cartItem.Product.StockQuantity)
                {
                    return new ApiResponse<CartResponseDTO>(400, $"Only {cartItem.Product.StockQuantity} units of {cartItem.Product.Name} are available.");
                }
                // Update the cart item's quantity and recalculate its total price.
                cartItem.Quantity = updateCartItemDTO.Quantity;
                cartItem.TotalPrice = (cartItem.UnitPrice - cartItem.Discount) * cartItem.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                // Mark the cart item as updated.
                _context.CartItems.Update(cartItem);
                // Update the cart's updated timestamp.
                cart.UpdatedAt = DateTime.UtcNow;
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();
                // Reload the updated cart with its items.
                cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id) ?? new Cart();
                // Map the updated cart to the DTO.
                var cartDTO = MapCartToDTO(cart);
                return new ApiResponse<CartResponseDTO>(200, cartDTO);
            }
            catch (Exception ex)
            {
                // Return error response if an exception occurs.
                return new ApiResponse<CartResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }
        // Removes a specific item from the customer's cart.
        public async Task<ApiResponse<CartResponseDTO>> RemoveCartItemAsync(RemoveCartItemDTO removeCartItemDTO)
        {
            try
            {
                // Retrieve the active cart along with its items and product details.
                var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == removeCartItemDTO.CustomerId && !c.IsCheckedOut);
                // Return 404 if no active cart is found.
                if (cart == null)
                {
                    return new ApiResponse<CartResponseDTO>(404, "Active cart not found.");
                }
                // Find the cart item to remove.
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == removeCartItemDTO.CartItemId);
                if (cartItem == null)
                {
                    return new ApiResponse<CartResponseDTO>(404, "Cart item not found.");
                }
                // Remove the cart item from the context.
                _context.CartItems.Remove(cartItem);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                // Reload the updated cart after removal.
                cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id) ?? new Cart();
                // Map the updated cart to the DTO.
                var cartDTO = MapCartToDTO(cart);
                return new ApiResponse<CartResponseDTO>(200, cartDTO);
            }
            catch (Exception ex)
            {
                // Return error response if an exception occurs.
                return new ApiResponse<CartResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }
        // Clears all items from the customer's active cart.
        public async Task<ApiResponse<ConfirmationResponseDTO>> ClearCartAsync(int customerId)
        {
            try
            {
                // Retrieve the active cart along with its items.
                var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);
                // Return 404 if no active cart is found.
                if (cart == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Active cart not found.");
                }
                // If there are any items in the cart, remove them.
                if (cart.CartItems.Any())
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                // Create a confirmation response DTO.
                var confirmation = new ConfirmationResponseDTO
                {
                    Message = "Cart has been cleared successfully."
                };
                return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
            }
            catch (Exception ex)
            {
                // Return error response if an exception occurs.
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        private CartResponseDTO MapCartToDTO(Cart cart)
        {
            var cartItemsDto = cart.CartItems?.Select(item => new CartItemResponseDTO
            {
                Id = item.Id,
                Discount = item.Discount,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList() ?? new List<CartItemResponseDTO>();

            decimal totalBasePrice = 0;
            decimal totalDiscount = 0;
            decimal totalAmount = 0;

            foreach (var item in cartItemsDto)
            {
                totalBasePrice += item.UnitPrice * item.Quantity;
                totalDiscount += item.Discount * item.Quantity;
                totalAmount += item.TotalPrice;
            }

            return new CartResponseDTO
            {
                Id = cart.Id,
                CustomerId = cart.CustomerId,
                IsCheckedOut = cart.IsCheckedOut,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                CartItems = cartItemsDto,
                TotalBasePrice = totalBasePrice,
                TotalDiscount = totalDiscount,
                TotalAmount = totalAmount
            };
        }
    }
}
