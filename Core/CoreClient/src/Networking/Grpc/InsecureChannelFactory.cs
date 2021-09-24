using System;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;

namespace Automata.Client.Networking.Grpc
{
    public class InsecureChannelFactory : IGrpcChannelFactory
    {
        private static readonly HttpClient DefaultHttpClient = new()
        {
            DefaultRequestVersion = new Version(2,0),
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
        };
        
        public static readonly SharedChannelFactory SharedInstance =
            new (new InsecureChannelFactory());

        private readonly HttpClient _httpClient;

        public InsecureChannelFactory(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? DefaultHttpClient;
        }

        public ChannelBase CreateChannel(GrpcAutomataServer server)
        {
            return GrpcChannel.ForAddress(
                server.ServerUri, new()
                {
                    DisposeHttpClient = false,
                    HttpClient = _httpClient,
                    Credentials = ChannelCredentials.Insecure
                });
        }
    }
}