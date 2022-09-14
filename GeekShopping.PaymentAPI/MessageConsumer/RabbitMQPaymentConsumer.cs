﻿using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender;
using GeekShopping.PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMQMessageSender _rabbitMQMessageSender;
        private readonly IProcessPayment _processPayment;

        public RabbitMQPaymentConsumer(IProcessPayment processPayment, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _processPayment = processPayment;
            _rabbitMQMessageSender = rabbitMQMessageSender;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "orderpaymentprocessqueue", false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, @event) =>
            {
                var content = Encoding.UTF8.GetString(@event.Body.ToArray());
                PaymentMessage paymentMessage = JsonSerializer.Deserialize<PaymentMessage>(content);
                ProcessPayment(paymentMessage).GetAwaiter().GetResult();
                _channel.BasicAck(@event.DeliveryTag, false);
            };
            _channel.BasicConsume("orderpaymentprocessqueue", false, consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessPayment(PaymentMessage checkoutHeaderVO)
        {
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage paymentResultMessage = new UpdatePaymentResultMessage()
            {
                Status = result,
                OrderId = checkoutHeaderVO.OrderId,
                Email = checkoutHeaderVO.Email
            };

            try
            {
                _rabbitMQMessageSender.SendMessage(paymentResultMessage);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
