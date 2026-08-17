using System;
using System.Threading;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.Entities;

namespace Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUser(userId, cancellationToken);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
        }

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
            if (existingUser != null)
                return existingUser;

            var user = new ToDoUser
            {
                Id = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAtUtc = DateTime.UtcNow
            };

            await _userRepository.Add(user, cancellationToken);
            return user;
        }
    }
}