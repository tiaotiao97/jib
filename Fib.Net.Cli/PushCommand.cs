// Copyright 2020 James Przybylinski
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CommandLine;
using Fib.Net.Core.Api;

namespace Fib.Net.Cli
{
    [Verb("push", HelpText = "Build an image and push to a remote registry.")]
    public class PushCommand : Command
    {
        protected override IContainerizer CreateContainerizer(FibCliConfiguration configuration)
        {
            var toImage = RegistryImage.Named(configuration.GetTargetImageReference());
            if(configuration.TargetImageCredential!=null && !string.IsNullOrEmpty(configuration.TargetImageCredential.UserName)
                && !string.IsNullOrEmpty(configuration.TargetImageCredential.Password))
            {
                toImage.AddCredential(configuration.TargetImageCredential.UserName, configuration.TargetImageCredential.Password);
            }
            return Containerizer.To(toImage);
        }
    }
}