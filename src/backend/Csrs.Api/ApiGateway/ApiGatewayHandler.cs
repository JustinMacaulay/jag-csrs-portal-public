﻿using Csrs.Api.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Csrs.Api.ApiGateway
{
    public class ApiGatewayHandler : DelegatingHandler
    {

        public readonly ApiGatewayOptions _apiGatewayOptions;
        private readonly ILogger<ApiGatewayHandler> _logger;

        public ApiGatewayHandler(
            IOptions<ApiGatewayOptions> apiGatewayOptions, ILogger<ApiGatewayHandler> logger)
        {
            _apiGatewayOptions = apiGatewayOptions.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_apiGatewayOptions.BasePath)) return await base.SendAsync(request, cancellationToken);

            if (Uri.TryCreate(CombineUrls(_apiGatewayOptions.BasePath, request.RequestUri.PathAndQuery), UriKind.Absolute, out var path))
            {
                //request.Headers.Add("MSCRM.SuppressDuplicateDetection", "false");

                //this is to deal with Dynamics, when the method is POST and payload is empty, 
                //Dynamics still looking for content-type.
                if (request.Content == null
                    //&& request.Method == HttpMethod.Post
                    )
                    request.Content = new StringContent(string.Empty,
                                    Encoding.UTF8,
                                    "application/json");//CONTENT-TYPE header

                _logger.LogInformation("The _apiGatewayOptions.BasePath is ", path);
                request.RequestUri = path;
            }

            return await base.SendAsync(request, cancellationToken);
        }


        public static string CombineUrls(string baseUrl, string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(relativeUrl))
                return baseUrl;

            baseUrl = baseUrl.TrimEnd('/');
            relativeUrl = relativeUrl.TrimStart('/');

            return $"{baseUrl}/{relativeUrl}";
        }
    }
}
