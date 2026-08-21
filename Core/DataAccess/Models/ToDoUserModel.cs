using System;
using LinqToDB.Mapping;

namespace Core.DataAccess.Models
{
    [Table("users")]
    public class ToDoUserModel
    {
        [Column("id"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("telegramuserid")]
        public long TelegramUserId { get; set; }

        [Column("telegramusername")]
        public string TelegramUserName { get; set; }

        [Column("registeredatutc")]
        public DateTime RegisteredAtUtc { get; set; }
    }
}