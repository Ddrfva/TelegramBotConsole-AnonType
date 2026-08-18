using Core.DataAccess;
using Core.Services;
using Core.Entities;
using dotenv.net;
using Infrastructure.DataAccess;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot_27_2.Scenarios;
using TelegramBot_30.Classes;

namespace TelegramBot_30
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DotEnv.Load();

            var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Ошибка: Токен бота не найден.");
                return;
            }

            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? "Host=localhost;Port=5432;Database=FlowerCare;Username=postgres;Password=0311;";

            var factory = new DataContextFactory(connectionString);

            var userRepository = new SqlUserRepository(factory);
            var todoRepository = new SqlToDoRepository(factory);
            var listRepository = new SqlToDoListRepository(factory);

            var userService = new UserService(userRepository);
            var todoService = new ToDoService(todoRepository, userRepository, maxTasks: 100, maxTaskLength: 500);
            var reportService = new ToDoReportService(todoRepository);
            var listService = new ToDoListService(listRepository);

            var contextRepository = new InMemoryScenarioContextRepository();

            var scenarios = new List<IScenario>
            {
                new AddTaskScenario(userService, todoService, listService),
                new AddListScenario(userService, listService),
                new DeleteListScenario(userService, listService, todoService),
                new DeleteTaskScenario(todoService)
            };

            var handler = new UpdateHandler(
                userService,
                todoService,
                reportService,
                listService,
                contextRepository,
                scenarios);

            var botClient = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };

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
                    new BotCommand { Command = "addlist", Description = "Создать список" },
                    new BotCommand { Command = "show", Description = "Показать задачи по спискам" },
                    new BotCommand { Command = "report", Description = "Статистика" },
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