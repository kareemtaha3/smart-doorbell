using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces.Services;

public interface IMelodyService
{
    Task<MelodyResponseDto> UploadMelodyAsync(Guid homeId, Guid userId, string name, Stream fileStream, string fileName, bool isMain, CancellationToken cancellationToken = default);
    Task<MelodyResponseDto> SetMainMelodyAsync(Guid homeId, Guid melodyId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MelodyResponseDto>> GetHomeMelodiesAsync(Guid homeId, CancellationToken cancellationToken = default);
}
