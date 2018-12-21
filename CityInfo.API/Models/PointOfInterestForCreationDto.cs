using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Models
{
    public class PointOfInterestForCreationDto
    {
        // with required - validate the input doesn't empty
        [Required (ErrorMessage ="You should provide a name value.")]
        [MaxLength(50)]  // validate max length of the name is 50
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

    }
}
