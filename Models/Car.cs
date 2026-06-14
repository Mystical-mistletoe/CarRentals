using System;
using CarRentals.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.Models
{
    internal class Car
    {
        public int CarId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int YearCar { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public TransmissionType TransmissionType { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public CarStatus StatusCar { get; set; }
    
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
