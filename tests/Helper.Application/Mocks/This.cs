namespace DevKit.Application.Mocks;

internal static class This
{
    public static string EmptyString() => It.Is<string>(word => string.IsNullOrWhiteSpace(word));
}
