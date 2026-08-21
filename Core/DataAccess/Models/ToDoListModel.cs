using System;
using LinqToDB.Mapping;

namespace Core.DataAccess.Models
{
    [Table("collections")]
    public class ToDoListModel
    {
        [Column("id"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("userid")]
        public Guid UserId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; }
    }
}