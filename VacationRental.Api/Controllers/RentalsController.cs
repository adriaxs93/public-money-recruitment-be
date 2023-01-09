using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;
using System.Linq;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public RentalsController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        /// <summary>
        /// Gets rental information.
        /// </summary>
        /// <param name="rentalId" example="1">The rental id</param>
        /// <returns>Rental Info</returns>
        /// <response code="200">Rental information returned</response>
        /// <response code="500">Error in getting rental information</response>
        [HttpGet]
        [Route("{rentalId:int}")]
        [Produces("application/json")]
        public RentalViewModel Get(int rentalId)
        {
            if (rentalId <= 0)
                throw new ApplicationException("Rental Id must be positive");
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            return _rentals[rentalId];
        }

        /// <summary>
        /// Creates a rental.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Rental Id</returns>
        /// <response code="200">Rental created successfully</response>
        /// <response code="500">Error in rental creation</response>
        [HttpPost]
        [Produces("application/json")]
        public ResourceIdViewModel Post(RentalBindingModel model)
        {
            if (model.Units <= 0)
                throw new ApplicationException("Rental Units must be positive");
            if (model.PreparationTimeInDays < 0)
                throw new ApplicationException("Rental PreparationTimeInDays cannot be negative");

            var key = new ResourceIdViewModel { Id = _rentals.Keys.Count + 1 };

            _rentals.Add(key.Id, new RentalViewModel
            {
                Id = key.Id,
                Units = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays
            });

            return key;
        }

        /// <summary>
        /// Updates a rental.
        /// </summary>
        /// <param name="rentalId" example="1">The rental id</param>
        /// <param name="model"></param>
        /// <returns>Rental Id</returns>
        /// <response code="200">Rental updated successfully</response>
        /// <response code="500">Error in rental update</response>
        [HttpPut]
        [Route("{rentalId:int}")]
        [Produces("application/json")]
        public ResourceIdViewModel Put(int rentalId, RentalBindingModel model)
        {
            if (rentalId <= 0)
                throw new ApplicationException("Rental id must be positive");
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            if (model.Units <= 0)
                throw new ApplicationException("Rental Units must be positive");
            if (model.PreparationTimeInDays < 0)
                throw new ApplicationException("Rental PreparationTimeInDays cannot be negative");

            int oldUnits = _rentals[rentalId].Units;
            int oldPreparationTimeInDays = _rentals[rentalId].PreparationTimeInDays;

            if (oldUnits != model.Units 
                || oldPreparationTimeInDays != model.PreparationTimeInDays)
            {
                if (model.Units < oldUnits
                    || model.PreparationTimeInDays > oldPreparationTimeInDays)
                {
                    _rentals[rentalId].Units = model.Units;
                    _rentals[rentalId].PreparationTimeInDays = model.PreparationTimeInDays;

                    if (!CheckUpdateRental(rentalId, model))
                    {
                        _rentals[rentalId].Units = oldUnits;
                        _rentals[rentalId].PreparationTimeInDays = oldPreparationTimeInDays;

                        if (model.PreparationTimeInDays != oldPreparationTimeInDays)
                        {
                            model.PreparationTimeInDays = oldPreparationTimeInDays;
                            IDictionary<int, BookingViewModel> bookingsRental = _bookings.Where(p => p.Value.RentalId == rentalId).ToDictionary(p => p.Key, p => p.Value);
                            this.UpdateBookings(rentalId, model, bookingsRental);
                        }

                        throw new ApplicationException("Rental cannot be modified by these parameters");
                    }
                }
                else
                {
                    _rentals[rentalId].Units = model.Units;
                    _rentals[rentalId].PreparationTimeInDays = model.PreparationTimeInDays;

                    IDictionary<int, BookingViewModel> bookingsRental = _bookings.Where(p => p.Value.RentalId == rentalId).ToDictionary(p => p.Key, p => p.Value);
                    this.UpdateBookings(rentalId, model, bookingsRental);
                }
            }
           

            var key = new ResourceIdViewModel { Id = rentalId };

            return key;
        }

        internal bool CheckUpdateRental(int rentalId, RentalBindingModel model)
        {
            IDictionary<int, BookingViewModel> bookingsRental = _bookings.Where(p => p.Value.RentalId == rentalId).ToDictionary(p => p.Key, p => p.Value);

            this.UpdateBookings(rentalId, model, bookingsRental);

            foreach (BookingViewModel booking in bookingsRental.Values)
            {
                IDictionary<int, BookingViewModel> bookingsExcludingBooking = bookingsRental.Where(p => p.Value.Id != booking.Id).ToDictionary(p => p.Key, p => p.Value);

                if (!booking.CheckAvailableUnits(bookingsExcludingBooking, _rentals))
                    return false;
            }

            return true;
        }

        internal void UpdateBookings(int rentalId, RentalBindingModel model, IDictionary<int, BookingViewModel> bookings = null)
        {
            if (bookings == null)
                bookings = _bookings;

            foreach (BookingViewModel booking in bookings.Values)
            {
                if (booking.RentalId == rentalId)
                {
                    booking.PreparationTimeInDays = model.PreparationTimeInDays;
                    booking.EndPreparation = booking.Start.Date.AddDays(booking.Nights + model.PreparationTimeInDays);
                }
            }
        }
    }
}
