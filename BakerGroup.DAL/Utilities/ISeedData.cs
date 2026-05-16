using Microsoft.Extensions.DependencyInjection;

namespace BakerGroup.DAL.Utilities;

public interface ISeedData
{
    Task DataSeedingAsync();
    Task IdentityDataSeedingAsync();
}
