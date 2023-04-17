# File Receiver

This project is a console application which acts as a file watcher for logs output by the collector using the [file exporter](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/fileexporter).

This relies on the [file rotation](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/fileexporter#file-rotation) feature to avoid file lock and synchronisation issues, as once a file is rotated, it is renamed and no longer touched by the collector.

## Input
Below is an example of the files this Receiver can process. To avoid file locking and synchronisation issues, the receiver only reads the rotated files, i.e. those suffixed with the timestamp they were rotated out. This also allows us to limit the input size of each batch the the forwarder processes. Upon successful processing of the file, the forwarder deletes the file, so it is not processed again. In future, we could add an option move the file to another location once they've been processed.
![image](https://user-images.githubusercontent.com/6062228/230997127-4096afc4-68e4-4c6e-84c7-5a6af955cc33.png)

