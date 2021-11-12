using System.Collections.Generic;
using BigLab.Entities;

namespace BigLab.Models
{
    public class IndexModel
    {
        public UserInfo User { get; set; }
        public List<Game> Games { get; set; }
    }
}