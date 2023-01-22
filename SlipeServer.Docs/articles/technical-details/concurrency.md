# Concurrency

Slipe Server has been built with concurrency in mind from the start, this does have some implications for consumers of the Slipe Server library though.

Unlike MTA (which processes all packets on a single thread), Slipe Server's packet processing is done using asynchronous worker tasks, which can be run on multiple threads. This allows for higher throughput of packets, but also means that multiple pieces of code can try to access the same memory at the same time.

There are many resources available online to help combat some of the issues that may arise from this.

Slipe Server generously uses the collections from the `System.Collections.Concurrent` namespace, which offer thread-safe implementations of some collections like dictionaries, queues and stacks.

In other situations where it's important that a resource is only used by a single thread we make use of simple C# `lock`s, and the C# [`ReaderWriterLockSlim `](https://learn.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim) class.

