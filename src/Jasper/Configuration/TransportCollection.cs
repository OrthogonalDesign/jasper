using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Messaging.Transports.Stub;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Configuration
{
    public class TransportCollection : IEnumerable<ITransport>, IEndpoints
    {
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();

        public TransportCollection()
        {
            Add(new TcpTransport());
            Add(new StubTransport());
            Add(new LocalTransport());
        }

        public ITransport TransportForScheme(string scheme)
        {
            return _transports[scheme];
        }

        public void Add(ITransport transport)
        {
            _transports.SmartAdd(transport.Protocol, transport);
        }

        public T Get<T>() where T : ITransport, new()
        {
            return _transports.Values.OfType<T>().FirstOrDefault();
        }

        public IEnumerator<ITransport> GetEnumerator()
        {
            return _transports.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Endpoint GetEndpoint(Uri uri)
        {
            var transport = TransportForScheme(uri.Scheme);
            if (transport == null)
            {
                throw new InvalidOperationException($"Unknown Transport scheme '{transport.Protocol}'");
            }

            return transport.TryGetEndpoint(uri);
        }

        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            var transport = TransportForScheme(uri.Scheme);
            if (transport == null)
            {
                throw new InvalidOperationException($"Unknown Transport scheme '{transport.Protocol}'");
            }

            return transport.GetOrCreateEndpoint(uri);
        }


        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        public IListenerConfiguration ListenForMessagesFrom(Uri uri)
        {
            if (_transports.TryGetValue(uri.Scheme, out var transport))
            {
                var settings = transport.ListenTo(uri);
                return new ListenerConfiguration(settings);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Unknown transport of type '{uri.Scheme}'");
            }
        }

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        public IListenerConfiguration ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(new Uri(uriString));
        }

        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     fast, but non-durable way
        /// </summary>
        /// <param name="port"></param>
        public IListenerConfiguration ListenAtPort(int port)
        {
            var settings = Get<TcpTransport>().ListenTo(TcpEndpoint.ToUri(port));
            return new ListenerConfiguration(settings);
        }

        public void Publish(Action<PublishingExpression> configuration)
        {
            var expression = new PublishingExpression(this);
            configuration(expression);
            expression.AttachSubscriptions();
        }

        public IPublishToExpression PublishAll()
        {
            var expression = new PublishingExpression(this)
            {
                AutoAddSubscriptions = true
            };

            expression.AddSubscriptionForAllMessages();
            return expression;
        }
    }
}
