// This software is part of the Autofac IoC container
// Copyright © 2017 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using Castle.Core.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using System;
using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;


[assembly: InternalsVisibleTo("Autofac.Integration.ServiceFabric.Test, PublicKey=00240000048000009400000006020000002400005253413100040000010001008728425885ef385e049261b18878327dfaaf0d666dea3bd2b0e4f18b33929ad4e5fbc9087e7eda3c1291d2de579206d9b4292456abffbe8be6c7060b36da0c33b883e3878eaf7c89fddf29e6e27d24588e81e86f3a22dd7b1a296b5f06fbfb500bbd7410faa7213ef4e2ce7622aefc03169b0324bcd30ccfe9ac8204e4960be6")]
[assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]

[assembly: SuppressMessage("Microsoft.Design", "CA1020", Scope = "namespace", Target = "Autofac.Integration.ServiceFabric")]



namespace Autofac.Integration.ServiceFabric
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class ActorFactoryRegistration : IActorFactoryRegistration
    {
        public void RegisterActorFactory<TActor>(
            ILifetimeScope container,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null) where TActor : ActorBase
        {
            ActorRuntime.RegisterActorAsync<TActor>((context, actorTypeInfo) =>
            {
                return new ActorService(context, actorTypeInfo, (actorService, actorId) =>
                {
                    var lifetimeScope = container.BeginLifetimeScope(builder =>
                    {
                        builder.RegisterInstance(context)
                            .As<StatefulServiceContext>()
                            .As<ServiceContext>();
                        builder.RegisterInstance(actorService)
                            .As<ActorService>();
                        builder.RegisterInstance(actorId)
                            .As<ActorId>();
                    });
                    var actor = lifetimeScope.Resolve<TActor>();
                    return actor;
                }, stateManagerFactory, stateProvider, settings);
            }).GetAwaiter().GetResult();
        }
    }
}
