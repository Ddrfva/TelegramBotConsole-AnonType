using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _tasks = new();

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ToDoItem>>(_tasks.Where(t => t.UserId == userId).ToList());
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ToDoItem>>(_tasks.Where(t => t.UserId == userId && t.State == 0).ToList());
        }

        public Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));
        }

        public Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            _tasks.Add(item);
            return Task.CompletedTask;
        }

        public Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            var index = _tasks.FindIndex(t => t.Id == item.Id);
            if (index != -1)
                _tasks[index] = item;
            return Task.CompletedTask;
        }

        public Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
                _tasks.Remove(task);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            return Task.FromResult(_tasks.Any(t => t.UserId == userId && t.Name == name));
        }

        public Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_tasks.Count(t => t.UserId == userId && t.State == 0));
        }

        public Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ToDoItem>>(_tasks.Where(t => t.UserId == userId && predicate(t)).ToList());
        }
    }
}