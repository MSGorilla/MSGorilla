using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MSGorilla.Startup))]
namespace MSGorilla
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
