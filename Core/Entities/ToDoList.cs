using System;
using System.Text.Json.Serialization;

namespace Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; }
        public string Name { get; }
        public ToDoUser User { get; }
        public DateTime CreatedAt { get; }

        public ToDoList(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
        }

        [JsonConstructor]
        public ToDoList(Guid id, string name, ToDoUser user, DateTime createdAt)
        {
            Id = id;
            Name = name;
            User = user;
            CreatedAt = createdAt;
        }
    }
}