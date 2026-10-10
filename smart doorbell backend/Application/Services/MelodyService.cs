using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Services;

public class MelodyService : IMelodyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMinioStorageService _minioService;

    public MelodyService(IUnitOfWork unitOfWork, IMinioStorageService minioService)
    {
        _unitOfWork = unitOfWork;
        _minioService = minioService;
    }

    public async Task<MelodyResponseDto> UploadMelodyAsync(
        Guid homeId,
        Guid userId,
        string name,
        Stream fileStream,
        string fileName,
        bool isMain,
        CancellationToken cancellationToken = default)
    {
        var ownerUserId = await _unitOfWork.Homes.GetOwnerUserIdAsync(homeId, cancellationToken);
        if (!ownerUserId.HasValue)
        {
            throw new KeyNotFoundException("Home not found.");
        }

        if (ownerUserId.Value != userId)
        {
            throw new InvalidOperationException("Only the home owner can add melodies.");
        }

        var bucket = "doorbell-melodies";
        await _minioService.EnsureBucketExistsAsync(bucket, cancellationToken);

        var objectKey = $"melodies/{homeId}/{Guid.NewGuid()}_{fileName}";
        var toneFileUrl = await _minioService.UploadFileAsync(fileStream, objectKey, bucket, "audio/mpeg", cancellationToken);

        if (isMain)
        {
            await _unitOfWork.ChimeMelodies.UnsetMainMelodiesForHomeAsync(homeId, cancellationToken);
        }

        var melody = new ChimeMelody
        {
            Id = Guid.NewGuid(),
            HomeId = homeId,
            Name = name.Trim(),
            ToneFileUrl = toneFileUrl,
            IsMain = isMain,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChimeMelodies.AddAsync(melody, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MelodyResponseDto(
            melody.Id,
            melody.HomeId,
            melody.Name,
            melody.ToneFileUrl,
            melody.IsMain,
            melody.CreatedAt
        );
    }

    public async Task<MelodyResponseDto> SetMainMelodyAsync(
        Guid homeId,
        Guid melodyId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var ownerUserId = await _unitOfWork.Homes.GetOwnerUserIdAsync(homeId, cancellationToken);
        if (!ownerUserId.HasValue)
        {
            throw new KeyNotFoundException("Home not found.");
        }

        if (ownerUserId.Value != userId)
        {
            throw new InvalidOperationException("Only the home owner can set the main melody.");
        }

        var melody = await _unitOfWork.ChimeMelodies.GetByIdAsync(melodyId, cancellationToken);
        if (melody == null || melody.HomeId != homeId)
        {
            throw new KeyNotFoundException("Melody not found for this home.");
        }

        await _unitOfWork.ChimeMelodies.UnsetMainMelodiesForHomeAsync(homeId, cancellationToken);
        melody.IsMain = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MelodyResponseDto(
            melody.Id,
            melody.HomeId,
            melody.Name,
            melody.ToneFileUrl,
            melody.IsMain,
            melody.CreatedAt
        );
    }

    public async Task<IEnumerable<MelodyResponseDto>> GetHomeMelodiesAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        var melodies = await _unitOfWork.ChimeMelodies.GetByHomeIdAsync(homeId, cancellationToken);
        return melodies.Select(m => new MelodyResponseDto(
            m.Id,
            m.HomeId,
            m.Name,
            m.ToneFileUrl,
            m.IsMain,
            m.CreatedAt
        ));
    }
}
