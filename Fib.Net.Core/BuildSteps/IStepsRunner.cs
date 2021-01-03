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

using Fib.Net.Core.Docker;
using Fib.Net.Core.FileSystem;
using System.Threading.Tasks;

namespace Fib.Net.Core.BuildSteps
{
    public interface IStepsRunner
    {
        StepsRunner AuthenticatePush(int index);
        StepsRunner BuildAndCacheApplicationLayers(int index);
        StepsRunner BuildImage(int index);
        StepsRunner LoadDocker(DockerClient dockerClient, int index);
        StepsRunner PullAndCacheBaseImageLayers(int index);
        StepsRunner PullBaseImage(int index);
        StepsRunner PushApplicationLayers(int index);
        StepsRunner PushBaseImageLayers(int index);
        StepsRunner PushContainerConfiguration(int index);
        StepsRunner PushImage(int index);
        StepsRunner RetrieveTargetRegistryCredentials(int index);
        Task<IBuildResult> RunAsync();
        StepsRunner WriteTarFile(SystemPath outputPath, int index);
    }
}