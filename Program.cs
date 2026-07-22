using dotenv.net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Core.Services;
using Core.DataAccess;
using Infrastructure.DataAccess;
using TelegramBot_26.Classes;

namespace TelegramBot_26
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DotEnv.Load();

            var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Ошибка: Токен бота не найден. Убедитесь, что файл .env настроен правильно.");
                return;
            }

            var dataPath = Path.Combine(Environment.CurrentDirectory, "Data");

            var userRepository = new FileUserRepository(Path.Combine(dataPath, "Users"));
            var todoRepository = new FileToDoRepository(Path.Combine(dataPath, "Tasks"));

            var userService = new UserService(userRepository);
            var todoService = new ToDoService(todoRepository, userRepository, maxTasks: 100, maxTaskLength: 500);
            var reportService = new ToDoReportService(todoRepository);

            var botClient = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };

            var handler = new UpdateHandler(userService, todoService, reportService);

            botClient.StartReceiving(
                handler.HandleUpdateAsync,
                handler.HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            await botClient.SetMyCommands(
                commands: new[]
                {
                    new BotCommand { Command = "start", Description = "Начать работу" },
                    new BotCommand { Command = "addtask", Description = "Добавить задачу" },
                    new BotCommand { Command = "showtasks", Description = "Показать активные задачи" },
                    new BotCommand { Command = "showalltasks", Description = "Показать все задачи" },
                    new BotCommand { Command = "report", Description = "Статистика" },
                    new BotCommand { Command = "find", Description = "Найти задачу" },
                    new BotCommand { Command = "completetask", Description = "Завершить задачу по Id" },
                },
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMe(cancellationToken: cts.Token);
            Console.WriteLine($"Бот @{me.Username} запущен!");
            Console.WriteLine("Нажмите клавишу A для выхода");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.A)
                {
                    Console.WriteLine("Завершение работы...");
                    await cts.CancelAsync();
                    break;
                }
            }
        }
    }
}