using System.Linq;
using System.Threading.Tasks;
using QRCoder;
using System.IO;
using Volleyball.Application.Interfaces;
using Volleyball.Domain.Entities;

namespace Volleyball.Application.Services
{
    public class PlayerService
    {
        private readonly IPlayerRepository _repo;
        public PlayerService(IPlayerRepository repo) { _repo = repo; }

        public async Task RegisterAsync(Player p)
        {
            // generate a simple registration number if not provided
            if (string.IsNullOrEmpty(p.RegistrationNumber))
            {
                p.RegistrationNumber = $"REG-{System.DateTime.UtcNow:yyyyMMddHHmmss}-{System.Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            }

            // generate QR code as base64 PNG and store in QrCode property
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(p.RegistrationNumber, QRCodeGenerator.ECCLevel.Q);
                var png = new PngByteQRCode(qrData).GetGraphic(20);
                var base64 = Convert.ToBase64String(png);
                p.QrCode = $"data:image/png;base64,{base64}";
            }
            catch
            {
                // on any QR generation failure, leave QrCode empty
                p.QrCode = string.Empty;
            }

            await _repo.AddAsync(p);
        }

        public IQueryable<Player> Search(string? q, string? position, string? gender, int? minAge, int? maxAge)
        {
            var query = _repo.Query();
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => p.FirstName.Contains(q) || p.LastName.Contains(q) || p.Email.Contains(q));
            }
            if (!string.IsNullOrEmpty(position)) query = query.Where(p => p.Position == position);
            if (!string.IsNullOrEmpty(gender)) query = query.Where(p => p.Gender == gender);
            if (minAge.HasValue)
            {
                var maxDob = System.DateTime.UtcNow.AddYears(-minAge.Value);
                query = query.Where(p => p.DateOfBirth <= maxDob);
            }
            if (maxAge.HasValue)
            {
                var minDob = System.DateTime.UtcNow.AddYears(-maxAge.Value - 1).AddDays(1);
                query = query.Where(p => p.DateOfBirth >= minDob);
            }
            return query;
        }
    }
}
