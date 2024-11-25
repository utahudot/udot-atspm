using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine.NamingConventionBinder;

namespace Utah.Udot.ATSPM.WatchDog.Commands
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
