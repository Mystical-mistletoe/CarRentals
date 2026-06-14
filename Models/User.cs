using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.Models
{
    internal class User
    {
        public int UserId { get; set; }
        public string LastName { get; set; } 
        public string Email { get; set; }   = string.Empty;

        //у пользователя может быть много бронирований
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
