# 🤖 Асинхронный консольный бот

## 📋 Описание
Проект представляет собой асинхронного консольного бота для управления задачами. 
Разработан на основе предыдущих ДЗ с добавлением: асинхронных методов (async/Task, CancellationToken), обработки ошибок через HandleErrorAsync, использования CancellationTokenSource для управления временем жизни бота.

## 🚀 Основные изменения (ДЗ №7)

### 1. Асинхронность интерфейсов и сервисов
Все методы в IUserRepository, IToDoRepository, IUserService, IToDoService, IToDoReportService переведены на Task<T> и принимают CancellationToken. 

Пример (IUserRepository.cs): 
Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken); 
Task Add(ToDoUser user, CancellationToken cancellationToken);

### 2. Обработка ошибок
Реализован метод HandleErrorAsync в UpdateHandler.cs, который выводит ошибки в консоль. 

public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) 
{ 
    Console.WriteLine($"[ОШИБКА] {exception.Message}"); 
    Console.WriteLine(exception.StackTrace); 
    return Task.CompletedTask; 
}

### 3. Управление токеном отмены
В Program.cs добавлен CancellationTokenSource, который передаётся в botClient.StartReceiving(). 

using var cts = new CancellationTokenSource(); 
botClient.StartReceiving(updateHandler, cts.Token);

## ✅ Критерии выполнения
- Подключена асинхронная библиотека, CancellationToken, HandleErrorAsync
- Все интерфейсы и сервисы переведены на async/Task/CancellationToken

## 👤 Автор
Дорофеева Дарья  
Дата: 20.05.2026  

## 📄 Лицензия
MIT License