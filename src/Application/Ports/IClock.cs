using System;

namespace FM.Application.Ports
{
    public interface IClock
    {
        DateTime Now => DateTime.UtcNow;
    }
}
