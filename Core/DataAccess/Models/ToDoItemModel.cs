using System;
using LinqToDB.Mapping;

namespace Core.DataAccess.Models
{
    [Table("flowers")]
    public class ToDoItemModel
    {
        [Column("id"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("species")]
        public string Species { get; set; }

        [Column("userid")]
        public Guid UserId { get; set; }

        [Column("collectionid")]
        public Guid? ListId { get; set; }

        [Column("wateringfrequencydays")]
        public int? WateringFrequencyDays { get; set; }

        [Column("lastwateredat")]
        public DateTime? LastWateredAt { get; set; }

        [Column("lightrequirement")]
        public string LightRequirement { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("state")]
        public int State { get; set; }

        [Column("createdatutc")]
        public DateTime CreatedAtUtc { get; set; }

        [Column("statechangedatutc")]
        public DateTime? StateChangedAtUtc { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; }

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel List { get; set; }
    }
}