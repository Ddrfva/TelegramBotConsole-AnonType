namespace Core.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; }
        public long TelegramUserId { get; }
        public string TelegramUserName { get; }
        public DateTime RegisteredAtUtc { get; }

        public DateTime RegisteredAtLocal => RegisteredAtUtc.ToLocalTime();

        public ToDoUser(long telegramUserId, string telegramUserName)
        {
            UserId = Guid.NewGuid();
            TelegramUserId = telegramUserId;
            TelegramUserName = telegramUserName;
            RegisteredAtUtc = DateTime.UtcNow;
        }
    }
}