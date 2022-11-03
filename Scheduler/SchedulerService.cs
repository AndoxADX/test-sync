using System;
using System.Threading;
using System.Threading.Tasks;
namespace TodoApi.Store
{   
     public class SchedulerService : HostedService
{
    private readonly TrxScheduler _provider;

    public SchedulerService(TrxScheduler provider)
    {
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _provider.RunScheduledTask(cancellationToken);
            // await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }
    }
}
}