using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;

        public PointsOfInterestController (ILogger<PointsOfInterestController> logger,
            IMailService mailService)
        {
            _logger = logger;
            _mailService = mailService;
        }

        // --------------  Get - Search  ---------------

        [HttpGet("{cityId}/pointsofinterest")]
        public IActionResult GetPointsOfInterest (int cityId)
        {
            try
            {
                var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

                if (city == null)
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accesing point of interest.");
                    return NotFound();
                }

                return Ok(city.PointsOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
            
        }

        [HttpGet("{cityId}/pointsofinterest/{id}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest( int cityId, int id)
        {
            var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterest = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);

            if (pointOfInterest == null)
            {
                return NotFound();
            }
            return Ok(pointOfInterest);
        }
        
        // -------------------  create a item   --------------------

        [HttpPost("{cityId}/pointsofinterest")]
        public IActionResult CreatePointOfInterest (int cityId, 
            [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            if(pointOfInterest == null)
            {
                return BadRequest();
            }

            if(pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", " The provide description should be different from the name.");
            }

            if (!ModelState.IsValid)
            {
                // Return error of input validate and pass the error message of PointOfInterestForCreationDto
                return BadRequest(ModelState);
            }

            var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            // calculate the max value in the id of cities
            var maxPointOfInterestId = CityDataStore.Current.Cities.SelectMany(
                c => c.PointsOfInterest).Max(p => p.Id);

            var finalPointOfInterest = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new
            { cityId = cityId, id = finalPointOfInterest.Id}, finalPointOfInterest);
        }

        //  --------------   Total  Update    ---------------

        [HttpPut("{cityId}/pointsofinterest/{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id,
            [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", " The provide description should be different from the name.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(
                p => p.Id == id);

            if(pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            // the  request is complete successfully
            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore.Description = pointOfInterest.Description;
            return NoContent();
        }

        //  ------------------   Partial Update    --------------------

        [HttpPatch("{cityId}/pointsofinterest/{id}")]
        public IActionResult PartialUpdatePointOfInterest(int cityId, int id,
            [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(
                p => p.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description
            };
            patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // validate not remove - error
            if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            TryValidateModel(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

            // -----------------  delete  ----------------------

            [HttpDelete("{cityId}/pointsofinterest/{id}")]
            public IActionResult DeletePointOfInterest(int cityId, int id)
            {
                var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

                if (city == null)
                {
                    return NotFound();
                }

                var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(
                    c => c.Id == id);

                if (pointOfInterestFromStore == null)
                {
                    return NotFound();
                }

                city.PointsOfInterest.Remove(pointOfInterestFromStore);

                _mailService.Send("Point of interest deleted.", 
                    $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was declared.");

                return NoContent();

            }

        


    }
}
