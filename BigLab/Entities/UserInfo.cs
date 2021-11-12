using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BigLab.Entities
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool isAdmin { get; set; }
        public virtual List<Game> UserGames { get; set; }
    }
}
