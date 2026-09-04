using Core.DataAccess;
using Core.Entities;
using Core.Exceptions;
using Core.Constants;

namespace Core.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _listRepository;

        public ToDoListService(IToDoListRepository listRepository)
        {
            _listRepository = listRepository;
        }

        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название списка не может быть пустым");

            if (name.Length > AppConstants.MaxListNameLength)
                throw new ArgumentException($"Название списка не может быть длиннее {AppConstants.MaxListNameLength} символов");

            if (await _listRepository.ExistsByName(user.Id, name, ct))
                throw new DuplicateTaskException($"Список с именем '{name}' уже существует");

            var list = new ToDoList
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _listRepository.Add(list, ct);
            return list;
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            return await _listRepository.Get(id, ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            await _listRepository.Delete(id, ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return await _listRepository.GetByUserId(userId, ct);
        }
    }
}