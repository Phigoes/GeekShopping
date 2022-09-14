using GeekShopping.Email.Messages;
using GeekShopping.Email.Model;
using GeekShopping.Email.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DbContextOptions<MySQLContext> _mySQLContext;

        public EmailRepository(DbContextOptions<MySQLContext> mySQLContext)
        {
            _mySQLContext = mySQLContext;
        }

        public async Task LogEmail(UpdatePaymentResultMessage paymentResultMessage)
        {
            EmailLog email = new EmailLog()
            {
                Email = paymentResultMessage.Email,
                SentDate = DateTime.Now,
                Log = $"Order - {paymentResultMessage.OrderId} has been created successfully!"
            };

            await using var _db = new MySQLContext(_mySQLContext);
            _db.Emails.Add(email);
            await _db.SaveChangesAsync();
        }
    }
}
