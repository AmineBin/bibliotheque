using Moq;
using Xunit;
using Bibliotheque.Api.Models.Entities;
using Bibliotheque.Api.Repositories;

namespace Bibliotheque.Tests.Repositories;

/// <summary>
/// Tests pour les repositories - validation des interfaces
/// </summary>
public class RepositoryInterfaceTests
{
    [Fact]
    public void IBookRepository_DefinesExpectedMethods()
    {
        // Verify that the interface has all expected methods
        var interfaceType = typeof(IBookRepository);
        
        Assert.NotNull(interfaceType.GetMethod("GetAllAsync"));
        Assert.NotNull(interfaceType.GetMethod("GetByIdAsync"));
        Assert.NotNull(interfaceType.GetMethod("CreateAsync"));
        Assert.NotNull(interfaceType.GetMethod("UpdateAsync"));
        Assert.NotNull(interfaceType.GetMethod("DeleteAsync"));
        Assert.NotNull(interfaceType.GetMethod("SearchAsync"));
        Assert.NotNull(interfaceType.GetMethod("UpdateAvailabilityAsync"));
    }

    [Fact]
    public void ILoanRepository_DefinesExpectedMethods()
    {
        var interfaceType = typeof(ILoanRepository);
        
        Assert.NotNull(interfaceType.GetMethod("GetAllAsync"));
        Assert.NotNull(interfaceType.GetMethod("GetByIdAsync"));
        Assert.NotNull(interfaceType.GetMethod("GetByUserIdAsync"));
        Assert.NotNull(interfaceType.GetMethod("CreateAsync"));
        Assert.NotNull(interfaceType.GetMethod("ReturnBookAsync"));
        Assert.NotNull(interfaceType.GetMethod("GetActiveLoanForBookAsync"));
        Assert.NotNull(interfaceType.GetMethod("GetStatsAsync"));
    }

    [Fact]
    public void IUserRepository_DefinesExpectedMethods()
    {
        var interfaceType = typeof(IUserRepository);
        
        Assert.NotNull(interfaceType.GetMethod("GetByEmailAsync"));
        Assert.NotNull(interfaceType.GetMethod("GetByIdAsync"));
        Assert.NotNull(interfaceType.GetMethod("CreateAsync"));
        Assert.NotNull(interfaceType.GetMethod("EmailExistsAsync"));
    }
}
