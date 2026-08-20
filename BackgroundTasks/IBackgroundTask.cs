using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_31.BackgroundTasks
{
    public interface IBackgroundTask
    {
        Task Start(CancellationToken ct);
    }
}