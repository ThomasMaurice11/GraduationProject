using GP.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Clinic
{
    public class AddClinicDto
    {




        public string Name { get; set; }

        public string Address { get; set; }

        public string LocationUrl { get; set; }
        public string Details { get; set; }

        public string Number { get; set; }
        [EmailAddress]
        public string CLinicEmail { get; set; }

    }
}
