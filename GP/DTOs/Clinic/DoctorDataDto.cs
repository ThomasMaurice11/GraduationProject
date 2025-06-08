namespace GP.DTOs.Clinic
{
    public class DoctorDataDto
    {

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public int ClinicId { get; set; }
        public string Number { get; set; }

        public string MedicalId { get; set; } // Store image as binary data
    }
}
