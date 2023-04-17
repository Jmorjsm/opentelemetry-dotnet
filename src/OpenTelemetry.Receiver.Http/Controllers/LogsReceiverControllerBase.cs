using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace LogForwarder.Receiver.Http.Controllers;

/// <summary>
/// Implementation for the api controller that receives export logs service requests.
/// </summary>
[ApiController]
[Route("otlp/v1/logs")]
public class LogsReceiverControllerBase: OtlpReceiverControllerBase<ExportLogsServiceRequest>
{
    public LogsReceiverControllerBase(ILogger<OtlpReceiverControllerBase<ExportLogsServiceRequest>> logger, BaseExporter<ExportLogsServiceRequest> exporter)
        : base(logger, exporter)
    {
    }

    protected override MessageParser<ExportLogsServiceRequest> ExportRequestParser => ExportLogsServiceRequest.Parser;
}
