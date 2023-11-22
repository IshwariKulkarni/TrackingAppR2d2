using System.ComponentModel.DataAnnotations;

namespace TrackingApp.Entities
{
    public class TrackingDB
    {
        [Key]
        [Required]
        [EmailAddress(ErrorMessage = "Please Enter a valid email")]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Mentor { get; set; }
        [Required]
        public string Course { get; set; }
        [Required]
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public int WarningCode { get; set; }
        [Required]
        public DateTime ExamDate { get; set; }
        public DateTime WarningDateTime { get; set; }
    }
}
