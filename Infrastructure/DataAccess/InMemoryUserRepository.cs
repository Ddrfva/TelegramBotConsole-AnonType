using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));
        }

        public Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.TelegramUserId == telegramUserId));
        }

        public Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoUser>>(_users.ToList());
        }
    }
}