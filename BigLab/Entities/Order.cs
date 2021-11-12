using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BigLab.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public virtual UserInfo UserInfo { get; set; }
        public virtual Game UserGame { get; set; }
    }
}
