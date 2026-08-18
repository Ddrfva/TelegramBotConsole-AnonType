using System;
using System.Threading;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace Infrastructure.DataAccess
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            return ModelMapper.MapFromModel(model);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.Users
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, cancellationToken);

            return ModelMapper.MapFromModel(model);
        }

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(user);
            await dbContext.InsertAsync(model);
        }
    }
}