using System;
using LinqToDB.Mapping;
using Core.DataAccess.Models;

namespace Infrastructure.DataAccess.Models
{
    [Table("Notifications")]
    public class NotificationModel
    {
        [Column("Id"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("TelegramUserId")]
        public long TelegramUserId { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Column("Text")]
        public string Text { get; set; }

        [Column("ScheduledAt")]
        public DateTime ScheduledAt { get; set; }

        [Column("IsNotified")]
        public bool IsNotified { get; set; }

        [Column("NotifiedAt")]
        public DateTime? NotifiedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; }
    }
}
