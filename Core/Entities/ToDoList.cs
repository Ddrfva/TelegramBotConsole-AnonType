using Core.Entities;

public class ToDoList
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ToDoUser User { get; set; }
}