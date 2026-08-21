using Core.DataAccess;
using Core.Services;
using dotenv.net;
using Infrastructure.DataAccess;
using Infrastructure.Services;  // ← для NotificationService
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot_31.Scenarios;
using TelegramBot_31.Classes;
using TelegramBot_31.BackgroundTasks;

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

            // ==========================================
            // 1. ПОДКЛЮЧЕНИЕ К БД И РЕПОЗИТОРИИ
            // ==========================================
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING not found in .env");

            var factory = new DataContextFactory(connectionString);

            var userRepository = new SqlUserRepository(factory);
            var todoRepository = new SqlToDoRepository(factory);
            var listRepository = new SqlToDoListRepository(factory);

            // ==========================================
            // 2. СЕРВИСЫ
            // ==========================================
            var userService = new UserService(userRepository);
            var todoService = new ToDoService(todoRepository, userRepository, maxTasks: 100, maxTaskLength: 500);
            var reportService = new ToDoReportService(todoRepository);
            var listService = new ToDoListService(listRepository);
            var notificationService = new NotificationService(factory);  // ← НОВЫЙ СЕРВИС

            // ==========================================
            // 3. РЕПОЗИТОРИЙ СЦЕНАРИЕВ
            // ==========================================
            var contextRepository = new InMemoryScenarioContextRepository();

            // ==========================================
            // 4. СЦЕНАРИИ
            // ==========================================
            var scenarios = new List<IScenario>
            {
                new AddTaskScenario(userService, todoService, listService),
                new AddListScenario(userService, listService),
                new DeleteListScenario(userService, listService, todoService),
                new DeleteTaskScenario(todoService)
            };

            // ==========================================
            // 5. БОТ
            // ==========================================
            var botClient = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();

            // ==========================================
            // 6. ХЕНДЛЕР
            // ==========================================
            var handler = new UpdateHandler(
                userService,
                todoService,
                reportService,
                listService,
                contextRepository,
                scenarios);

            // ==========================================
            // 7. ФОНОВЫЕ ЗАДАЧИ
            // ==========================================
            var backgroundTaskRunner = new BackgroundTaskRunner();

            // 7.1 Сброс сценариев (1 час)
            var resetTask = new ResetScenarioBackgroundTask(
                TimeSpan.FromHours(1),
                contextRepository,
                botClient
            );
            backgroundTaskRunner.AddTask(resetTask);

            // 7.2 Отправка нотификаций (каждую минуту)
            var notificationTask = new NotificationBackgroundTask(
                notificationService,
                botClient
            );
            backgroundTaskRunner.AddTask(notificationTask);

            // 7.3 Дедлайны (каждый час)
            var deadlineTask = new DeadlineBackgroundTask(
                notificationService,
                userRepository,
                todoRepository
            );
            backgroundTaskRunner.AddTask(deadlineTask);

            // 7.4 Задачи на сегодня (раз в день)
            var todayTask = new TodayBackgroundTask(
                notificationService,
                userRepository,
                todoRepository
            );
            backgroundTaskRunner.AddTask(todayTask);

            backgroundTaskRunner.StartTasks(cts.Token);

            // ==========================================
            // 8. ЗАПУСК БОТА
            // ==========================================
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
            Console.WriteLine($"✅ Бот @{me.Username} запущен!");
            Console.WriteLine("Нажмите клавишу A для выхода");

            // ==========================================
            // 9. ОЖИДАНИЕ ЗАВЕРШЕНИЯ
            // ==========================================
            try
            {
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
            finally
            {
                await backgroundTaskRunner.StopTasks(CancellationToken.None);
                Console.WriteLine("Бот остановлен.");
            }
        }
    }
}