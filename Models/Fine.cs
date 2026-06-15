using CarRentals.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.Models
{
    internal class Fine
    {
        public int FineId { get; set; }

        public FineType FineType { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; } //? может быть null

        public DateTime IssueDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        public FineStatus StatusFine { get; set; }

        //FK
        public int BookingId { get; set; }

        //
        public Booking Booking { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string DisplayName => $"{GetFineTypeName()} — {IssueDate:dd.MM.yyyy HH:mm} — {Booking?.User?.LastName}";

        private string GetFineTypeName()
        {
            switch (FineType)
            {
                case FineType.Speeding: return "Превышение скорости";
                case FineType.Parking: return "Неправильная парковка";
                case FineType.WrongRed: return "Проезд на красный";
                case FineType.Damage: return "Повреждение авто";
                case FineType.Another: return "Другое";
                default: return FineType.ToString();
            }
        }
    }
}

    
