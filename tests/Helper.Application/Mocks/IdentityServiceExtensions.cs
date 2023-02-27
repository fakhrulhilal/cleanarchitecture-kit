using DevKit.Application.Ports;

namespace DevKit.Application.Mocks;

public static class IdentityServiceExtensions
{
    public static Mock<IIdentityService> GrantedByRole(this Mock<IIdentityService> svc, string? role = null) {
        if (string.IsNullOrWhiteSpace(role))
            svc.Setup(x => x.IsInRoleAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        else
            svc.Setup(x => x.IsInRoleAsync(It.IsAny<string>(), role)).ReturnsAsync(true);
        return svc;
    }

    public static Mock<IIdentityService> GrantedByPolicy(this Mock<IIdentityService> svc,
        string? role = null) {
        if (string.IsNullOrWhiteSpace(role))
            svc.Setup(x => x.AuthorizeAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        else
            svc.Setup(x => x.AuthorizeAsync(It.IsAny<string>(), role)).ReturnsAsync(true);
        return svc;
    }

    public static Mock<IIdentityService> NotInRole(this Mock<IIdentityService> svc) {
        svc.Setup(x => x.IsInRoleAsync(This.EmptyString(), It.IsAny<string>())).ReturnsAsync(false);
        return svc;
    }
    
    public static Mock<IIdentityService> NotInPolicy(this Mock<IIdentityService> svc) {
        svc.Setup(x => x.AuthorizeAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        return svc;
    }
    
    public static Mock<IIdentityService> NotGrantedForAnonymous(this Mock<IIdentityService> svc) {
        svc.Setup(x => x.AuthorizeAsync(This.EmptyString(), It.IsAny<string>())).ReturnsAsync(false);
        svc.Setup(x => x.IsInRoleAsync(This.EmptyString(), It.IsAny<string>())).ReturnsAsync(false);
        return svc;
    }
}
