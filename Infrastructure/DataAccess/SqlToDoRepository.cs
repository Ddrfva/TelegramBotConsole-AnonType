using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace Infrastructure.DataAccess
{
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.Items
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync();

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.Items
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId && i.State == 0)
                .ToListAsync();

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.Items
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            return ModelMapper.MapFromModel(model);
        }

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(item);
            await dbContext.InsertAsync(model);
        }

        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(item);
            await dbContext.UpdateAsync(model);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.Items
                .Where(i => i.Id == id)
                .DeleteAsync();
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.Items
                .AnyAsync(i => i.UserId == userId && i.Name == name);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.Items
                .Where(i => i.UserId == userId && i.State == 0)
                .CountAsync();
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.Items
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var entities = models.Select(ModelMapper.MapFromModel).ToList();
            return entities.Where(predicate).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(
            Guid userId,
            DateTime from,
            DateTime to,
            CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.Items
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .Where(i => i.UserId == userId
                    && i.State == 0
                    && i.Deadline != null
                    && i.Deadline >= from
                    && i.Deadline < to)
                .ToListAsync();

            return models.Select(ModelMapper.MapFromModel).ToList();
        }
    }
}