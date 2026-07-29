using dotenv.net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Core.Services;
using Core.DataAccess;
using Infrastructure.DataAccess;
<<<<<<< HEAD
using TelegramBot_27.TelegramBot;
using TelegramBot_27.TelegramBot.Scenarios;

namespace TelegramBot_27
=======
using TelegramBot_26.Classes;

namespace TelegramBot_26
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DotEnv.Load();

            var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
<<<<<<< HEAD
                Console.WriteLine("Ошибка: Токен бота не найден.");
=======
                Console.WriteLine("Ошибка: Токен бота не найден. Убедитесь, что файл .env настроен правильно.");
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
                return;
            }

            var dataPath = Path.Combine(Environment.CurrentDirectory, "Data");
<<<<<<< HEAD
=======

>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
            var userRepository = new FileUserRepository(Path.Combine(dataPath, "Users"));
            var todoRepository = new FileToDoRepository(Path.Combine(dataPath, "Tasks"));

            var userService = new UserService(userRepository);
            var todoService = new ToDoService(todoRepository, userRepository, maxTasks: 100, maxTaskLength: 500);
            var reportService = new ToDoReportService(todoRepository);

<<<<<<< HEAD
            var contextRepository = new InMemoryScenarioContextRepository();
            var scenarios = new List<IScenario>
            {
                new AddTaskScenario(userService, todoService)
            };

            var handler = new UpdateHandler(userService, todoService, reportService, contextRepository, scenarios);

=======
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
            var botClient = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };

<<<<<<< HEAD
=======
            var handler = new UpdateHandler(userService, todoService, reportService);

>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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