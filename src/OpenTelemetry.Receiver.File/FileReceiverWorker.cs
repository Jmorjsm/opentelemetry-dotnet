using System.Buffers.Binary;
using System.IO.Abstractions;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using OpenTelemetry;


namespace LogForwarder.Receiver.File;

/// <summary>
/// Worker receiving files containing signal export requests of type <see cref="T"/> from a file.
/// </summary>
/// <typeparam name="T">the type of signal this class receives.</typeparam>
public class FileReceiverWorker<T> : BackgroundService
    where T : class, IMessage<T>, new()
{
    private static MessageParser<T> exportRequestParser;
    private readonly ILogger<FileReceiverWorker<T>> logger;
    private readonly IFileSystem fileSystem;
    private readonly string path;
    private readonly FileFormatType formatType;
    private readonly FileCompressionType compressionType;
    private readonly BaseExporter<T> exporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileReceiverWorker{T}"/> class, receiving files containing signal export requests of type <see cref="T"/>.
    /// </summary>
    /// <param name="logger">the logger.</param>
    /// <param name="options">the file receiver options.</param>
    /// <param name="fileSystem">the file system.</param>
    /// <param name="exporter">the exporter to use.</param>
    public FileReceiverWorker(ILogger<FileReceiverWorker<T>> logger, IOptions<FileReceiverWorkerOptions> options, IFileSystem fileSystem, BaseExporter<T> exporter)
    {
        this.logger = logger;
        this.fileSystem = fileSystem;
        this.exporter = exporter;
        this.path = options.Value.Path;
        this.formatType = options.Value.FormatType;
        this.compressionType = options.Value.CompressionType;
    }

    private static Batch<T> ParseProto(FileSystemStream fileStream)
    {
        // TODO: Implement based on https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/fileexporter#file-format
        // let overallRequest = new ExportLogsServiceRequest
        // while (stream.read){
        // let size = read 4 byte uint32,
        // let currentRequest = parseFrom(read(size));
        // overallRequest.add(currentRequest)
        // }
        T request = new T();

        while (ReadNextProtoExportRequest(fileStream, out T? nextRequest))
        {
            request.MergeFrom(nextRequest);
        }

        return new Batch<T>(new[] { request }, 1);
    }

    private static bool ReadNextProtoExportRequest(
        FileSystemStream fileStream,
        out T exportLogsServiceRequest)
    {
        byte[] requestBuffer;
        try
        {
            byte[] sizeBuffer = new byte[4];
            fileStream.ReadExactly(sizeBuffer);

            // Size is written big-endian, uint32 decodes little-endian
            uint size = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(sizeBuffer));
            requestBuffer = new byte[size];
            fileStream.ReadExactly(requestBuffer);
        }
        catch
        {
            exportLogsServiceRequest = null;
            return false;
        }

        exportLogsServiceRequest = exportRequestParser.ParseFrom(requestBuffer);
        return true;
    }


    /// <summary>
    /// Start processing files.
    /// </summary>
    /// <param name="stoppingToken">the cancellation token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("File Receiver Worker started at: {Time}", DateTimeOffset.Now);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!this.TryGetNextFileToProcess(out IFileInfo? logFileToProcess) || logFileToProcess == null)
            {
                this.logger.LogInformation("No files to process at: {Time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
                continue;
            }

            this.logger.LogInformation("Work to do at: {Time}, processing {FileName}", DateTimeOffset.Now, logFileToProcess.Name);
            this.ProcessFile(logFileToProcess);
        }
    }

    private void ProcessFile(IFileInfo fileToProcess)
    {
        using (FileSystemStream fileStream = fileToProcess.OpenRead())
        {
            Batch<T> exportServiceRequest = this.ParseFile(fileStream);
            ExportResult result = this.exporter.Export(exportServiceRequest);

            switch (result)
            {
                case ExportResult.Success:
                    break;
                case ExportResult.Failure:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        fileToProcess.Delete();
    }

    private Batch<T> ParseFile(FileSystemStream fileStream)
    {
        switch (this.compressionType)
        {
            case FileCompressionType.Zstd:
                throw new NotImplementedException();
            case FileCompressionType.None:
                break;
        }

        switch (this.formatType)
        {
            case FileFormatType.Json:
                throw new NotImplementedException();
            case FileFormatType.Proto:
                return ParseProto(fileStream);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool TryGetNextFileToProcess(out IFileInfo? logFileToProcess)
    {
        string? dirName = this.fileSystem.Path.GetDirectoryName(this.path);
        if (dirName == null)
        {
            throw new ArgumentNullException(nameof(dirName), "Provided path does not include a directory.");
        }

        string fileName = Path.GetFileName(this.path);
        dirName = this.fileSystem.Path.GetFullPath(dirName);
        if (!this.fileSystem.Directory.Exists(dirName))
        {
            this.logger.LogWarning("Directory \"{DirName}\" doesn't exist", dirName);
            logFileToProcess = null;
            return false;
        }

        IEnumerable<string> files = this.fileSystem.Directory.EnumerateFiles(dirName, $"{fileName}*");

        // Ignore the exact match, sort by file name asc as it's a sortable date and we want oldest first.
        List<string> filesList = files.Where(f =>
            !string.Equals(Path.GetFileName(f), fileName, StringComparison.InvariantCultureIgnoreCase))
            .Order()
            .ToList();

        this.logger.LogInformation("Identified {FileCount} files in directory {DirectoryName} matching file name {FileName}",filesList.Count, dirName, fileName);
        if (filesList.Count > 0)
        {
            logFileToProcess = this.fileSystem.FileInfo.New(filesList[0]);
            return true;
        }

        logFileToProcess = null;
        return false;
    }
}
