using Transaction.Core.Entities;

namespace Transaction.Service.Services.Interfaces;

public interface ILocationService
{
    Task<string> GetTimeZoneByLocation(string clientLocation, CancellationToken cancellationToken = default);
}