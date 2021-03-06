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

using System.Collections.Generic;
using Fib.Net.Core.FileSystem;

namespace Fib.Net.Core.Api
{
    /** Builds to the Docker daemon. */
    public sealed class DockerDaemonImage
    {
        /**
         * Instantiate with the image reference to tag the built image with. This is the name that shows
         * up on the Docker daemon.
         *
         * @param imageReference the image reference
         * @return a new {@link DockerDaemonImage}
         */
        public static DockerDaemonImage Named(ImageReference imageReference)
        {
            return new DockerDaemonImage(imageReference);
        }

        /**
         * Instantiate with the image reference to tag the built image with. This is the name that shows
         * up on the Docker daemon.
         *
         * @param imageReference the image reference
         * @return a new {@link DockerDaemonImage}
         * @throws InvalidImageReferenceException if {@code imageReference} is not a valid image reference
         */
        public static DockerDaemonImage Named(string imageReference)
        {
            return Named(ImageReference.Parse(imageReference));
        }

        private readonly ImageReference imageReference;
        private SystemPath dockerExecutable;
        private IDictionary<string, string> dockerEnvironment = new Dictionary<string, string>();

        /** Instantiate with {@link #named}. */
        private DockerDaemonImage(ImageReference imageReference)
        {
            this.imageReference = imageReference;
        }

        /**
         * Sets the path to the {@code docker} CLI. This is {@code docker} by default.
         *
         * @param dockerExecutable the path to the {@code docker} CLI
         * @return this
         */
        public DockerDaemonImage SetDockerExecutable(SystemPath dockerExecutable)
        {
            this.dockerExecutable = dockerExecutable;
            return this;
        }

        /**
         * Sets the additional environment variables to use when running {@link #dockerExecutable docker}.
         *
         * @param dockerEnvironment additional environment variables
         * @return this
         */
        public DockerDaemonImage SetDockerEnvironment(IDictionary<string, string> dockerEnvironment)
        {
            this.dockerEnvironment = dockerEnvironment;
            return this;
        }

        public ImageReference GetImageReference()
        {
            return imageReference;
        }

        public Maybe<SystemPath> GetDockerExecutable()
        {
            return Maybe.OfNullable(dockerExecutable);
        }

        public IDictionary<string, string> GetDockerEnvironment()
        {
            return dockerEnvironment;
        }
        public void AddCredential(string username, string password)
        {
            this.Credential =Credential.From(username, password);
        }
        public Credential Credential { get; set; }
    }
}
