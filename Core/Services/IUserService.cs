using Core.Entities;

namespace Core.Services
{
    public interface IUserService
    {
        Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken); // ← ДОБАВИТЬ!
        Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken);
    }
}