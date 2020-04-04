using System;
using SQLite;

namespace server
{
    public class User
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string Password { get; set; }
    }
}