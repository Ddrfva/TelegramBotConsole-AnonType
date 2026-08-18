using System;
using Core.Entities;
using Core.DataAccess.Models;

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
                User = entity.User != null ? MapToModel(entity.User) : null,
                List = entity.List != null ? MapToModel(entity.List) : null
            };
        }
    }
}