// Copyright 2017 Google LLC.
// Copyright 2020 James Przybylinski
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// NOTICE: This file was modified by James Przybylinski to be C#.

using Fib.Net.Core.Configuration;
using Fib.Net.Core.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fib.Net.Core.Http
{
    /**
     * Sends an HTTP {@link Request} and stores the {@link Response}. Clients should not send more than
     * one request.
     *
     * <p>Example usage:
     *
     * <pre>{@code
     * try (Connection connection = new Connection(url)) {
     *   Response response = connection.get(request);
     *   // ... process the response
     * }
     * }</pre>
     */
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed")]
    public sealed class Connection : IDisposable, IConnection
    {
        private readonly IEventHandlers _eventHandlers;

        /**
         * Returns a factory for {@link Connection}.
         *
         * @return {@link Connection} factory, a function that generates a {@link Connection} to a Uri
         */
        public static Func<Uri, Connection> GetConnectionFactory(IEventHandlers eventHandlers = null)
        {
            /*
             * Do not use {@link NetHttpTransport}. It does not process response errors properly. A new
             * {@link ApacheHttpTransport} needs to be created for each connection because otherwise HTTP
             * connection persistence causes the connection to throw {@link NoHttpResponseException}.
             *
             * @see <a
             *     href="https://github.com/google/google-http-java-client/issues/39">https://github.com/google/google-http-java-client/issues/39</a>
             */
            return url => new Connection(url, eventHandlers);
        }

        /**
         * Returns a factory for {@link Connection} that does not verify TLS peer verification.
         *
         * @throws GeneralSecurityException if unable to turn off TLS peer verification
         * @return {@link Connection} factory, a function that generates a {@link Connection} to a Uri
         */
        public static Func<Uri, Connection> GetInsecureConnectionFactory()
        {
            // Do not use {@link NetHttpTransport}. See {@link getConnectionFactory} for details.

            return url => new Connection(url, true);
        }

        /** The Uri to send the request to. */
        private readonly HttpClient client;

        private static readonly ConcurrentDictionary<Uri, HttpClient> clients = new ConcurrentDictionary<Uri, HttpClient>();
        private static readonly ConcurrentDictionary<Uri, HttpClient> insecureClients = new ConcurrentDictionary<Uri, HttpClient>();

        private static readonly HttpMessageHandler insecureHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        /**
         * Make sure to wrap with a try-with-resource to ensure that the connection is closed after usage.
         *
         * @param url the url to send the request to
         */
        public Connection(Uri url, IEventHandlers eventHandlers = null) : this(url, false, eventHandlers) { }

        private string proxyLog="";
        
        public Connection(Uri url, bool insecure, IEventHandlers eventHandlers = null)
        {
            _eventHandlers = eventHandlers;
            var proxy = FibSystemProperties.GetHttpProxy();
            WebProxy proxy1 = null;
            if (!string.IsNullOrEmpty(proxy))
            {
                proxyLog = $"use proxy:{proxy}";
                if (proxy.Contains("@_@"))
                {
                    //127.0.0.1:8080@_@username&pass
                    var arr = proxy.Split(new string[] { "@_@" }, StringSplitOptions.None);
                    proxy1 = new WebProxy
                    {
                        Address = new Uri($"http://{arr}"),
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false,

                        // *** These creds are given to the proxy server, not the web server ***
                        Credentials = new NetworkCredential(
                            userName: arr[1].Split('&')[0],
                            password: arr[1].Split('&')[1])
                    };
                }
                else
                {
                    proxy1 = new WebProxy
                    {
                        Address = new Uri($"http://{proxy}"),
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false,
                    };
                }
            }



            if (insecure)
            {
                if (proxy1 != null)
                {
                    client = insecureClients.GetOrAdd(url, _ =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            Proxy = proxy1, ServerCertificateCustomValidationCallback = (_____, __, ___, ____) => true
                        };
                        var c = new HttpClient(httpClientHandler)
                        {
                            BaseAddress = url,
                            Timeout = TimeSpan.FromMilliseconds(FibSystemProperties.GetHttpTimeout()),
                        };
                        return c;

                    });
                }
                else
                {
                    client = insecureClients.GetOrAdd(url, _ => new HttpClient(insecureHandler)
                    {
                        BaseAddress = url,
                        Timeout = TimeSpan.FromMilliseconds(FibSystemProperties.GetHttpTimeout()),
                    });
                }

            }
            else
            {
                if (proxy1 != null)
                {
                    client = clients.GetOrAdd(url, _ =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            Proxy = proxy1,
                        };
                        var c = new HttpClient(httpClientHandler)
                        {
                            BaseAddress = url,
                            Timeout = TimeSpan.FromMilliseconds(FibSystemProperties.GetHttpTimeout()),
                        };
                        return c;

                    });
                }
                else
                {
                    client = clients.GetOrAdd(url, _ => new HttpClient
                    {
                        BaseAddress = url,
                        Timeout = TimeSpan.FromMilliseconds(FibSystemProperties.GetHttpTimeout()),
                    });
                }
            }
        }

        public void Dispose()
        {
        }

        /**
         * Sends the request.
         *
         * @param httpMethod the HTTP request method
         * @param request the request to send
         * @return the response to the sent request
         * @throws IOException if building the HTTP request fails.
         */
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            try
            {
                return await client.SendAsync(request).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _eventHandlers?.Dispatch(LogEvent.Info("Exception retrieving " + request.RequestUri + "->ex:"+e.Message));
                Debug.WriteLine("Exception retrieving " + request.RequestUri);
                throw;
            }
        }

        public string Proxy()
        {
            return this.proxyLog;
        }
    }
}
