﻿using System;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Tcp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public class RabbitMQListeningAgent : DefaultBasicConsumer, IListeningAgent
    {
        private readonly ITransportLogger _logger;
        private readonly IModel _channel;
        private readonly IEnvelopeMapper _mapper;
        private EventingBasicConsumer _consumer;
        private IReceiverCallback _callback;

        public RabbitMQListeningAgent(Uri address, ITransportLogger logger, IModel channel, IEnvelopeMapper mapper) : base(channel)
        {
            _logger = logger;
            _channel = channel;
            _mapper = mapper;
            Address = address;
        }

        public void Dispose()
        {
            // Nothing, assuming the model is owned by the agent
        }

        public void Start(IReceiverCallback callback)
        {
            _callback = callback;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, byte[] body)
        {
            if (_callback == null) return;

            Envelope envelope = null;
            try
            {
                // TODO -- bypass the BasicDeliverEventArgs thing here
                // TODO -- add the RabbitMQ exchange & routingKey to the headers
                var props = new BasicDeliverEventArgs(consumerTag, deliveryTag, redelivered, exchange, routingKey,
                    properties, body);

                envelope = _mapper.ReadEnvelope(props);
            }
            catch (Exception e)
            {
                _logger.LogException(e, message:"Error trying to map an incoming RabbitMQ message to an Envelope");
                _channel.BasicAck(deliveryTag, false);

                return;
            }

            _callback.Received(Address, new [] {envelope}).ContinueWith(t =>
            {
                // TODO -- HARDEN THIS TOO?
                if (t.IsFaulted)
                {
                    _logger.LogException(t.Exception, envelope.Id, "Failure to receive an incoming message");
                    _channel.BasicNack(deliveryTag, false, true);
                }
                else
                {
                    _channel.BasicAck(deliveryTag, false);
                }
            });

        }

        public Uri Address { get; }
    }
}