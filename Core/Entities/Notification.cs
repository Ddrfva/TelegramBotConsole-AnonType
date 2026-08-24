using System;

namespace Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public Guid UserId { get; set; }
        public long TelegramUserId { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public DateTime ScheduledAt { get; set; }
        public bool IsNotified { get; set; }
        public DateTime? NotifiedAt { get; set; }
    }
}
