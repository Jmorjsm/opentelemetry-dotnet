// <copyright file="OtlpReceiverControllerBase.cs" company="OpenTelemetry Authors">
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

using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;

namespace LogForwarder.Receiver.Http.Controllers;

/// <summary>
/// Base class for signal receiver api controllers.
/// </summary>
/// <typeparam name="T">the type of signal to receive.</typeparam>
public abstract class OtlpReceiverControllerBase<T> : ControllerBase
    where T : class, IMessage<T>
{
    private readonly ILogger<OtlpReceiverControllerBase<T>> logger;
    private readonly BaseExporter<T> exporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="OtlpReceiverControllerBase{T}"/> class.
    /// </summary>
    /// <param name="logger">the logger.</param>
    /// <param name="exporter">the exporter to use.</param>
    public OtlpReceiverControllerBase(ILogger<OtlpReceiverControllerBase<T>> logger, BaseExporter<T> exporter)
    {
        this.logger = logger;
        this.exporter = exporter;
    }

    /// <summary>
    /// The message parser for this signal export request type.
    /// </summary>
    protected abstract MessageParser<T> ExportRequestParser { get; }

    /// <summary>
    /// Http POST handler.
    /// </summary>
    /// <returns>the result of receiving the signal.</returns>
    [HttpPost(Name = "Logs")]
    public ActionResult Post()
    {
        Stream stream = this.Request.BodyReader.AsStream();

        T exportLogsServiceRequest = this.ExportRequestParser.ParseFrom(stream);
        ExportResult result = this.exporter.Export(new Batch<T>(new[] { exportLogsServiceRequest }, 1));
        switch (result)
        {
            case ExportResult.Success:
                break;
            case ExportResult.Failure:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new OkResult();
    }
}
