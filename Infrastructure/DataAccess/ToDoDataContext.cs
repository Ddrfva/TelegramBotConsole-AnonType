using LinqToDB;
using LinqToDB.Data;
using Core.DataAccess.Models;

namespace Infrastructure.DataAccess
{
    public class ToDoDataContext : DataConnection
    {
        public ToDoDataContext(string connectionString)
            : base(ProviderName.PostgreSQL, connectionString)
        {
        }

        public ITable<ToDoUserModel> Users => this.GetTable<ToDoUserModel>();
        public ITable<ToDoListModel> Lists => this.GetTable<ToDoListModel>();
        public ITable<ToDoItemModel> Items => this.GetTable<ToDoItemModel>();
    }
}