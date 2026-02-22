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

public class LoginHandlerTests
{
    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static Mock<SignInManager<ApplicationUser>> CreateMockSignInManager(Mock<UserManager<ApplicationUser>> userManagerMock)
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>();
        return new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            contextAccessor.Object,
            userClaimsPrincipalFactory.Object,
            options,
            logger.Object,
            null!,
            null!);
    }

    [Fact]
    public async Task Handle_null_email_should_throw_UnauthorizedAccessException()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var handler = new LoginHandler(signInManager.Object, userManager.Object);

        var act = () => handler.Handle(new LoginRequest { Email = "", Password = "Pass123!" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email and password are required.");
    }

    [Fact]
    public async Task Handle_null_password_should_throw_UnauthorizedAccessException()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var handler = new LoginHandler(signInManager.Object, userManager.Object);

        var act = () => handler.Handle(new LoginRequest { Email = "a@b.com", Password = "" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email and password are required.");
    }

    [Fact]
    public async Task Handle_user_not_found_should_throw_UnauthorizedAccessException()
    {
        var userManager = CreateMockUserManager();
        userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        var signInManager = CreateMockSignInManager(userManager);
        var handler = new LoginHandler(signInManager.Object, userManager.Object);

        var act = () => handler.Handle(new LoginRequest { Email = "unknown@b.com", Password = "Pass123!" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_wrong_password_should_throw_UnauthorizedAccessException()
    {
        var user = new ApplicationUser { Id = "user-1", Email = "u@b.com", UserName = "u@b.com" };
        var userManager = CreateMockUserManager();
        userManager.Setup(x => x.FindByEmailAsync("u@b.com")).ReturnsAsync(user);
        var signInManager = CreateMockSignInManager(userManager);
        signInManager.Setup(x => x.PasswordSignInAsync(user, "WrongPass", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);
        var handler = new LoginHandler(signInManager.Object, userManager.Object);

        var act = () => handler.Handle(new LoginRequest { Email = "u@b.com", Password = "WrongPass" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_valid_credentials_should_return_UserInfo()
    {
        var user = new ApplicationUser { Id = "user-1", Email = "u@b.com", UserName = "u@b.com" };
        var userManager = CreateMockUserManager();
        userManager.Setup(x => x.FindByEmailAsync("u@b.com")).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
        userManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<System.Security.Claims.Claim>());

        var signInManager = CreateMockSignInManager(userManager);
        signInManager.Setup(x => x.PasswordSignInAsync(user, "Pass123!", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var handler = new LoginHandler(signInManager.Object, userManager.Object);

        var result = await handler.Handle(new LoginRequest { Email = "u@b.com", Password = "Pass123!", RememberMe = false });

        result.Should().NotBeNull();
        result.UserId.Should().Be("user-1");
        result.Email.Should().Be("u@b.com");
        result.Roles.Should().ContainSingle("Admin");
    }
}
