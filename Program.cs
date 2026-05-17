using Core.Services;
using Core.DataAccess;
using Infrastructure.DataAccess;
using TelegramBot;
using Otus.ToDoList.ConsoleBot;

namespace TelegramBotConsole_Interface
{
    class Program
    {
        static void Main(string[] args)
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

            botClient.StartReceiving(updateHandler);

            Console.WriteLine("Бот запущен. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}