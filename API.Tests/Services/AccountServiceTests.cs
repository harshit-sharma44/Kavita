using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constants;
using API.Data;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace API.Tests.Services;

public class AccountServiceTests
{
    private static (AccountService, AppUser) Setup(params string[] roles)
    {
        var userManager = Substitute.For<UserManager<AppUser>>(Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);
        var user = new AppUser { UserName = "user" };
        userManager.GetRolesAsync(user).Returns(Task.FromResult<IList<string>>(new List<string>(roles)));

        var service = new AccountService(userManager, Substitute.For<ILogger<AccountService>>(), Substitute.For<IUnitOfWork>());
        return (service, user);
    }

    [Fact]
    public async Task CanChangeAgeRestriction_NullUser_ReturnsFalse()
    {
        var (service, _) = Setup();
        Assert.False(await service.CanChangeAgeRestriction(null));
    }

    [Fact]
    public async Task CanChangeAgeRestriction_ChangePasswordWithoutChangeRestriction_ReturnsFalse()
    {
        var (service, user) = Setup(PolicyConstants.LoginRole, PolicyConstants.ChangePasswordRole);
        Assert.False(await service.CanChangeAgeRestriction(user));
    }

    [Fact]
    public async Task CanChangeAgeRestriction_ChangeRestrictionRole_ReturnsTrue()
    {
        var (service, user) = Setup(PolicyConstants.LoginRole, PolicyConstants.ChangeRestrictionRole);
        Assert.True(await service.CanChangeAgeRestriction(user));
    }

    [Fact]
    public async Task CanChangeAgeRestriction_Admin_ReturnsTrue()
    {
        var (service, user) = Setup(PolicyConstants.AdminRole);
        Assert.True(await service.CanChangeAgeRestriction(user));
    }

    [Fact]
    public async Task CanChangeAgeRestriction_ReadOnlyWithChangeRestriction_ReturnsFalse()
    {
        var (service, user) = Setup(PolicyConstants.ChangeRestrictionRole, PolicyConstants.ReadOnlyRole);
        Assert.False(await service.CanChangeAgeRestriction(user));
    }
}
