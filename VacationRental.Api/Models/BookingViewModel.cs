using System;
using System.Collections.Generic;

namespace VacationRental.Api.Models
{
    public class BookingViewModel
    {
        /// <example>1</example>
        public int Id { get; set; }
        /// <example>1</example>
        public int RentalId { get; set; }
        /// <example>2023-01-11</example>
        public DateTime Start { get; set; }
        /// <example>3</example>
        public int Nights { get; set; }
        /// <example>2</example>
        public int PreparationTimeInDays { get; set; }
        /// <example>2</example>
        public int Unit { get; set; }
        /// <example>2023-01-14</example>
        public DateTime End { get; set; }
        /// <example>2023-01-16</example>
        public DateTime EndPreparation { get; set; }

        public bool OverlapBookings(BookingViewModel existentBooking)
        {
            if (this.RentalId == existentBooking.RentalId
                && 
                    (   (this.Start.Date >= existentBooking.Start.Date && this.Start.Date < existentBooking.EndPreparation.Date)
                        || (this.EndPreparation > existentBooking.Start && this.EndPreparation.Date <= existentBooking.EndPreparation.Date)
                        || (this.Start.Date < existentBooking.Start.Date && this.EndPreparation.Date > existentBooking.EndPreparation.Date))
                    )
            {
                return true;
            }

            return false;
        }

        public bool CheckAvailableUnits(IDictionary<int, BookingViewModel> _bookings, IDictionary<int, RentalViewModel> _rentals, bool assignRentalUnit = false)
        {
            int rentalUnits = _rentals[this.RentalId].Units;
            List<int> rentalUnitsAssigned = new List<int>();

            var count = 0;
            foreach (var booking in _bookings.Values)
            {
                if (this.OverlapBookings(booking))
                {
                    if (!rentalUnitsAssigned.Contains(booking.Unit))
                        rentalUnitsAssigned.Add(booking.Unit);

                    count++;
                }  
            }

            if (count >= rentalUnits)
                return false;

            if (assignRentalUnit)
                this.AssignRentalUnit(rentalUnitsAssigned, rentalUnits);

            return true;
        }

        public void AssignRentalUnit(List<int> rentalUnitsAssigned, int rentalUnits)
        {
            for (var i = 1; i <= rentalUnits; i++)
            {
                if (!rentalUnitsAssigned.Contains(i))
                {
                    this.Unit = i;
                    break;
                }
            }
        }
    }
}
