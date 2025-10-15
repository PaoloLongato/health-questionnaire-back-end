using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using QuestionnaireService.Status;

namespace QuestionnaireService.UnitTests.Status;

public sealed class ServiceStatusProviderTests
{
    [Fact]
    public void GetStatus_ReturnsExpectedMetadata()
    {
        var environment = new TestHostEnvironment("Testing");
        var provider = new ServiceStatusProvider(environment);

        var status = provider.GetStatus();

        Assert.Equal(typeof(ServiceStatusProvider).Assembly.GetName().Name, status.Service);
        Assert.Equal(environment.EnvironmentName, status.Environment);
        Assert.False(string.IsNullOrWhiteSpace(status.Version));
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
            ApplicationName = "QuestionnaireService";
            ContentRootPath = AppContext.BaseDirectory;
        }

        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
