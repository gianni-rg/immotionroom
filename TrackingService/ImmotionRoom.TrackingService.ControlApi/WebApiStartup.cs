namespace ImmotionAR.ImmotionRoom.TrackingService
{
    using System.Web.Http;
    using Owin;

    public class WebApiStartup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host.
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("DefaultApi", "internal/v1/{controller}/{action}/{id}", new {id = RouteParameter.Optional});

            appBuilder.UseWebApi(config);
        }
    }
}
