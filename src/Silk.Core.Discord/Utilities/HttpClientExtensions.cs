﻿using System.Net.Http;
using Silk.Shared.Constants;

namespace Silk.Core.Discord.Utilities
{
    public static class HttpClientExtensions
    {
        public static HttpClient CreateSilkClient(this IHttpClientFactory httpClientFactory)
        {
            return httpClientFactory.CreateClient(StringConstants.HttpClientName);
        }
    }
}