// Copyright 2018 Google LLC.
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

using System;
using System.Globalization;

namespace Fib.Net.Core
{
    /** Names of system properties defined/used by Fib. */
    public static class FibSystemProperties
    {
        private const int defaultTimeoutMills = 600000;
        public static readonly string HttpTimeout = "fib.httpTimeout";

        public static readonly string SendCredentialsOverHttp = "sendCredentialsOverHttp";

        private const string SERIALIZE = "fibSerialize";

        private const string DISABLE_USER_AGENT = "_JIB_DISABLE_USER_AGENT";

        private const string HTTP_PROXY = "_JIB_HTTP_PROXY";

        public static string GetHttpProxy()
        {
            var proxy = Environment.GetEnvironmentVariable(HTTP_PROXY);
            if (string.IsNullOrEmpty(proxy)) return null;
            return proxy;
        }

        /**
         * Gets the HTTP connection/read timeouts for registry interactions in milliseconds. This is
         * defined by the {@code fib.httpTimeout} system property. The default value is 20000 if the
         * system property is not set, and 0 indicates an infinite timeout.
         *
         * @return the HTTP connection/read timeouts for registry interactions in milliseconds
         */
        public static int GetHttpTimeout()
        {
            if (int.TryParse(Environment.GetEnvironmentVariable(HttpTimeout), out int timeoutMills))
            {
                return timeoutMills;
            }
            else
            {
                return defaultTimeoutMills;
            }
        }

        /**
         * Gets whether or not to serialize Fib's execution. This is defined by the {@code fibSerialize}
         * system property.
         *
         * @return {@code true} if Fib's execution should be serialized, {@code false} if not
         */
        public static bool IsSerializedExecutionEnabled()
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable(SERIALIZE), out bool serialize))
            {
                return serialize;
            }
            else
            {
                return false;
            }
        }

        /**
         * Gets whether or not to allow sending authentication information over insecure HTTP connections.
         * This is defined by the {@code sendCredentialsOverHttp} system property.
         *
         * @return {@code true} if authentication information is allowed to be sent over insecure
         *     connections, {@code false} if not
         */
        public static bool IsSendCredentialsOverHttpEnabled()
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable(SendCredentialsOverHttp), out bool sendCredentialsOverHttp))
            {
                return sendCredentialsOverHttp;
            }
            else
            {
                return false;
            }
        }

        /**
         * Gets whether or not to enable the User-Agent header. This is defined by the {@code
         * _JIB_DISABLE_USER_AGENT} system property.
         *
         * @return {@code true} if the User-Agent header is enabled, {@code false} if not
         */
        public static bool IsUserAgentEnabled()
        {
            return string.IsNullOrEmpty(Environment.GetEnvironmentVariable(DISABLE_USER_AGENT));
        }

        /**
         * Checks the {@code fib.httpTimeout} system property for invalid (non-integer or negative)
         * values.
         *
         * @throws NumberFormatException if invalid values
         */
        public static void CheckHttpTimeoutProperty()
        {
            CheckNumericSystemProperty(HttpTimeout, lowerBound: 0);
        }

        private static void CheckNumericSystemProperty(string property, int? lowerBound = null, int? upperBound = null)
        {
            string value = Environment.GetEnvironmentVariable(property);
            if (value == null)
            {
                return;
            }

            int parsed;
            try
            {
                parsed = int.Parse(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new FormatException(property + " must be an integer: " + value, ex);
            }
            if (lowerBound > parsed)
            {
                throw new FormatException(
                    property + " cannot be less than " + lowerBound + ": " + value);
            }
            else if (upperBound < parsed)
            {
                throw new FormatException(
                    property + " cannot be greater than " + upperBound + ": " + value);
            }
        }
    }
}
