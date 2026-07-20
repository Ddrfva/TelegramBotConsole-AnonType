using System.Text.Json;
using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class FileToDoRepository : IToDoRepository
    {
        private readonly string _basePath;
        private readonly string _indexFilePath;
        private Dictionary<Guid, Guid> _index;

        public FileToDoRepository(string basePath)
        {
            _basePath = basePath;
            _indexFilePath = Path.Combine(_basePath, "index.json");
            LoadIndex();
        }

        private void LoadIndex()
        {
            if (File.Exists(_indexFilePath))
            {
                var json = File.ReadAllText(_indexFilePath);
                _index = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json) ?? new Dictionary<Guid, Guid>();
            }
            else
            {
                _index = new Dictionary<Guid, Guid>();
                RebuildIndex();
            }
        }

        private void RebuildIndex()
        {
            if (!Directory.Exists(_basePath))
                return;

            foreach (var userDir in Directory.GetDirectories(_basePath))
            {
                if (!Guid.TryParse(Path.GetFileName(userDir), out Guid userId))
                    continue;

                foreach (var file in Directory.GetFiles(userDir, "*.json"))
                {
                    if (Path.GetFileName(file) == "index.json")
                        continue;

                    if (!Guid.TryParse(Path.GetFileNameWithoutExtension(file), out Guid itemId))
                        continue;

                    _index[itemId] = userId;
                }
            }
            SaveIndex();
        }

        private void SaveIndex()
        {
            var json = JsonSerializer.Serialize(_index);
            File.WriteAllText(_indexFilePath, json);
        }

        private string GetUserDirectory(Guid userId)
        {
            var path = Path.Combine(_basePath, userId.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private string GetItemFilePath(Guid userId, Guid itemId)
        {
            return Path.Combine(GetUserDirectory(userId), $"{itemId}.json");
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var userDir = Path.Combine(_basePath, userId.ToString());
            if (!Directory.Exists(userDir))
                return new List<ToDoItem>();

            var items = new List<ToDoItem>();
            foreach (var file in Directory.GetFiles(userDir, "*.json"))
            {
                if (Path.GetFileName(file) == "index.json")
                    continue;

                try
                {
                    var json = await File.ReadAllTextAsync(file, cancellationToken);
                    var item = JsonSerializer.Deserialize<ToDoItem>(json);
                    if (item != null)
                        items.Add(item);
                }
                catch { /* Пропускаем повреждённые файлы */ }
            }
            return items;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var all = await GetAllByUserId(userId, cancellationToken);
            return all.Where(t => t.State == ToDoItemState.Active).ToList();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            if (!_index.TryGetValue(id, out Guid userId))
                return null;

            var filePath = GetItemFilePath(userId, id);
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<ToDoItem>(json);
        }

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            var userDir = GetUserDirectory(item.User.UserId);
            var filePath = Path.Combine(userDir, $"{item.Id}.json");
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);

            _index[item.Id] = item.User.UserId;
            SaveIndex();
        }

        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            if (!_index.TryGetValue(item.Id, out Guid userId))
                throw new Exception($"Item with Id {item.Id} not found");

            var filePath = GetItemFilePath(userId, item.Id);
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            if (!_index.TryGetValue(id, out Guid userId))
                return;

            var filePath = GetItemFilePath(userId, id);
            if (File.Exists(filePath))
                File.Delete(filePath);

            _index.Remove(id);
            SaveIndex();
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            var items = GetAllByUserId(userId, cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(items.Any(t => t.Name == name));
        }

        public Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            var active = GetActiveByUserId(userId, cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(active.Count);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var all = await GetAllByUserId(userId, cancellationToken);
            return all.Where(predicate).ToList();
        }
    }
}