using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Server.Domain.Entities;
using Server.Features.Auth;
using Shared.Core.Auth;
using Xunit;

namespace Unit.Features.Auth;

public class LogoutHandlerTests
{
    private static Mock<SignInManager<ApplicationUser>> CreateMockSignInManager()
    {
        var userManagerMock = CreateMockUserManager();
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var logger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
        return new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            contextAccessor.Object,
            userClaimsPrincipalFactory.Object,
            options,
            logger.Object,
            null!,
            null!);
    }

    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options,
            null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_should_call_SignOutAsync_once()
    {
        var signInManager = CreateMockSignInManager();
        signInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

        var handler = new LogoutHandler(signInManager.Object);
        await handler.Handle(new LogoutCommand());

        signInManager.Verify(x => x.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_should_return_null()
    {
        var signInManager = CreateMockSignInManager();
        signInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

        var handler = new LogoutHandler(signInManager.Object);
        var result = await handler.Handle(new LogoutCommand());

        result.Should().BeNull();
    }
}
