namespace LogForwarder.Receiver.File;

public class FileReceiverWorkerOptions<T>
{
    public string Path { get; set; }
    public FileFormatType FormatType { get; set; }
    public FileCompressionType CompressionType { get; set; }

}
