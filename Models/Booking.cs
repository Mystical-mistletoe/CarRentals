using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRentals.Enums;


namespace CarRentals.Models
{
    internal class Booking
    {
        public int BookingId { get; set; }

        public DateTime StartDateTime { get; set; } 
        public DateTime EndDateTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus StatusBooking { get; set; }
        public int? Mileage { get; set; }

        //FK
        public int CarId { get; set; }
        public int UserId { get; set; }
        

        //
        public Car Car { get; set; } = null!; //автоматически заполнит
        public virtual User User { get; set; }
        public ICollection<Fine> Fines { get; set; } = new List<Fine>();
        //в лист бокс
        public string StartDateOnly => StartDateTime.ToString("dd.MM.yyyy");

    }
}
