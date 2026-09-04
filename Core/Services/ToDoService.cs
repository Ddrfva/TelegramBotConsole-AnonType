using Core.DataAccess;
using Core.Entities;
using Core.Exceptions;
using Core.Constants;

namespace Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;
        private readonly IUserRepository _userRepository;

        public ToDoService(IToDoRepository toDoRepository, IUserRepository userRepository)
        {
            _toDoRepository = toDoRepository;
            _userRepository = userRepository;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetAllByUserId(userId, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetActiveByUserId(userId, cancellationToken);
        }

        public async Task<ToDoItem> Add(ToDoUser user, string name, Guid? listId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название задачи не может быть пустым");

            if (name.Length > AppConstants.MaxTaskLength)
                throw new TaskLengthLimitException(name.Length, AppConstants.MaxTaskLength);

            if (await _toDoRepository.ExistsByName(user.Id, name, cancellationToken))
                throw new DuplicateTaskException(name);

            var activeCount = await _toDoRepository.CountActive(user.Id, cancellationToken);
            if (activeCount >= AppConstants.MaxTasksPerUser)
                throw new TaskCountLimitException(AppConstants.MaxTasksPerUser);

            var newTask = new ToDoItem
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = user.Id,
                ListId = listId,
                State = ToDoItemState.Active,
                CreatedAtUtc = DateTime.UtcNow,
                StateChangedAtUtc = DateTime.UtcNow
            };

            await _toDoRepository.Add(newTask, cancellationToken);
            return newTask;
        }

        public async Task MarkCompleted(Guid id, CancellationToken cancellationToken)
        {
            var task = await _toDoRepository.Get(id, cancellationToken);
            if (task == null)
                throw new ArgumentException($"Задача с Id '{id}' не найдена");

            task.State = ToDoItemState.Completed;
            task.StateChangedAtUtc = DateTime.UtcNow;
            await _toDoRepository.Update(task, cancellationToken);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            await _toDoRepository.Delete(id, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken cancellationToken)
        {
            return await _toDoRepository.Find(user.Id, t => t.Name.StartsWith(namePrefix), cancellationToken);
        }
    }
}