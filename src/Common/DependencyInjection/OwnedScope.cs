using System;
using Microsoft.Extensions.DependencyInjection;

namespace MarcRocNy.Common.DependencyInjection;

/// <summary>
/// A sub-scoped dependency, ideal for scoped-dependency resolution in a singleton service.
/// </summary>
public class OwnedScope<T> : IDisposable
    where T : class
{
    private readonly IDisposable _scope;

    public T Value { get; }

    public OwnedScope(IServiceScopeFactory scopeFactory)
    {
        var scope = scopeFactory.CreateScope();
        _scope = scope;
        Value = scope.ServiceProvider.GetRequiredService<T>();
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _scope.Dispose();
    }
}
