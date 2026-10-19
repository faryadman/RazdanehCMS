using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Project.Application;
using Project.Application.DTOs.Group;
using Project.Domain.Entities;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class MapperLifetimeTests
{
    [Fact]
    public async Task Mapper_uses_the_host_logger_factory_until_the_host_disposes_it()
    {
        var loggerFactory = new TrackingLoggerFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(_ => loggerFactory);
        services.ConfigureApplicationServices(new ConfigurationBuilder().Build());
        await using (var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true }))
        {
            using (var scope = provider.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                Assert.Equal("گروه نمونه", mapper.Map<GroupDTO>(new Group { Title = "گروه نمونه" }).Title);
                Assert.True(loggerFactory.LoggerCreations > 0);
                Assert.False(loggerFactory.Disposed);
            }
            Assert.False(loggerFactory.Disposed);
        }
        Assert.True(loggerFactory.Disposed);
    }

    private sealed class TrackingLoggerFactory : ILoggerFactory
    {
        public int LoggerCreations { get; private set; }
        public bool Disposed { get; private set; }
        public ILogger CreateLogger(string categoryName)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            LoggerCreations++;
            return NullLogger.Instance;
        }
        public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();
        public void Dispose() => Disposed = true;
    }
}
