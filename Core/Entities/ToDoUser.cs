using System;

namespace Core.Entities
{
    public class ToDoUser
    {
        public Guid Id { get; set; }
        public long TelegramUserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAtUtc { get; set; }
    }
}