using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBot_31.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _storage = new();

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            _storage.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _storage[userId] = context;
            return Task.CompletedTask;
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            _storage.TryRemove(userId, out _);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<(long UserId, ScenarioContext Context)>> GetContexts(CancellationToken ct)
        {
            var contexts = _storage
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<(long UserId, ScenarioContext Context)>>(contexts);
        }
    }
}