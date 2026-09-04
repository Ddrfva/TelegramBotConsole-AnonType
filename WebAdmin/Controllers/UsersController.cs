using Core.DataAccess;
using Core.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBot_31.WebAdmin.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;
        private readonly IToDoListRepository _listRepository;
        private readonly INotificationService _notificationService;

        public UsersController(
            IUserRepository userRepository,
            IToDoRepository toDoRepository,
            IToDoListRepository listRepository,
            INotificationService notificationService)
        {
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
            _listRepository = listRepository;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetUsers(CancellationToken.None);
            return View(users);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userRepository.GetUser(id, CancellationToken.None);
            if (user == null)
                return NotFound();

            var plants = await _toDoRepository.GetAllByUserId(id, CancellationToken.None);
            var lists = await _listRepository.GetByUserId(id, CancellationToken.None);
            var notifications = await _notificationService.GetScheduledNotifications(DateTime.UtcNow, CancellationToken.None);

            ViewBag.Plants = plants;
            ViewBag.Lists = lists;
            ViewBag.Notifications = notifications;
            return View(user);
        }
    }
}