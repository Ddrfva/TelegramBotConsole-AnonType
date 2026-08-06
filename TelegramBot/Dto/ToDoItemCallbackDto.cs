using System;

namespace TelegramBot_29.TelegramBot.Dto
{
    public class ToDoItemCallbackDto : CallbackDto
    {
        public Guid ToDoItemId { get; set; }

        public ToDoItemCallbackDto(string action, Guid toDoItemId) : base(action)
        {
            ToDoItemId = toDoItemId;
        }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new ToDoItemCallbackDto(string.Empty, Guid.Empty);

            var parts = input.Split('|');
            var action = parts[0];

            if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid itemId))
                return new ToDoItemCallbackDto(action, itemId);

            return new ToDoItemCallbackDto(action, Guid.Empty);
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}";
        }
    }
}