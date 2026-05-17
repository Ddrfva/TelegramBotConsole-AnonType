using Core.DataAccess;
using Core.Entities;
using Core.Exceptions;

namespace Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;
        private readonly IUserRepository _userRepository;
        private readonly int _maxTasks;
        private readonly int _maxTaskLength;

        public ToDoService(IToDoRepository toDoRepository, IUserRepository userRepository, int maxTasks, int maxTaskLength)
        {
            _toDoRepository = toDoRepository;
            _userRepository = userRepository;
            _maxTasks = maxTasks;
            _maxTaskLength = maxTaskLength;
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название задачи не может быть пустым");

            if (name.Length > _maxTaskLength)
                throw new TaskLengthLimitException(name.Length, _maxTaskLength);

            if (_toDoRepository.ExistsByName(user.UserId, name))
                throw new DuplicateTaskException(name);

            var activeCount = _toDoRepository.CountActive(user.UserId);
            if (activeCount >= _maxTasks)
                throw new TaskCountLimitException(_maxTasks);

            var newTask = new ToDoItem(user, name);
            _toDoRepository.Add(newTask);
            return newTask;
        }

        public void MarkCompleted(Guid id)
        {
            var task = _toDoRepository.Get(id);
            if (task == null)
                throw new ArgumentException($"Задача с Id '{id}' не найдена");

            task.Complete();
            _toDoRepository.Update(task);
        }

        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.UserId, t => t.Name.StartsWith(namePrefix));
        }
    }
}