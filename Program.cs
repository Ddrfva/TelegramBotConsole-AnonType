using Core.DataAccess;
using Core.Services;
using dotenv.net;
using Infrastructure.DataAccess;
using Infrastructure.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot_31.Scenarios;
using TelegramBot_31.Classes;
using TelegramBot_31.BackgroundTasks;
using TelegramBot_31.WebAdmin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot_31
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DotEnv.Load();

            var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("❌ Ошибка: Токен бота не найден.");
                return;
            }

            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING not found in .env");

            var factory = new DataContextFactory(connectionString);

            var userRepository = new SqlUserRepository(factory);
            var todoRepository = new SqlToDoRepository(factory);
            var listRepository = new SqlToDoListRepository(factory);

            var userService = new UserService(userRepository);
            var todoService = new ToDoService(todoRepository, userRepository);
            var reportService = new ToDoReportService(todoRepository);
            var listService = new ToDoListService(listRepository);
            var notificationService = new NotificationService(factory);

            var contextRepository = new InMemoryScenarioContextRepository();

            var scenarios = new List<IScenario>
            {
                new AddTaskScenario(userService, todoService, listService),
                new AddListScenario(userService, listService),
                new DeleteListScenario(userService, listService, todoService),
                new DeleteTaskScenario(todoService)
            };

            var webHost = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:5000");
                    webBuilder.UseSetting("ContentRootPath", Directory.GetCurrentDirectory());
                })
                .Build();

            _ = webHost.RunAsync();

            Console.WriteLine("🌐 Веб-администрирование доступно по адресу: http://localhost:5000");

            try
            {
                var botClient = new TelegramBotClient(token);
                var cts = new CancellationTokenSource();

                var handler = new UpdateHandler(
                    userService,
                    todoService,
                    reportService,
                    listService,
                    contextRepository,
                    scenarios);

                var backgroundTaskRunner = new BackgroundTaskRunner();

                backgroundTaskRunner.AddTask(new ResetScenarioBackgroundTask(
                    TimeSpan.FromHours(1),
                    contextRepository,
                    botClient
                ));
                backgroundTaskRunner.AddTask(new NotificationBackgroundTask(
                    notificationService,
                    botClient
                ));
                backgroundTaskRunner.AddTask(new DeadlineBackgroundTask(
                    notificationService,
                    userRepository,
                    todoRepository
                ));
                backgroundTaskRunner.AddTask(new TodayBackgroundTask(
                    notificationService,
                    userRepository,
                    todoRepository
                ));

                backgroundTaskRunner.StartTasks(cts.Token);

                botClient.StartReceiving(
                    handler.HandleUpdateAsync,
                    handler.HandleErrorAsync,
                    new ReceiverOptions
                    {
                        AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                        DropPendingUpdates = true
                    },
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
                Console.WriteLine($"✅ Бот @{me.Username} запущен!");
                Console.WriteLine("Нажмите клавишу A для выхода");

                while (true)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.A)
                    {
                        Console.WriteLine("⏹️ Завершение работы...");
                        await cts.CancelAsync();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка бота: {ex.Message}");
                Console.WriteLine("Админка продолжает работать на http://localhost:5000");

                while (true)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.A)
                    {
                        break;
                    }
                }
            }
        }
    }
}