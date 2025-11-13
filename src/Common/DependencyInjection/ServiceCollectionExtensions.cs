using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MarcRocNy.Common.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddInterfacesFor<TImplementation>(this IServiceCollection services, ServiceLifetime lifetime,
        IList<Type> interfaceTypes)
        where TImplementation : class
    {
        if (interfaceTypes.Count == 0) return services;
        var implType = typeof(TImplementation);

        // register against first interface directly
        services.Add(ServiceDescriptor.Describe(interfaceTypes[0], implType, lifetime));

        object factory(IServiceProvider sp) => sp.GetRequiredService(interfaceTypes[0]);
        foreach (var interfaceType in interfaceTypes.Skip(1))
        {
            services.Add(ServiceDescriptor.Describe(interfaceType, factory, lifetime));
        }
        return services;
    }

    private static IServiceCollection AddAsInterfaces<TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
        where TImplementation : class
    {
        var implType = typeof(TImplementation);
        var interfaceTypes = implType.GetInterfaces()
            .Where(it => it != typeof(IDisposable)).ToList();
        return services.AddInterfacesFor<TImplementation>(lifetime, interfaceTypes);
    }

    /// <summary>
    /// Adds the singleton implementation class to the collection, as its implemented interfaces.
    /// </summary>
    /// <remarks>
    /// MSEDI has the strange property that for scoped and singleton services, adding a class as each of its
    /// implemented interfaces and resolving for those different interfaces will result in the resolution
    /// of distinct instances. This can be surprising behavior, even for those of us who came from Autofac.
    /// <para/>
    /// These shim registration methods follow the more expected behavior, in an API that clearly links them
    /// through a single call. The underlying <see cref="IServiceCollection"/> `Add()` calls use the service
    /// provider factory to clearly return the same instance for all interfaces.
    /// </remarks>
    public static IServiceCollection AddSingletonAsInterfaces<TImplementation>(this IServiceCollection services)
        where TImplementation : class
        => AddAsInterfaces<TImplementation>(services, ServiceLifetime.Singleton);

    /// <summary>
    /// Adds the singleton implementation class to the collection, as its implemented interfaces.
    /// </summary>
    /// <remarks>
    /// <see cref="AddSingletonAsInterfaces{TImplementation}(IServiceCollection)"/>.
    /// </remarks>
    public static IServiceCollection AddScopedAsInterfaces<TImplementation>(this IServiceCollection services)
        where TImplementation : class
        => AddAsInterfaces<TImplementation>(services, ServiceLifetime.Scoped);

    /// <summary>
    /// Adds registration of two services for the underlying implementing class, both pointing to the same instance.
    /// </summary>
    public static IServiceCollection AddSingleton<TService1, TService2, TImplementation>(this IServiceCollection services)
        where TImplementation : class
        => services.AddInterfacesFor<TImplementation>(
            ServiceLifetime.Singleton, 
            [typeof(TService1), typeof(TService2)]);

    /// <summary>
    /// Adds registration of two services for the underlying implementing class, both pointing to the same instance.
    /// </summary>
    public static IServiceCollection AddScoped<TService1, TService2, TImplementation>(this IServiceCollection services)
        where TImplementation : class
        => services.AddInterfacesFor<TImplementation>(
            ServiceLifetime.Scoped,
            [typeof(TService1), typeof(TService2)]);

    /// <summary>
    /// Adds registration of three services for the underlying implementing class, all pointing to the same instance.
    /// </summary>
    public static IServiceCollection AddSingleton<TService1, TService2, TService3, TImplementation>(this IServiceCollection services)
        where TImplementation : class
        => services.AddInterfacesFor<TImplementation>(
            ServiceLifetime.Singleton,
            [typeof(TService1), typeof(TService2), typeof(TService3)]);

    /// <summary>
    /// Adds registration of three services for the underlying implementing class, all pointing to the same instance.
    /// </summary>
    public static IServiceCollection AddScoped<TService1, TService2, TService3, TImplementation>(this IServiceCollection services)
        where TImplementation : class
        => services.AddInterfacesFor<TImplementation>(
            ServiceLifetime.Scoped,
            [typeof(TService1), typeof(TService2), typeof(TService3)]);
}
