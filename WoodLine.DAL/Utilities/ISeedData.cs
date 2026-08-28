using Microsoft.Extensions.DependencyInjection;

namespace WoodLine.DAL.Utilities;

public interface ISeedData
{
    Task DataSeedingAsync();
    Task IdentityDataSeedingAsync();
}
