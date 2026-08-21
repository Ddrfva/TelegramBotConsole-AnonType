using System;
using Core.Entities;
using Core.DataAccess.Models;
using Infrastructure.DataAccess.Models;

namespace Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            if (model == null) return null;

            return new ToDoUser
            {
                Id = model.Id,
                TelegramUserId = model.TelegramUserId,
                TelegramUserName = model.TelegramUserName,
                RegisteredAtUtc = model.RegisteredAtUtc
            };
        }

        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            if (entity == null) return null;

            return new ToDoUserModel
            {
                Id = entity.Id,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAtUtc = entity.RegisteredAtUtc
            };
        }

        public static ToDoList MapFromModel(ToDoListModel model)
        {
            if (model == null) return null;

            return new ToDoList
            {
                Id = model.Id,
                Name = model.Name,
                UserId = model.UserId,
                CreatedAt = model.CreatedAt,
                User = model.User != null ? MapFromModel(model.User) : null
            };
        }

        public static ToDoListModel MapToModel(ToDoList entity)
        {
            if (entity == null) return null;

            return new ToDoListModel
            {
                Id = entity.Id,
                Name = entity.Name,
                UserId = entity.UserId,
                CreatedAt = entity.CreatedAt,
                User = entity.User != null ? MapToModel(entity.User) : null
            };
        }

        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            if (model == null) return null;

            return new ToDoItem
            {
                Id = model.Id,
                Name = model.Name,
                Species = model.Species,
                UserId = model.UserId,
                ListId = model.ListId,
                WateringFrequencyDays = model.WateringFrequencyDays,
                LastWateredAt = model.LastWateredAt,
                LightRequirement = model.LightRequirement,
                Notes = model.Notes,
                State = (ToDoItemState)model.State,
                CreatedAtUtc = model.CreatedAtUtc,
                StateChangedAtUtc = model.StateChangedAtUtc,
                Deadline = model.Deadline,
                User = model.User != null ? MapFromModel(model.User) : null,
                List = model.List != null ? MapFromModel(model.List) : null
            };
        }

        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            if (entity == null) return null;

            return new ToDoItemModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Species = entity.Species,
                UserId = entity.UserId,
                ListId = entity.ListId,
                WateringFrequencyDays = entity.WateringFrequencyDays,
                LastWateredAt = entity.LastWateredAt,
                LightRequirement = entity.LightRequirement,
                Notes = entity.Notes,
                State = (int)entity.State,
                CreatedAtUtc = entity.CreatedAtUtc,
                StateChangedAtUtc = entity.StateChangedAtUtc,
                Deadline = entity.Deadline,
                User = entity.User != null ? MapToModel(entity.User) : null,
                List = entity.List != null ? MapToModel(entity.List) : null
            };
        }

        public static Notification MapFromModel(NotificationModel model)
        {
            if (model == null) return null;

            return new Notification
            {
                Id = model.Id,
                UserId = model.UserId,
                TelegramUserId = model.TelegramUserId,
                Type = model.Type,
                Text = model.Text,
                ScheduledAt = model.ScheduledAt,
                IsNotified = model.IsNotified,
                NotifiedAt = model.NotifiedAt
            };
        }

        public static NotificationModel MapToModel(Notification entity)
        {
            if (entity == null) return null;

            return new NotificationModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                Type = entity.Type,
                Text = entity.Text,
                ScheduledAt = entity.ScheduledAt,
                IsNotified = entity.IsNotified,
                NotifiedAt = entity.NotifiedAt
            };
        }
    }
}