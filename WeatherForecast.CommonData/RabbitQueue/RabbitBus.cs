using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WeatherForecast.CommonData.RabbitQueue
{
    /*
        https://medium.com/@jaroslawjaniszewski/asp-net-core-web-api-with-rabbitmq-for-beginners-2afe0b12c761
    
        RabbitBus class has two generic asynchronous methods SendAsync and ReceiveAsync 
        that responsible for communication with RabbitMq.
       
        Method SendAsync takes two parameters, queue name, and message which will be sent. 
        On the method body, is a declaration of a queue for a specific channel, 
        then the message is serialized on JSON format and publish to queue.
    
        Method ReceiveAsync takes two parameters, queue name, and delegate of a method which will be invoked 
        when received a message. On the body, is a declaration of a queue that is received messages. 
        Then it created an event that fired when a delivery arrives for the consumer. 
        When a message is received, its deserialize and passing as a parameter to delegate a method.
    */
    
    public class RabbitBus : IBus
    {
        private readonly IModel _channel;

        internal RabbitBus(IModel channel)
        {
            _channel = channel;
        }

        public async Task SendAsync<T>(string queue, T message)
        {
            await Task.Run(() =>
            {
                _channel.QueueDeclare(queue, true, false, false);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = false;

                var output = JsonConvert.SerializeObject(message);
                _channel.BasicPublish(string.Empty, queue, properties, Encoding.UTF8.GetBytes(output));
            });
        }

        public async Task ReceiveAsync<T>(string queue, Action<T> onMessage)
        {
            _channel.QueueDeclare(queue, true, false, false);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (s, e) =>
            {
                var jsonSpecified = Encoding.UTF8.GetString(e.Body.Span);
                var item = JsonConvert.DeserializeObject<T>(jsonSpecified);
                onMessage(item);
                await Task.Yield();
            };
            _channel.BasicConsume(queue, true, consumer);
            await Task.Yield();
        }
    }
}
