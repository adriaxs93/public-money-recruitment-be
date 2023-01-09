using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public BookingsController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        /// <summary>
        /// Gets booking information.
        /// </summary>
        /// <param name="bookingId" example="1">The booking id</param>
        /// <returns>Booking Info</returns>
        /// <response code="200">Booking information returned</response>
        /// <response code="500">Error in getting booking information</response>
        [HttpGet]
        [Route("{bookingId:int}")]
        [Produces("application/json")]
        public BookingViewModel Get(int bookingId)
        {
            if (!_bookings.ContainsKey(bookingId))
                throw new ApplicationException("Booking not found");

            return _bookings[bookingId];
        }

        /// <summary>
        /// Creates a booking.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Booking Id</returns>
        /// <response code="200">Booking created successfully</response>
        /// <response code="500">Error in booking creation</response>
        [HttpPost]
        [Produces("application/json")]
        public ResourceIdViewModel Post(BookingBindingModel model)
        {
            if (model.Nights <= 0)
                throw new ApplicationException("Nights must be positive");
            if (model.RentalId <= 0)
                throw new ApplicationException("RentalId must be positive");
            if (!_rentals.ContainsKey(model.RentalId))
                throw new ApplicationException("Rental not found");

            int rentalPreparationTime = _rentals[model.RentalId].PreparationTimeInDays;

            BookingViewModel newBooking = new BookingViewModel
            {
                Id = 0,
                Nights = model.Nights,
                RentalId = model.RentalId,
                Start = model.Start.Date,
                PreparationTimeInDays = rentalPreparationTime,
                End = model.Start.Date.AddDays(model.Nights),
                EndPreparation = model.Start.AddDays(model.Nights + rentalPreparationTime)
            };

            if (!newBooking.CheckAvailableUnits(_bookings, _rentals, true))
                throw new ApplicationException("Not available");

            var key = new ResourceIdViewModel { Id = _bookings.Keys.Count + 1 };
            newBooking.Id = key.Id;

            _bookings.Add(newBooking.Id, newBooking);

            return key;
        }      
    }
}
