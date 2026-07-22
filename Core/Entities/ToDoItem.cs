using System;

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

        public DateTime CreatedAtLocal => CreatedAtUtc.ToLocalTime();
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

        public void Complete()
        {
            State = ToDoItemState.Completed;
            StateChangedAtUtc = DateTime.UtcNow;
        }
    }
}