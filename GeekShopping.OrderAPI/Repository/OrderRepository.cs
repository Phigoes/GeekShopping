using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<MySQLContext> _mySQLContext;

        public OrderRepository(DbContextOptions<MySQLContext> mySQLContext)
        {
            _mySQLContext = mySQLContext;
        }

        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            if (orderHeader == null) return false;

            await using var _db = new MySQLContext(_mySQLContext);
            _db.OrderHeaders.Add(orderHeader);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task UpdateOrderPaymentStatus(long orderHeaderId, bool status)
        {
            await using var _db = new MySQLContext(_mySQLContext);
            var orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(o => o.Id == orderHeaderId);

            if (orderHeader != null)
            {
                orderHeader.PaymentStatus = status;
                await _db.SaveChangesAsync();
            }
        }
    }
}
