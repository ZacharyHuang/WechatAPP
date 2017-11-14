using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WerewolfBackend.Startup))]
namespace WerewolfBackend
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
