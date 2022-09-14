using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CouponAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;

        public CouponController(ICouponRepository productRepository)
        {
            _couponRepository = productRepository ?? throw new ArgumentNullException(nameof(_couponRepository));
        }


        [Authorize]
        [HttpGet("{couponCode}")]
        public async Task<ActionResult<CouponVO>> GetCouponByCouponCode(string couponCode)
        {
            var coupon = await _couponRepository.GetCouponByCouponCode(couponCode);
            if (coupon == null) return NotFound();

            return Ok(coupon);
        }
    }
}