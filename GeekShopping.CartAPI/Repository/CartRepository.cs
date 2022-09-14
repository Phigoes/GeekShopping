using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;
using GeekShopping.CartAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly MySQLContext _mySQLContext;
        private readonly IMapper _mapper;

        public CartRepository(MySQLContext mySQLContext, IMapper mapper)
        {
            _mySQLContext = mySQLContext;
            _mapper = mapper;
        }

        public async Task<CartVO> FindCartByUserId(string userId)
        {
            var cart = new Cart()
            {
                CartHeader = await _mySQLContext.CartHeaders
                    .FirstOrDefaultAsync(c => c.UserId == userId) ?? new CartHeader()
            };

            cart.CartDetails = _mySQLContext.CartDetails
                .Where(c => c.CartHeaderId == cart.CartHeader.Id)
                .Include(c => c.Product);

            return _mapper.Map<CartVO>(cart);
        }

        public async Task<CartVO> SaveOrUpdateCart(CartVO cartVO)
        {
            var cart = _mapper.Map<Cart>(cartVO);
            var product = await _mySQLContext.Products.FirstOrDefaultAsync(p => 
                p.Id == cartVO.CartDetails.FirstOrDefault().ProductId);

            if (product == null)
            {
                _mySQLContext.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _mySQLContext.SaveChangesAsync();
            }

            var cartHeader = await _mySQLContext.CartHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == cart.CartHeader.UserId);

            if (cartHeader == null)
            {
                _mySQLContext.CartHeaders.Add(cart.CartHeader);
                await _mySQLContext.SaveChangesAsync();

                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.Id;
                cart.CartDetails.FirstOrDefault().Product = null;
                _mySQLContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _mySQLContext.SaveChangesAsync();
            }
            else
            {
                var cartDetail = await _mySQLContext.CartDetails
                    .AsNoTracking().FirstOrDefaultAsync(p =>
                        p.ProductId == cart.CartDetails.FirstOrDefault().ProductId &&
                        p.CartHeaderId == cartVO.CartHeader.Id);

                if (cartDetail == null)
                {
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeader.Id;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _mySQLContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _mySQLContext.SaveChangesAsync();
                }
                else
                {
                    cart.CartDetails.FirstOrDefault().Product = null;
                    cart.CartDetails.FirstOrDefault().Count += cartDetail.Count;
                    cart.CartDetails.FirstOrDefault().Id = cartDetail.Id;
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetail.CartHeaderId;
                    _mySQLContext.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                    await _mySQLContext.SaveChangesAsync();
                }
            }

            return _mapper.Map<CartVO>(cart);
        }

        public async Task<bool> ApplyCoupon(string userId, string couponCode)
        {
            var cartHeader = await _mySQLContext.CartHeaders
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cartHeader != null)
            {
                cartHeader.CouponCode = couponCode;
                _mySQLContext.Update(cartHeader);

                await _mySQLContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            var cartHeader = await _mySQLContext.CartHeaders
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cartHeader != null)
            {
                cartHeader.CouponCode = "";
                _mySQLContext.Update(cartHeader);

                await _mySQLContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> RemoveFromCart(long cartDetailsId)
        {
            try
            {
                CartDetail cartDetail = await _mySQLContext.CartDetails
                    .FirstOrDefaultAsync(c => c.Id == cartDetailsId);

                int total = _mySQLContext.CartDetails.Where(c => c.CartHeaderId == cartDetail.CartHeaderId).Count();

                _mySQLContext.CartDetails.Remove(cartDetail);

                if (total == 1)
                {
                    var cartHeaderToRemove = await _mySQLContext.CartHeaders
                        .FirstOrDefaultAsync(c => c.Id == cartDetail.CartHeaderId);
                    _mySQLContext.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _mySQLContext.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeader = await _mySQLContext.CartHeaders
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cartHeader != null)
            {
                _mySQLContext.CartDetails
                    .RemoveRange(_mySQLContext.CartDetails.Where(c => c.CartHeaderId == cartHeader.Id));

                _mySQLContext.CartHeaders.Remove(cartHeader);

                await _mySQLContext.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
