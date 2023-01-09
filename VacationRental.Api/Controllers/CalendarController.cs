using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public CalendarController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        /// <summary>
        /// Gets calendar information.
        /// </summary>
        /// <param name="rentalId" example="1">The rental id</param>
        /// <param name="start" example="2023-01-09">The start date of the calendar</param>
        /// <param name="nights" example="3">The number of nights to see information</param>
        /// <returns>Calendar Info</returns>
        /// <response code="200">Calendar information returned</response>
        /// <response code="500">Error in getting calendar information</response>
        [HttpGet]
        [Produces("application/json")]
        public CalendarViewModel Get(int rentalId, DateTime start, int nights)
        {
            if (nights <= 0)
                throw new ApplicationException("Nights must be positive");
            if (rentalId <= 0)
                throw new ApplicationException("RentalId must be positive");
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            var result = new CalendarViewModel 
            {
                RentalId = rentalId,
                Dates = new List<CalendarDateViewModel>() 
            };
            for (var i = 0; i < nights; i++)
            {
                var date = new CalendarDateViewModel
                {
                    Date = start.Date.AddDays(i),
                    Bookings = new List<CalendarBookingViewModel>(),
                    PreparationTimes = new List<CalendarPreparationTimeViewModel>()
                };

                foreach (var booking in _bookings.Values)
                {
                    if (booking.RentalId == rentalId
                        && booking.Start <= date.Date 
                        && booking.Start.AddDays(booking.Nights) > date.Date)
                    {
                        date.Bookings.Add(new CalendarBookingViewModel { Id = booking.Id, Unit = booking.Unit });
                    }

                    if (booking.RentalId == rentalId
                        && booking.Start.AddDays(booking.Nights) <= date.Date
                        && booking.Start.AddDays(booking.Nights + booking.PreparationTimeInDays) > date.Date)
                    {
                        date.PreparationTimes.Add(new CalendarPreparationTimeViewModel { Unit = booking.Unit });
                    }
                }

                result.Dates.Add(date);
            }

            return result;
        }
    }
}
