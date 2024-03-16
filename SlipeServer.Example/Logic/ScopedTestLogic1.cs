using Microsoft.Extensions.Logging;
using System.Threading;

namespace SlipeServer.Example.Logic;

public class SampleScopedService
{
    public static int _counter;
    public SampleScopedService(ILogger logger)
    {
        Interlocked.Increment(ref _counter);
        logger.LogInformation("SampleScopedService instance id {0} created", _counter);
    }
}

public class ScopedTestLogic1
{
    public ScopedTestLogic1(ILogger logger, SampleScopedService sampleScopedService1, SampleScopedService sampleScopedService2)
    {
        logger.LogInformation("ScopedTestLogic1: {counter} {areServicesTheSame}", SampleScopedService._counter, sampleScopedService1 == sampleScopedService2);
    }
}

public class ScopedTestLogic2
{
    public ScopedTestLogic2(ILogger logger, SampleScopedService sampleScopedService1, SampleScopedService sampleScopedService2)
    {
        logger.LogInformation("ScopedTestLogic2: {counter} {areServicesTheSame}", SampleScopedService._counter, sampleScopedService1 == sampleScopedService2);
    }
}
