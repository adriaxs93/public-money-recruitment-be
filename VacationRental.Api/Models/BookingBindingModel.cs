using System;

namespace VacationRental.Api.Models
{
    public class BookingBindingModel
    {
        /// <example>1</example>
        public int RentalId { get; set; }

        /// <example>2023-01-11</example>
        public DateTime Start
        {
            get => _startIgnoreTime;
            set => _startIgnoreTime = value.Date;
        }

        private DateTime _startIgnoreTime;

        /// <example>3</example>
        public int Nights { get; set; }
    }
}
