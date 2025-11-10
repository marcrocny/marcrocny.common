using System;

namespace MarcRocNy.Common.DependencyInjection;

public class TestService : IDisposable
{
    public bool IsDisposed { get; private set; }
    void IDisposable.Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
    }
}

