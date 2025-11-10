using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcRocNy.Common.DependencyInjection;

public class ServiceCollectionExtensionsRelationshipsTests
{
    [Fact]
    public void AddLazySupport_ShouldResolveExpected()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddLazySupport();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        var lazy = dic.GetRequiredService<Lazy<TestService>>();
        lazy.Should().NotBeNull();
        lazy.IsValueCreated.Should().BeFalse();
        lazy.Value.Should().NotBeNull();
    }

    [Fact]
    public void AddOwnedScopeSupport_ShouldResolveExpectedLifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddOwnedScopeSupport();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        using var owned = dic.GetRequiredService<OwnedScope<TestService>>();
        owned.Should().NotBeNull();
        using (owned) owned.Value.IsDisposed.Should().BeFalse();
        owned.Value.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void AddFuncFor_ShouldResolveExpected()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddFuncFor<TestService>();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        TestService ts, ts2;
        using (var scope = dic.CreateScope())
        {
            var func = scope.ServiceProvider.GetRequiredService<Func<TestService>>();
            func.Should().NotBeNull();

            ts = func();
            ts.Should().NotBeNull();

            var func2 = scope.ServiceProvider.GetRequiredService<Func<TestService>>();
            func2.Should().NotBeSameAs(func); // transient
            ts2 = func2();
            ts2.Should().BeSameAs(ts);  // resolves within the containing scope
            ts.IsDisposed.Should().BeFalse();
            ts2.IsDisposed.Should().BeFalse();
        }
        ts.IsDisposed.Should().BeTrue();
        ts2.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void AddFactorySupport_ShouldResolveExpected()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddFactorySupport();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        var factory = dic.GetRequiredService<Factory<TestService>>();
        factory.Should().NotBeNull();

        var ts = factory.Create();
        ts.Should().NotBeNull();
        ts.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Nested_FuncOwned_ShouldResolveExpected()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddOwnedScopeSupport();
        services.AddFuncFor<OwnedScope<TestService>>();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        var func = dic.GetRequiredService<Func<OwnedScope<TestService>>>();
        func.Should().NotBeNull();
        var owned = func();
        owned.Should().NotBeNull();
        func().Should().NotBeSameAs(owned); // generates fresh

        using (owned)
        {
            owned.Value.Should().NotBeNull();
            owned.Value.IsDisposed.Should().BeFalse();
        }
        // contained object lifetime is tied to scope
        owned.Value.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Nested_FactoryOwned_ShouldResolveExpected()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddOwnedScopeSupport();
        services.AddFactorySupport();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        var factory = dic.GetRequiredService<Factory<OwnedScope<TestService>>>();
        factory.Should().NotBeNull();
        var owned = factory.Create();
        owned.Should().NotBeNull();
        factory.Create().Should().NotBeSameAs(owned); // generates fresh

        using (owned)
        {
            owned.Value.Should().NotBeNull();
            owned.Value.IsDisposed.Should().BeFalse();
        }
        // contained object lifetime is tied to scope
        owned.Value.IsDisposed.Should().BeTrue();
    }
}