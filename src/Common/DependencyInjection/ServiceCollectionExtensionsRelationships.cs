using System;
using Microsoft.Extensions.DependencyInjection;

namespace MarcRocNy.Common.DependencyInjection;

/// <summary>
/// Add some implicit relationships to MS DI.
/// </summary>
/// <remarks>
/// Sometimes you'd like some Autofac-like features without going "full Autofac". One of the nicest features
/// Autofac has to offer are Implicit Relationships. These are pretty deep inside Autofac. They can be simulated
/// with MS DI to a certain extent.
/// </remarks>
public static class ServiceCollectionExtensionsRelationships
{
    /// <summary>
    /// Generic-resolution factory.
    /// </summary>
    /// <remarks>
    /// Keeping the simplest one at the top. <see cref="Lazy{T}"/> is pretty simple because it is not sealed. This is the
    /// answer to the question: how does one create a runtime factory for an open generic registration when 
    /// <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection, Type, Func{IServiceProvider, object})"/>
    /// has no means of passing the concrete type to the factory delegate? Sidestep—let the runtime and DI do the work:
    /// by assigning this derived class as the implementation, we get the concrete type and dependecies injected at
    /// resolution for free. See also: https://stackoverflow.com/a/42650112/85269
    /// </remarks>
    private class LazyResolver<T> : Lazy<T>
        where T : class
    {
        public LazyResolver(IServiceProvider serviceProvider) : base(serviceProvider.GetRequiredService<T>, true) { }
    }

    /// <summary>
    /// Add Autofac-like <see cref="Lazy{T}"/> implicit relationships.
    /// </summary>
    public static IServiceCollection AddLazySupport(this IServiceCollection services)
        => services.AddTransient(typeof(Lazy<>), typeof(LazyResolver<>));

    /// <summary>
    /// Add Autofac-like (`Owned{T}`) <see cref="OwnedScope{T}"/> implicit relationships.
    /// </summary>
    public static IServiceCollection AddOwnedScopeSupport(this IServiceCollection services)
        => services.AddTransient(typeof(OwnedScope<>));

    /// <summary>
    /// Add <see cref="Func{TResult}"/> factory support for the given service-type; does not register the service itself, 
    /// only the factory.
    /// </summary>
    /// <remarks>
    /// With MSEDI it is not possible to add an open-generic registration for Autofac-style implicit `Func{T}` factories.
    /// <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection, Type, Type)"/> requires a derived 
    /// resolver (as with `Lazy{T}` above) but delegates are `sealed`.
    /// <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection, Type, Func{IServiceProvider, object})"/>
    /// does not pass the resolution-time `TService` to the factory-lambda.
    /// <para/>
    /// An open-generic factory is possible, but isn't quite as pretty.
    /// </remarks>
    public static IServiceCollection AddFuncFor<T>(this IServiceCollection services)
        where T : class
        => services.AddTransient<Func<T>>(sp => sp.GetRequiredService<T>);

    /// <summary>
    /// Add implicit <see cref="Factory{T}"/> support; like Autofac's implicit <see cref="Func{TResult}"/>,
    /// but not quite as sugary.
    /// </summary>
    public static IServiceCollection AddFactorySupport(this IServiceCollection services)
        => services.AddTransient(typeof(Factory<>));
}
