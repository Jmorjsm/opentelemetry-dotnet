using System.IO.Abstractions;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace LogForwarder.Receiver.File;

/// <inheritdoc />
public class LogFileReceiverWorker : FileReceiverWorkerBase<ExportLogsServiceRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogFileReceiverWorker"/> class.
    /// </summary>
    /// <param name="logger">the logger.</param>
    /// <param name="options">the options.</param>
    /// <param name="fileSystem">the file system.</param>
    /// <param name="exporter">the exporter.</param>
    public LogFileReceiverWorker(ILogger<LogFileReceiverWorker> logger, IOptions<FileReceiverWorkerOptions<ExportLogsServiceRequest>> options, IFileSystem fileSystem, BaseExporter<ExportLogsServiceRequest> exporter)
        : base(logger, options, fileSystem, exporter)
    {
    }

    /// <inheritdoc />
    protected override MessageParser<ExportLogsServiceRequest> ExportRequestParser => ExportLogsServiceRequest.Parser;
}
