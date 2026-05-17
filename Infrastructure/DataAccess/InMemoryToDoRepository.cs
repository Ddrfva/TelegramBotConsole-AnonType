using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _tasks = new();

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId).ToList();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
        }

        public ToDoItem? Get(Guid id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        public void Add(ToDoItem item)
        {
            _tasks.Add(item);
        }

        public void Update(ToDoItem item)
        {
            var index = _tasks.FindIndex(t => t.Id == item.Id);
            if (index != -1)
                _tasks[index] = item;
        }

        public void Delete(Guid id)
        {
            var task = Get(id);
            if (task != null)
                _tasks.Remove(task);
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return _tasks.Any(t => t.User.UserId == userId && t.Name == name);
        }

        public int CountActive(Guid userId)
        {
            return _tasks.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return _tasks.Where(t => t.User.UserId == userId && predicate(t)).ToList();
        }
    }
}