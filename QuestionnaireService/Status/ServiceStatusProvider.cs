using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace QuestionnaireService.Status;

public sealed class ServiceStatusProvider
{
    private readonly IHostEnvironment _environment;
    private readonly string _serviceName;
    private readonly string _version;

    public ServiceStatusProvider(IHostEnvironment environment)
    {
        _environment = environment;
        _serviceName = Assembly.GetExecutingAssembly().GetName().Name ?? nameof(ServiceStatusProvider);
        _version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    }

    public ServiceStatus GetStatus()
    {
        return new ServiceStatus(_serviceName, _environment.EnvironmentName, _version);
    }
}
