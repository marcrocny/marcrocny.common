using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcRocNy.Common.DependencyInjection;

public class FactoryTests
{
    [Fact]
    public void Factory_ShouldResolveExpectedLifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();
        services.AddTransient<Factory<TestService>>();
        using var dic = services.BuildServiceProvider();

        // Act/Assert
        TestService ts, ts2;
        using (var scope = dic.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<Factory<TestService>>();
            factory.Should().NotBeNull();

            ts = factory.Create();
            ts.Should().NotBeNull();

            var factory2 = scope.ServiceProvider.GetRequiredService<Factory<TestService>>();
            factory2.Should().NotBeSameAs(factory); // transient
            ts2 = factory2.Create();
            ts2.Should().BeSameAs(ts);  // resolves within the containing scope
            ts.IsDisposed.Should().BeFalse();
            ts2.IsDisposed.Should().BeFalse();
        }
        ts.IsDisposed.Should().BeTrue();
        ts2.IsDisposed.Should().BeTrue();
    }
}
