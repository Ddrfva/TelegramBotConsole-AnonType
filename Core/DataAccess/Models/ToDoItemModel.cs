using System;
using LinqToDB.Mapping;

namespace Core.DataAccess.Models
{
    [Table("flowers")]
    public class ToDoItemModel
    {
        [Column("id"), PrimaryKey]  // ← нижний регистр!
        public Guid Id { get; set; }

        [Column("name")]  // ← нижний регистр!
        public string Name { get; set; }

        [Column("species")]  // ← нижний регистр!
        public string Species { get; set; }

        [Column("userid")]  // ← ИСПРАВЛЕНО!
        public Guid UserId { get; set; }

        [Column("collectionid")]  // ← ИСПРАВЛЕНО!
        public Guid? ListId { get; set; }

        [Column("wateringfrequencydays")]  // ← нижний регистр!
        public int? WateringFrequencyDays { get; set; }

        [Column("lastwateredat")]  // ← нижний регистр!
        public DateTime? LastWateredAt { get; set; }

        [Column("lightrequirement")]  // ← нижний регистр!
        public string LightRequirement { get; set; }

        [Column("notes")]  // ← нижний регистр!
        public string Notes { get; set; }

        [Column("state")]  // ← нижний регистр!
        public int State { get; set; }

        [Column("createdatutc")]  // ← нижний регистр!
        public DateTime CreatedAtUtc { get; set; }

        [Column("statechangedatutc")]  // ← нижний регистр!
        public DateTime? StateChangedAtUtc { get; set; }

        [Column("deadline")]  // ← нижний регистр!
        public DateTime? Deadline { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; }

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel List { get; set; }
    }
}