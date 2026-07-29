<<<<<<< HEAD
﻿using System;
using System.Text.Json.Serialization;

namespace Core.Entities
=======
﻿namespace Core.Entities
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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
<<<<<<< HEAD
        public DateTime Deadline { get; private set; }
        public ToDoItemState State { get; private set; }
        public DateTime? StateChangedAtUtc { get; private set; }

        [JsonIgnore]
        public DateTime CreatedAtLocal => CreatedAtUtc.ToLocalTime();

        [JsonIgnore]
        public DateTime? StateChangedAtLocal => StateChangedAtUtc?.ToLocalTime();

        public ToDoItem(ToDoUser user, string name, DateTime deadline)
=======
        public ToDoItemState State { get; private set; }
        public DateTime? StateChangedAtUtc { get; private set; }

        public DateTime CreatedAtLocal => CreatedAtUtc.ToLocalTime();
        public DateTime? StateChangedAtLocal => StateChangedAtUtc?.ToLocalTime();

        public ToDoItem(ToDoUser user, string name)
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAtUtc = DateTime.UtcNow;
<<<<<<< HEAD
            Deadline = deadline.ToUniversalTime();
=======
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
            State = ToDoItemState.Active;
            StateChangedAtUtc = null;
        }

<<<<<<< HEAD
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

=======
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
        public void Complete()
        {
            State = ToDoItemState.Completed;
            StateChangedAtUtc = DateTime.UtcNow;
        }
    }
}