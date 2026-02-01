using ApiTwitterUala.Services.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTwitterUala.Tests.TestHelpers
{
    public class NoOpBackgroundTaskQueue : IBackgroundTaskQueue
    {
        public Task<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            _ = Task.Run(() => workItem(CancellationToken.None));
        }
    }
}