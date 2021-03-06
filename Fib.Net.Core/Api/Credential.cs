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

namespace Fib.Net.Core.Api
{
    /** Holds credentials (username and password). */
    public sealed class Credential
    {
        // If the username is set to <token>, the secret would be a refresh token.
        // https://github.com/docker/cli/blob/master/docs/reference/commandline/login.md#credential-helper-protocol
        private const string OAUTH2_TOKEN_USER_NAME = "<token>";

        /**
         * Gets a {@link Credential} configured with a username and password.
         *
         * @param username the username
         * @param password the password
         * @return a new {@link Credential}
         */
        public static Credential From(string username, string password)
        {
            return new Credential(username, password);
        }


        public string UserName { get; set; }
        public string Password { get; set; }

        public Credential()
        {

        }
        public Credential(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }

        /**
         * Gets the username.
         *
         * @return the username
         */
        public string GetUsername()
        {
            return UserName;
        }

        /**
         * Gets the password.
         *
         * @return the password
         */
        public string GetPassword()
        {
            return Password;
        }

        /**
         * Check whether this credential is an OAuth 2.0 refresh token.
         *
         * @return true if this credential is an OAuth 2.0 refresh token.
         */
        public bool IsOAuth2RefreshToken()
        {
            return OAUTH2_TOKEN_USER_NAME == UserName;
        }

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (!(other is Credential otherCredential))
            {
                return false;
            }
            return UserName == otherCredential.UserName
                && Password == otherCredential.Password;
        }

        public override int GetHashCode()
        {
            return Objects.Hash(UserName, Password);
        }

        public override string ToString()
        {
            return UserName + ":" + Password;
        }
    }
}
