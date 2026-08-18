using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TelegramBot_31.BackgroundTasks
{
    public abstract class BackgroundTask : IBackgroundTask
    {
        private readonly TimeSpan _delay;
        private readonly string _name;

        protected BackgroundTask(TimeSpan delay, string name)
        {
            _delay = delay;
            _name = name;
        }

        protected abstract Task Execute(CancellationToken ct);

        public async Task Start(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"{_name}. Execute");
                    await Execute(ct);

                    Console.WriteLine($"{_name}. Start delay {_delay}");
                    await Task.Delay(_delay, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    //завершение при отмене
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{_name}. Error: {ex}");
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
            }
        }
    }
}