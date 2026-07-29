using System.Text.Json;
using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class FileUserRepository : IUserRepository
    {
        private readonly string _basePath;

        public FileUserRepository(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        private string GetFilePath(Guid userId)
        {
            return Path.Combine(_basePath, $"{userId}.json");
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            var filePath = GetFilePath(userId);
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<ToDoUser>(json);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_basePath))
                return null;

            foreach (var file in Directory.GetFiles(_basePath, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var user = JsonSerializer.Deserialize<ToDoUser>(json);
                if (user != null && user.TelegramUserId == telegramUserId)
                    return user;
            }
            return null;
        }

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            var filePath = GetFilePath(user.UserId);
            var json = JsonSerializer.Serialize(user);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }
    }
}