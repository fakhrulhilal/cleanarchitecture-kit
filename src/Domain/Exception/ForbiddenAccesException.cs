namespace FM.Domain.Exception
{
    /// <summary>
    /// Authenticated user but not no granted permission to access certain resource
    /// </summary>
    public class ForbiddenAccessException : BusinessApplicationException
    { }
}
