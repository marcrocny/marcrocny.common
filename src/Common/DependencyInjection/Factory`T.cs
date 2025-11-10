using System;
using Microsoft.Extensions.DependencyInjection;

namespace MarcRocNy.Common.DependencyInjection;

/// <summary>
/// A very basic factory wrapper.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Factory<T>
    where T : class
{
    private readonly Func<T> _factory;

    public Factory(IServiceProvider serviceProvider)
    {
        _factory = serviceProvider.GetRequiredService<T>;
    }

    public T Create() => _factory();
}
