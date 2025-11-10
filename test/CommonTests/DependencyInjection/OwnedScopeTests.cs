using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcRocNy.Common.DependencyInjection;

public class OwnedScopeTests
{
    [Fact]
    public void OwnedScope_ShouldResolveExpectedLifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddTransient<OwnedScope<TestService>>();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        var owned = dic.GetRequiredService<OwnedScope<TestService>>();
        owned.Should().NotBeNull();

        using (owned)
        {
            owned.Value.Should().NotBeNull();
            owned.Value.IsDisposed.Should().BeFalse();
        }
        // contained object lifetime is tied to scope
        owned.Value.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void OwnedScope_ShouldResolveExpectedScopes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddTransient<OwnedScope<TestService>>();
        using var dic = services.BuildServiceProvider();

        // Act
        using var owned = dic.GetRequiredService<OwnedScope<TestService>>();
        using var owned2 = dic.GetRequiredService<OwnedScope<TestService>>();

        // Assert - they are in fact separate scopes, so will resolve distinct Scoped services
        owned.Value.Should().NotBeSameAs(owned2.Value);
    }
}
