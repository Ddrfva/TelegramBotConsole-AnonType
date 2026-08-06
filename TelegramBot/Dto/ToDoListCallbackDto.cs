using System;

namespace TelegramBot_29.TelegramBot.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        public ToDoListCallbackDto(string action, Guid? toDoListId = null) : base(action)
        {
            ToDoListId = toDoListId;
        }

        public static new ToDoListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new ToDoListCallbackDto(string.Empty);

            var parts = input.Split('|');
            var action = parts[0];

            if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid listId))
                return new ToDoListCallbackDto(action, listId);

            return new ToDoListCallbackDto(action);
        }

        public override string ToString()
        {
            return ToDoListId.HasValue ? $"{base.ToString()}|{ToDoListId}" : base.ToString();
        }
    }
}