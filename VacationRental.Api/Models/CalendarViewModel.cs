using System.Collections.Generic;

namespace VacationRental.Api.Models
{
    public class CalendarViewModel
    {
        /// <example>1</example>
        public int RentalId { get; set; }
        public List<CalendarDateViewModel> Dates { get; set; }
    }
}
