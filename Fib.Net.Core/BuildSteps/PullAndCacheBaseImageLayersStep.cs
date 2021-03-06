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

using Fib.Net.Core.Async;
using Fib.Net.Core.Caching;
using Fib.Net.Core.Configuration;
using Fib.Net.Core.Events.Progress;
using Fib.Net.Core.Events.Time;
using Fib.Net.Core.Images;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using static Fib.Net.Core.BuildSteps.PullBaseImageStep;

namespace Fib.Net.Core.BuildSteps
{
    /** Pulls and caches the base image layers. */
    public class PullAndCacheBaseImageLayersStep : IAsyncStep<IReadOnlyList<ICachedLayer>>
    {
        private const string DESCRIPTION = "Setting up base image caching";

        private readonly BuildConfiguration buildConfiguration;
        private readonly ProgressEventDispatcher.Factory progressEventDispatcherFactory;

        private readonly PullBaseImageStep pullBaseImageStep;

        private readonly Task<IReadOnlyList<ICachedLayer>> listenableFuture;

        public PullAndCacheBaseImageLayersStep(
            BuildConfiguration buildConfiguration,
            ProgressEventDispatcher.Factory progressEventDispatcherFactory,
            PullBaseImageStep pullBaseImageStep)
        {
            this.buildConfiguration = buildConfiguration;
            this.progressEventDispatcherFactory = progressEventDispatcherFactory;
            this.pullBaseImageStep = pullBaseImageStep;

            listenableFuture = CallAsync();
        }

        public Task<IReadOnlyList<ICachedLayer>> GetFuture()
        {
            return listenableFuture;
        }

        public int Index { get; set; }

        public async Task<IReadOnlyList<ICachedLayer>> CallAsync()
        {
            BaseImageWithAuthorization pullBaseImageStepResult = await pullBaseImageStep.GetFuture().ConfigureAwait(false);
            ImmutableArray<ILayer> baseImageLayers = pullBaseImageStepResult.GetBaseImage().GetLayers();

            var checkIndex = 0;
            using (var progressEventDispatcher = progressEventDispatcherFactory.Create(
                    "checking base image layers", this.Index))
            using (var factory = progressEventDispatcher.NewChildProducer()("[child progress]checking base image layers" , baseImageLayers.Length))
            using (TimerEventDispatcher ignored =
                    new TimerEventDispatcher(buildConfiguration.GetEventHandlers(), DESCRIPTION))

            {
                List<Task<ICachedLayer>> pullAndCacheBaseImageLayerStepsBuilder = new List<Task<ICachedLayer>>();
                foreach (ILayer layer in baseImageLayers)
                {
                    checkIndex++;
                    pullAndCacheBaseImageLayerStepsBuilder.Add(
                        new PullAndCacheBaseImageLayerStep(
                            buildConfiguration,
                            factory.NewChildProducer(),
                            layer.GetBlobDescriptor().GetDigest(),
                            pullBaseImageStepResult.GetBaseImageAuthorization())
                        {
                            Index = checkIndex
                        }.GetFuture());
                }

                return await Task.WhenAll(pullAndCacheBaseImageLayerStepsBuilder).ConfigureAwait(false);
            }
        }
    }
}
