// <copyright file="FileReceiverServiceCollectionExtensions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
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
// </copyright>

using System.IO.Abstractions;
using Google.Protobuf;

namespace LogForwarder.Receiver.File;

/// <summary>
/// Extension methods for configuring DI for file receiver.
/// </summary>
public static class FileReceiverServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures a file receiver to the DI container.
    /// </summary>
    /// <param name="services">the service collection.</param>
    /// <param name="optionsBuilder">the options builder for this type of export service request.</param>
    /// <typeparam name="T">the service request.</typeparam>
    /// <returns>the service collection with the added file receiever.</returns>
    public static IServiceCollection AddFileReceiver<T>(this IServiceCollection services, Action<FileReceiverWorkerOptions<T>> optionsBuilder)
        where T : class, IMessage<T>, new()
    {
        services.AddOptions<FileReceiverWorkerOptions<T>>().Configure(optionsBuilder);

        services.AddTransient<IFileSystem, FileSystem>();
        services.AddHostedService<FileReceiverWorkerBase<T>>();
        return services;
    }
}
