using System;
using SQLite;

namespace server
{
    public class Session
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string LastAddress { get; set; }
        public int LastPort { get; set; }
        public DateTime DateCreated { get; set; }
    }
}