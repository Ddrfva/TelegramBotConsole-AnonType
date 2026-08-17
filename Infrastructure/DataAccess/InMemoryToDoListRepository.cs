using System.Collections.Concurrent;
using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class InMemoryToDoListRepository : IToDoListRepository
    {
        private readonly ConcurrentDictionary<Guid, ToDoList> _storage = new();

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            _storage.TryGetValue(id, out var list);
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var lists = _storage.Values.Where(l => l.UserId == userId).ToList();
            return Task.FromResult<IReadOnlyList<ToDoList>>(lists);
        }

        public Task Add(ToDoList list, CancellationToken ct)
        {
            _storage[list.Id] = list;
            return Task.CompletedTask;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            _storage.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var exists = _storage.Values.Any(l => l.UserId == userId && l.Name == name);
            return Task.FromResult(exists);
        }
    }
}