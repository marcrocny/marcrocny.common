using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcRocNy.Common.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    private interface ITestA { string A { get; } }
    private interface ITestB { string B { get; } }
    private interface IFoo { bool IsDisposed { get; } }

    private class TestSvc : ITestA, ITestB, IFoo, IDisposable
    {
        public string A => "A";
        public string B => "B";
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
        }
    }

    [Fact]
    public void AddSingletonAsInterfaces_ShouldNotResolveDisposable()
    {
        // arrange
        ServiceCollection services = new();
        services.AddSingletonAsInterfaces<TestSvc>();
        using var dic = services.BuildServiceProvider();

        Action act = () => dic.GetRequiredService<IDisposable>();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddSingletonAsInterfaces_ShouldResolveExpected()
    {
        // arrange
        ServiceCollection services = new();
        services.AddSingletonAsInterfaces<TestSvc>();
        using var dic = services.BuildServiceProvider();

        IFoo foo;
        using (var scope = dic.CreateScope())
        {
            // act
            var testA = scope.ServiceProvider.GetRequiredService<ITestA>();
            var testB = scope.ServiceProvider.GetRequiredService<ITestB>();
            foo = scope.ServiceProvider.GetRequiredService<IFoo>();

            // assert
            testA.Should().BeSameAs(testB);
            testA.Should().BeSameAs(foo);
            foo.IsDisposed.Should().BeFalse();
        }
        foo.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void AddScopedAsInterfaces_ShouldResolveExpected()
    {
        // arrange
        ServiceCollection services = new();
        services.AddScopedAsInterfaces<TestSvc>();
        using var dic = services.BuildServiceProvider();

        IFoo foo;
        using (var scope = dic.CreateScope())
        {
            // act
            var testA = scope.ServiceProvider.GetRequiredService<ITestA>();
            var testB = scope.ServiceProvider.GetRequiredService<ITestB>();
            foo = scope.ServiceProvider.GetRequiredService<IFoo>();

            // assert
            testA.Should().BeSameAs(testB);
            testA.Should().BeSameAs(foo);
            foo.IsDisposed.Should().BeFalse();
        }
        foo.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void AddScoped2T_ShouldResolveExpected()
    {
        // arrange
        ServiceCollection services = new();
        services.AddScoped<ITestA, IFoo, TestSvc>();
        using var dic = services.BuildServiceProvider();

        IFoo foo;
        using (var scope = dic.CreateScope())
        {
            // act
            var testA = scope.ServiceProvider.GetRequiredService<ITestA>();
            var testB = scope.ServiceProvider.GetService<ITestB>();
            foo = scope.ServiceProvider.GetRequiredService<IFoo>();

            // assert
            testA.Should().BeSameAs(foo);
            testB.Should().BeNull();
            foo.IsDisposed.Should().BeFalse();
        }
        foo.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void AddSingleton2T_ShouldResolveExpected()
    {
        // arrange
        ServiceCollection services = new();
        services.AddSingleton<ITestA, IFoo, TestSvc>();
        using var dic = services.BuildServiceProvider();

        IFoo foo;
        using (var scope = dic.CreateScope())
        {
            // act
            var testA = scope.ServiceProvider.GetRequiredService<ITestA>();
            var testB = scope.ServiceProvider.GetService<ITestB>();
            foo = scope.ServiceProvider.GetRequiredService<IFoo>();

            // assert
            testA.Should().BeSameAs(foo);
            testB.Should().BeNull();
            foo.IsDisposed.Should().BeFalse();
        }
        foo.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void AddSingleton3T_ShouldResolveExpected()
    {
        // arrange
        ServiceCollection services = new();
        services.AddSingleton<ITestA, ITestB, IFoo, TestSvc>();
        using var dic = services.BuildServiceProvider();

        IFoo foo;
        using (var scope = dic.CreateScope())
        {
            // act
            var testA = scope.ServiceProvider.GetRequiredService<ITestA>();
            var testB = scope.ServiceProvider.GetRequiredService<ITestB>();
            foo = scope.ServiceProvider.GetRequiredService<IFoo>();

            // assert
            testA.Should().BeSameAs(testB);
            testA.Should().BeSameAs(foo);
            foo.IsDisposed.Should().BeFalse();
        }
        foo.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void AddScoped3T_ShouldResolveExpected()
    {
        // arrange
        ServiceCollection services = new();
        services.AddScoped<ITestA, ITestB, IFoo, TestSvc>();
        using var dic = services.BuildServiceProvider();

        IFoo foo;
        using (var scope = dic.CreateScope())
        {
            // act
            var testA = scope.ServiceProvider.GetRequiredService<ITestA>();
            var testB = scope.ServiceProvider.GetRequiredService<ITestB>();
            foo = scope.ServiceProvider.GetRequiredService<IFoo>();

            // assert
            testA.Should().BeSameAs(testB);
            testA.Should().BeSameAs(foo);
            foo.IsDisposed.Should().BeFalse();
        }
        foo.IsDisposed.Should().BeTrue();
    }

}
