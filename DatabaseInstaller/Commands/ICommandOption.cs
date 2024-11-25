using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine.NamingConventionBinder;

namespace DatabaseInstaller.Commands
{
    public interface ICommandOption<T> : ICommandOption
    {
        ModelBinder<T> GetOptionsBinder();
    }
    public interface ICommandOption
    {
        void BindCommandOptions(HostBuilderContext host, IServiceCollection services);
    }
}
