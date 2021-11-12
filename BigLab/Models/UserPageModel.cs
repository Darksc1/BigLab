using System.Collections.Generic;
using BigLab.Entities;

namespace BigLab.Models
{
    public class UserPageModel
    {
        public UserInfo User { get; set; }
        public List<UserInfo> Users { get; set; }
        
        public List<Order> Orders { get; set; }
    }
}