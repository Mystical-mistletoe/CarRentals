using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.Models
{
    public class AdAccount
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty; // не null
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; }  // "admin" или "manager"
    }
}
