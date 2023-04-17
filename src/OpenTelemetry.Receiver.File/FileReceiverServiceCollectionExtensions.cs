using System.IO.Abstractions;

namespace LogForwarder.Receiver.File;

public static class FileReceiverServiceCollectionExtensions
{
    public static IServiceCollection AddFileReceiver(this IServiceCollection services, Action<FileReceiverWorkerOptions> optionsBuilder)
    {
        services.AddOptions<FileReceiverWorkerOptions>().Configure(optionsBuilder);

        services.AddTransient<IFileSystem, FileSystem>();
        services.AddHostedService<FileReceiverWorker>();
        return services;
    }
}