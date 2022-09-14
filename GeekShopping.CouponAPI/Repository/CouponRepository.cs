using AutoMapper;
using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CouponAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly MySQLContext _mySQLContext;
        private readonly IMapper _mapper;

        public CouponRepository(MySQLContext mySQLContext, IMapper mapper)
        {
            _mySQLContext = mySQLContext;
            _mapper = mapper;
        }

        public async Task<CouponVO> GetCouponByCouponCode(string couponCode)
        {
            var coupon = await _mySQLContext.Coupons
                .FirstOrDefaultAsync(c => c.CouponCode == couponCode);

            return _mapper.Map<CouponVO>(coupon);
        }
    }
}
