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
    public class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.Lists
                .LoadWith(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            return ModelMapper.MapFromModel(model);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.Lists
                .LoadWith(l => l.User)
                .Where(l => l.UserId == userId)
                .ToListAsync();

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(list);
            await dbContext.InsertAsync(model);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.Lists
                .Where(l => l.Id == id)
                .DeleteAsync();
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.Lists
                .AnyAsync(l => l.UserId == userId && l.Name == name);
        }
    }
}