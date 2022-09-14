using GeekShopping.Web.Models;

namespace GeekShopping.Web.Services.IServices
{
    public interface ICartService
    {
        Task<CartViewModel> FindCartByUserId(string userId, string token);
        Task<CartViewModel> AddItemToCart(CartViewModel cartViewModel, string token);
        Task<CartViewModel> UpdateCart(CartViewModel cartViewModel, string token);
        Task<bool> RemoveFromCart(long cartId, string token);
        Task<bool> ApplyCoupon(CartViewModel cartViewModel, string token);
        Task<bool> RemoveCoupon(string userId, string token);
        Task<bool> ClearCart(string userId, string token);
        Task<object> Checkout(CartHeaderViewModel cartHeaderViewModel, string token);
    }
}
