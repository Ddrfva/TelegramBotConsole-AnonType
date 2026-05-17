using Core.DataAccess;
using Core.Entities;

namespace Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _toDoRepository;

        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var allTasks = _toDoRepository.GetAllByUserId(userId);
            var total = allTasks.Count;
            var completed = allTasks.Count(t => t.State == ToDoItemState.Completed);
            var active = allTasks.Count(t => t.State == ToDoItemState.Active);
            return (total, completed, active, DateTime.Now);
        }
    }
}
