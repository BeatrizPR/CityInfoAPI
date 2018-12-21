using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
        
    [Route ("api/cities")]
    public class CitiesControllers : Controller
    {
        [HttpGet()]
        public IActionResult GetCities()
        {
            return Ok( CityDataStore.Current.Cities);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity( int id)
        {
            //find city
            var cityToReturn = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);
            if (cityToReturn == null)
            {
                // method NotFound and Ok of both return are of IActionResult (of system Microsoft.ASP.Net)
                return NotFound();

            }
            // return 500 error intern server
            return Ok(cityToReturn);
        }








    }
}
