using Otus.ToDoList.ConsoleBot;
using Core.Services;
using Core.DataAccess;
using Infrastructure.DataAccess;

namespace TelegramBotConsole_Async
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.Write("Введите максимальное количество задач (1-100): ");
                int maxTasks = int.Parse(Console.ReadLine());

                Console.Write("Введите максимальную длину задачи (1-100): ");
                int maxTaskLength = int.Parse(Console.ReadLine());

                var userRepository = new InMemoryUserRepository();
                var toDoRepository = new InMemoryToDoRepository();

                var userService = new UserService(userRepository);
                var todoService = new ToDoService(toDoRepository, userRepository, maxTasks, maxTaskLength);
                var reportService = new ToDoReportService(toDoRepository);

                var botClient = new ConsoleBotClient();
                var updateHandler = new UpdateHandler(userService, todoService, reportService);

                using var cts = new CancellationTokenSource();
                botClient.StartReceiving(updateHandler, cts.Token);

                Console.WriteLine("Бот запущен. Нажмите любую клавишу для выхода...");
                Console.ReadKey();
                cts.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
            }
        }
    }
}