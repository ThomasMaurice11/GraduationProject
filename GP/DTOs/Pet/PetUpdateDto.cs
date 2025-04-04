﻿using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Pet
{
    public class PetUpdateDto
    {
     
        public string Name { get; set; }

        
        public int Age { get; set; }

   
        public string Breed { get; set; }

        
        public string Gender { get; set; }

     
        public string HealthStatus { get; set; }


        public List<IFormFile> Photos { get; set; }



    }
}
