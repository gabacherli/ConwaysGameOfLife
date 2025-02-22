using GameOfLife.API.Extensions;
using GameOfLife.API.Repositories.Read;
using GameOfLife.API.Repositories.Write;
using GameOfLife.API.Services;
using GameOfLife.API.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace GameOfLife.API.Tests.ExtensionsTests
{
    public static class ServicesExtensionsTests
    {
        [Fact]
        public static void AddAppSettings_ShouldConfigureAppSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = Substitute.For<IConfiguration>();

            // Act
            services.AddAppSettings(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<AppSettings>>();
            Assert.NotNull(options);
        }

        [Fact]
        public static void AddServices_ShouldRegisterScopedServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddServices();
            services.BuildServiceProvider();

            // Assert
            AssertServiceRegistered<IBoardService, BoardService>(services);
            AssertServiceRegistered<IBoardReadRepository, BoardReadRepository>(services);
            AssertServiceRegistered<IBoardWriteRepository, BoardWriteRepository>(services);
        }

        private static void AssertServiceRegistered<TService, TImplementation>(IServiceCollection services)
        {
            var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TService));
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
            Assert.Equal(typeof(TImplementation), serviceDescriptor.ImplementationType);
        }
    }

}
