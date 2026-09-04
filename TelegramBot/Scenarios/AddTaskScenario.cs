using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_31.Scenarios;
using Core.Constants;

namespace TelegramBot_31.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoListService _listService;

        public AddTaskScenario(IUserService userService, IToDoService todoService, IToDoListService listService)
        {
            _userService = userService;
            _todoService = todoService;
            _listService = listService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            var chatId = message.Chat.Id;

            if (!context.Data.TryGetValue("User", out var userObj) || userObj is not ToDoUser user)
            {
                await bot.SendMessage(chatId, "Сначала зарегистрируйтесь через /start", cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            switch (context.CurrentStep)
            {
                case null:
                    context.CurrentStep = "Name";
                    await bot.SendMessage(chatId, "🌱 Введите название растения:", cancellationToken: ct);
                    return ScenarioResult.Transition;

                case "Name":
                    var name = message.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(name) || name.Length > AppConstants.MaxTaskNameLength)
                    {
                        await bot.SendMessage(chatId, "Название должно быть от 1 до 200 символов. Попробуйте снова:", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    context.Data["PlantName"] = name;
                    context.CurrentStep = "Species";
                    await bot.SendMessage(chatId, "🌿 Введите вид растения (или нажмите /skip):", cancellationToken: ct);
                    return ScenarioResult.Transition;

                case "Species":
                    if (message.Text != "/skip")
                    {
                        var species = message.Text?.Trim();
                        if (!string.IsNullOrWhiteSpace(species))
                            context.Data["Species"] = species;
                    }

                    context.CurrentStep = "List";

                    var lists = await _listService.GetUserLists(user.Id, ct);
                    if (lists.Any())
                    {
                        var buttons = lists.Select(list =>
                            InlineKeyboardButton.WithCallbackData($"📁 {list.Name}", $"list_{list.Id}")
                        ).ToList();
                        buttons.Add(InlineKeyboardButton.WithCallbackData("➕ Без списка", "list_null"));

                        var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));
                        await bot.SendMessage(chatId, "Выберите список для растения (или создайте новый через /addlist):",
                            replyMarkup: keyboard, cancellationToken: ct);
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "У вас пока нет списков. Создайте список через /addlist или просто добавьте растение без списка.",
                            cancellationToken: ct);
                        context.CurrentStep = "NoList";
                    }
                    return ScenarioResult.Transition;

                case "NoList":
                    await AddPlant(bot, chatId, context, user, null, ct);
                    return ScenarioResult.Completed;

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг. Начните заново.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }

        private async Task AddPlant(ITelegramBotClient bot, long chatId, ScenarioContext context, ToDoUser user, Guid? listId, CancellationToken ct)
        {
            try
            {
                var name = context.Data["PlantName"].ToString();
                var species = context.Data.ContainsKey("Species") ? context.Data["Species"]?.ToString() : null;

                var item = await _todoService.Add(user, name, listId, ct);

                await bot.SendMessage(chatId, $"✅ Растение \"{name}\" добавлено! 🌱", cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ Ошибка: {ex.Message}", cancellationToken: ct);
            }
        }
    }
}