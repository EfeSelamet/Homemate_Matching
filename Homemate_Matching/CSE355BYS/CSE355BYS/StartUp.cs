using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Homemate_Matching.Startup))] // Replace YourProjectName

namespace Homemate_Matching // Replace YourProjectName
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Register your custom IUserIdProvider
            var userIdProvider = new DatabaseUserIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => userIdProvider);

            // Map SignalR hubs
            app.MapSignalR();
        }
    }
}
