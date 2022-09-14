using GeekShopping.CartAPI.Repository;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.RabbitMQSender;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMQMessageSender _rabbitMQMessageSender;

        public RabbitMQCheckoutConsumer(OrderRepository orderRepository, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _orderRepository = orderRepository;
            _rabbitMQMessageSender = rabbitMQMessageSender;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "checkoutqueue", false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, @event) =>
            {
                var content = Encoding.UTF8.GetString(@event.Body.ToArray());
                CheckoutHeaderVO checkoutHeaderVO = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);
                ProcessOrder(checkoutHeaderVO).GetAwaiter().GetResult();
                _channel.BasicAck(@event.DeliveryTag, false);
            };
            _channel.BasicConsume("checkoutqueue", false, consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessOrder(CheckoutHeaderVO checkoutHeaderVO)
        {
            OrderHeader orderHeader = new OrderHeader
            {
                UserId = checkoutHeaderVO.UserId,
                FirstName = checkoutHeaderVO.FirstName,
                LastName = checkoutHeaderVO.LastName,
                OrderDetails = new List<OrderDetail>(),
                CardNumber = checkoutHeaderVO.CardNumber,
                CouponCode = checkoutHeaderVO.CouponCode,
                CVV = checkoutHeaderVO.CVV,
                DiscountAmount = checkoutHeaderVO.DiscountAmount,
                Email = checkoutHeaderVO.Email,
                ExpiryMonthYear = checkoutHeaderVO.ExpiryMonthYear,
                OrderTime = DateTime.Now,
                PurchaseAmount = checkoutHeaderVO.PurchaseAmount,
                PaymentStatus = false,
                Phone = checkoutHeaderVO.Phone,
                OrderDate = checkoutHeaderVO.DateTime
            };

            foreach (var details in checkoutHeaderVO.CartDetailsVO)
            {
                OrderDetail detail = new OrderDetail
                {
                    ProductId = details.ProductId,
                    ProductName = details.Product.Name,
                    Price = details.Product.Price,
                    Count = details.Count
                };

                orderHeader.CartTotalItems += detail.Count;
                orderHeader.OrderDetails.Add(detail);
            }

            await _orderRepository.AddOrder(orderHeader);

            PaymentVO paymentVO = new PaymentVO
            {
                Name = $"{orderHeader.FirstName} {orderHeader.LastName}",
                CardNumber = checkoutHeaderVO.CardNumber,
                CVV= checkoutHeaderVO.CVV,
                ExpiryMonthYear = checkoutHeaderVO.ExpiryMonthYear,
                OrderId = orderHeader.Id,
                PurchaseAmount = orderHeader.PurchaseAmount,
                Email = orderHeader.Email
            };

            try
            {
                _rabbitMQMessageSender.SendMessage(paymentVO, "orderpaymentprocessqueue");
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
