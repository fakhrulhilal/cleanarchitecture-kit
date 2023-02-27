using DevKit.Application.Ports;

namespace DevKit.Application.Mocks;

using static Testing;

public static class CurrentUserServiceExtensions
{
    public static Mock<ICurrentUserService> AnonymousUser(this Mock<ICurrentUserService> svc) {
        svc.SetupGet(x => x.UserId).Returns(string.Empty);
        return svc;
    }

    public static Mock<ICurrentUserService> AuthenticatedUser(this Mock<ICurrentUserService> svc) {
        svc.SetupGet(x => x.UserId).Returns(Faker.Person.UserName);
        return svc;
    }
}
