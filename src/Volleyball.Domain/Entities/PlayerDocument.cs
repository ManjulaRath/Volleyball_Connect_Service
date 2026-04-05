using System;

namespace Volleyball.Domain.Entities
{
    public class PlayerDocument
    {
        public Guid Id { get; set; }
        public int PlayerId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Player? Player { get; set; }
    }
}
