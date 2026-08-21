using System;

namespace Core.Entities
{
    public enum ToDoItemState
    {
        Active = 0,
        Completed = 1
    }

    public class ToDoItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public Guid UserId { get; set; }
        public Guid? ListId { get; set; }
        public int? WateringFrequencyDays { get; set; }
        public DateTime? LastWateredAt { get; set; }
        public string LightRequirement { get; set; }
        public string Notes { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? StateChangedAtUtc { get; set; }

        // мнбне ябниярбн
        public DateTime? Deadline { get; set; }

        public ToDoUser User { get; set; }
        public ToDoList List { get; set; }
    }
}