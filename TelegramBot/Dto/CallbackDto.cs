namespace TelegramBot_29.TelegramBot.Dto
{
    public class CallbackDto
    {
        public string Action { get; set; }

        public CallbackDto(string action)
        {
            Action = action;
        }

        public static CallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new CallbackDto(string.Empty);

            var parts = input.Split('|');
            return new CallbackDto(parts[0]);
        }

        public override string ToString()
        {
            return Action;
        }
    }
}