using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;
using Shared.Core;
using Shared.Core.Auth;

namespace Server.Features.Auth;

public class LogoutHandler : IRequestHandler<LogoutCommand, object?>
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutHandler(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<object?> Handle(LogoutCommand request, CancellationToken cancellationToken = default)
    {
        await _signInManager.SignOutAsync();
        return null;
    }
}
