using System;
using System.Text.Json.Serialization;

namespace Core.Entities
{
    public enum ToDoItemState
    {
        Active,
        Completed
    }

    public class ToDoItem
    {
        public Guid Id { get; }
        public ToDoUser User { get; }
        public string Name { get; }
        public DateTime CreatedAtUtc { get; }
        public DateTime Deadline { get; private set; }
        public ToDoItemState State { get; private set; }
        public DateTime? StateChangedAtUtc { get; private set; }

        [JsonIgnore]
        public DateTime CreatedAtLocal => CreatedAtUtc.ToLocalTime();

        [JsonIgnore]
        public DateTime? StateChangedAtLocal => StateChangedAtUtc?.ToLocalTime();

        public ToDoItem(ToDoUser user, string name, DateTime deadline)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAtUtc = DateTime.UtcNow;
            Deadline = deadline.ToUniversalTime();
            State = ToDoItemState.Active;
            StateChangedAtUtc = null;
        }

        [JsonConstructor]
        public ToDoItem(
            Guid id,
            ToDoUser user,
            string name,
            DateTime createdAtUtc,
            DateTime deadline,
            ToDoItemState state,
            DateTime? stateChangedAtUtc)
        {
            Id = id;
            User = user;
            Name = name;
            CreatedAtUtc = createdAtUtc;
            Deadline = deadline;
            State = state;
            StateChangedAtUtc = stateChangedAtUtc;
        }

        public void Complete()
        {
            State = ToDoItemState.Completed;
            StateChangedAtUtc = DateTime.UtcNow;
        }
    }
}