using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("Convey.Discovery.Consul")]
[assembly:InternalsVisibleTo("Convey.LoadBalancing.Fabio")]
namespace Convey.HTTP
{
    public static class Extensions
    {
        private const string SectionName = "httpClient";
        private const string RegistryName = "http.client";

        public static IConveyBuilder AddHttpClient(this IConveyBuilder builder, string clientName = "convey",
            string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }
            
            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentException("HTTP client name cannot be empty.", nameof(clientName));
            }

            var options = builder.GetOptions<HttpClientOptions>(sectionName);
            builder.Services.AddSingleton(options);
            builder.Services.AddHttpClient<IHttpClient, ConveyHttpClient>(clientName);

            return builder;
        }


        [Description("This is a hack related to HttpClient issue: https://github.com/aspnet/AspNetCore/issues/13346")]
        internal static void RemoveHttpClient(this IConveyBuilder builder)
        {
            var registryType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .SingleOrDefault(t => t.Name == "HttpClientMappingRegistry");
            var registry = builder.Services.SingleOrDefault(s => s.ServiceType == registryType)?.ImplementationInstance;
            var registrations = registry?.GetType().GetProperty("TypedClientRegistrations");
            var clientRegistrations = registrations?.GetValue(registry) as IDictionary<Type, string>;
            clientRegistrations?.Remove(typeof(IHttpClient));
        }
    }
}