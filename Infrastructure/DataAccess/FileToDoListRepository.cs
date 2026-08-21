using System.Text.Json;
using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _basePath;

        public FileToDoListRepository(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
            Console.WriteLine($"[FileToDoListRepository] Путь: {_basePath}");
        }

        private string GetFilePath(Guid listId)
        {
            return Path.Combine(_basePath, $"{listId}.json");
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var path = GetFilePath(id);
            if (!File.Exists(path))
                return null;

            var json = await File.ReadAllTextAsync(path, ct);
            return JsonSerializer.Deserialize<ToDoList>(json);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var result = new List<ToDoList>();
            if (!Directory.Exists(_basePath))
                return result;

            foreach (var file in Directory.GetFiles(_basePath, "*.json"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file, ct);
                    var list = JsonSerializer.Deserialize<ToDoList>(json);
                    if (list != null && list.UserId == userId)
                        result.Add(list);
                }
                catch
                {
                    // Пропускаем повреждённые файлы
                }
            }
            return result;
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            var path = GetFilePath(list.Id);
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json, ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            var path = GetFilePath(id);
            if (File.Exists(path))
                File.Delete(path);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var lists = await GetByUserId(userId, ct);
            return lists.Any(l => l.Name == name);
        }
    }
}