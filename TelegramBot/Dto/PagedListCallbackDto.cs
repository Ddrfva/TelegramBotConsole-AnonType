using System;

namespace TelegramBot_29.TelegramBot.Dto
{
    public class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public PagedListCallbackDto(string action, Guid? toDoListId = null, int page = 0) : base(action, toDoListId)
        {
            Page = page;
        }

        public static new PagedListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new PagedListCallbackDto(string.Empty);

            var parts = input.Split('|');
            var action = parts[0];
            Guid? listId = null;
            int page = 0;

            if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid parsedListId))
                listId = parsedListId;

            if (parts.Length > 2 && int.TryParse(parts[2], out int parsedPage))
                page = parsedPage;

            return new PagedListCallbackDto(action, listId, page);
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{Page}";
        }
    }
}