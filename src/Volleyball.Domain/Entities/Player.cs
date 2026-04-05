using System;

namespace Volleyball.Domain.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Additional fields
        public string NicNumber { get; set; } = string.Empty;
        public string PostalId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string NameWithInitials { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public int? AgeGroupId { get; set; } = 1;
        public decimal? HeightCm { get; set; } = 0;
        public decimal? WeightKg { get; set; } = 0;
        public string ClubTeam { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public int Status { get; set; } = 1; // 1 = Active, 0 = Inactive
        public string PhotoUrl { get; set; } = string.Empty;
        // Stored as a data URL (base64 PNG) or URL to image
        public string QrCode { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
    }
}
