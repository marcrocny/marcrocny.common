using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using MarcRocNy.Common.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Xunit;

namespace MarcRocNy.Common.Resilience;

/// <summary>
/// Demo of the pipeline-holder and advanced config.
/// </summary>
public class PipelineHolderTests
{
    public class Retry : IPollyPipeline<PipelineHolderTests>
    {
        public static readonly Type[] Retryable = [
            typeof(SocketException),
            typeof(System.Net.Http.HttpRequestException),            
        ];

        public ResiliencePipeline Pipeline { get; }

        public Retry()
        {
            Pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new Polly.Retry.RetryStrategyOptions()
                {
                    ShouldHandle = args => ValueTask.FromResult(Retryable.Contains(args.Outcome.Exception?.GetType())),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(10),
                    BackoffType = DelayBackoffType.Constant,
                }).Build();
        }

    }

    /// <summary>
    /// Basic exercise of the pipeline holder.
    /// </summary>
    /// <exception cref="SocketException"></exception>
    [Fact]
    public void Retry_Exception_Retries()
    {
        int calls = 0;

        Retry sut = new();

        sut.Pipeline.Execute(() =>
        {
            calls++;
            if (calls < 2) throw new SocketException();
        });

        calls.Should().Be(2);
    }

    /// <summary>
    /// Perennial issue: a common configuration subsection, but how to consume without leaking
    /// the parent config object through? It can be done by using a config-holder interface. This
    /// solution "inverts" by using a marker-generic and conventional config.
    /// </summary>
    /// <typeparam name="TParentMarker"></typeparam>
    public record RetryConfiguration<TParentMarker> : ISettings
    {
        public static string SectionName { get; } = $"{typeof(TParentMarker).Name}:Retry";

        public int MaxRetryAttempts { get; init; } = 3;

        public TimeSpan Delay { get; init; } = TimeSpan.FromMilliseconds(10);

        public DelayBackoffType BackoffType { get; init; } = DelayBackoffType.Constant;
    }

    /// <summary>
    /// It's a mouthful, but you'll never have to write it out.
    /// </summary>
    /// <typeparam name="TParentMarker"></typeparam>
    public class ConventionalConfigRetry<TParentMarker> : IPollyPipeline<TParentMarker>
    {
        public static readonly Type[] Retryable = [
            typeof(SocketException),
            typeof(System.Net.Http.HttpRequestException),
        ];

        public ResiliencePipeline Pipeline { get; }

        /// <summary>
        /// This cleanly detects the config via the marker class name. A static method of setting up the
        /// retryable exceptions can also be set up. This does fall down on more complex retry logic--it
        /// really must be reusable. But that may often be the case for things like HttpClient implementations.
        /// The retry handler will be the same, but this allows tweaking the retry params for specific
        /// endpoints.
        /// </summary>
        public ConventionalConfigRetry(IOptions<RetryConfiguration<TParentMarker>> options)
        {
            var config = options.Value;

            Pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new Polly.Retry.RetryStrategyOptions()
                {
                    ShouldHandle = args => ValueTask.FromResult(Retryable.Contains(args.Outcome.Exception?.GetType())),
                    MaxRetryAttempts = config.MaxRetryAttempts,
                    Delay = config.Delay,
                    BackoffType = config.BackoffType,
                }).Build();

        }
    }

    public class RetryConsumer(IPollyPipeline<RetryConsumer> pollyPipeline)
    {
        private readonly ResiliencePipeline retry = pollyPipeline.Pipeline;

        /// <summary>
        /// Returns attempts.
        /// </summary>
        /// <returns>attempts</returns>
        /// <exception cref="SocketException"></exception>
        public int TryAttempts(int throwThisManyTimes)
        {
            int calls = 0;

            retry.Execute(() =>
            {
                calls++;
                if (calls < throwThisManyTimes) throw new SocketException(calls);   // cheap use of ErrorCode
            });

            return calls;
        }
    }

    [Fact]
    public void DemoConventionalGenericConfig()
    {
        // arrange
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RetryConsumer:Retry:MaxretryAttempts"] = "5",
                ["RetryConsumer:Retry:Delay"] = "00:00:00.05",
                ["retryconsumer:retry:backofftype"] = nameof(DelayBackoffType.Linear),
                // this allows "retry" as a config subsection to retryconsumer config; e.g.
                ["retryconsumer:host"] = "127.0.0.1",
                ["retryconsumer:port"] = "8080",
            })
            .Build();

        ServiceProvider services = new ServiceCollection()
            .AddScoped<RetryConsumer>()
            // You may need sub-contracts of `IPollyPipeline<>` for pipeline setups that are specific
            // to a given overall consumer type. Or your consumer may allow more flexible config than
            // was outlined her. There's some flexibility. It's art, not science.
            // subnote: this method of registration is underused. One case where a generic typeparam
            // shows its limitations
            .AddSingleton(typeof(IPollyPipeline<>), typeof(ConventionalConfigRetry<>))
            .Configure<RetryConfiguration<RetryConsumer>>(configuration)
            .BuildServiceProvider();

        // act
        var config = services.GetRequiredService<IOptions<RetryConfiguration<RetryConsumer>>>();
        var consumer = services.GetRequiredService<RetryConsumer>();

        // assert
        config.Value.Should().BeEquivalentTo(new RetryConfiguration<int>
        {
            MaxRetryAttempts = 5,
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(0.05),
        });

        consumer.TryAttempts(3).Should().Be(3);
        Action act = () => consumer.TryAttempts(8);
        act.Should().Throw<SocketException>().Where(ex => ex.ErrorCode == 6);
    }
}
