using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_31.Scenarios;

namespace TelegramBot_31.BackgroundTasks
{
    public class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;
        private readonly IScenarioContextRepository _scenarioRepository;
        private readonly ITelegramBotClient _bot;

        public ResetScenarioBackgroundTask(
            TimeSpan resetScenarioTimeout,
            IScenarioContextRepository scenarioRepository,
            ITelegramBotClient bot)
            : base(resetScenarioTimeout, nameof(ResetScenarioBackgroundTask))
        {
            _resetScenarioTimeout = resetScenarioTimeout;
            _scenarioRepository = scenarioRepository;
            _bot = bot;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var contexts = await _scenarioRepository.GetContexts(ct);

            var expiredContexts = contexts
                .Where(x => x.Context.CreatedAt + _resetScenarioTimeout < DateTime.UtcNow)
                .ToList();

            foreach (var (userId, context) in expiredContexts)
            {
                try
                {
                    await _scenarioRepository.ResetContext(userId, ct);

                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "/addtask", "/addlist" },
                        new KeyboardButton[] { "/show", "/report" },
                        new KeyboardButton[] { "/cancel", "/exit" }
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = false
                    };

                    await _bot.SendMessage(
                        chatId: userId,
                        text: $"Сценарий отменен, так как не поступил ответ в течение {_resetScenarioTimeout.TotalHours} часов.",
                        replyMarkup: keyboard,
                        cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resetting scenario for user {userId}: {ex.Message}");
                }
            }
        }
    }
}