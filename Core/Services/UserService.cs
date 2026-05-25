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

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
            if (existingUser != null)
                return existingUser;

            var user = new ToDoUser(telegramUserId, telegramUserName);
            await _userRepository.Add(user, cancellationToken);
            return user;
        }

        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
        }
    }
}