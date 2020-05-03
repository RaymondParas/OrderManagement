using AuthenticationLibrary.Services;
using Autofac;
using Autofac.Integration.WebApi;
using OrderLibrary.Helpers;
using OrderLibrary.Logger;
using OrderLibrary.Parsers;
using OrderLibrary.Services;
using System.Reflection;
using System.Web.Http;

namespace OrderManagementAPI.App_Start
{
    public class AutofacWebAPIConfig
    {

        public static IContainer Container;

        public static void Initialize(HttpConfiguration config)
        {
            Initialize(config, RegisterServices(new ContainerBuilder()));
        }


        public static void Initialize(HttpConfiguration config, IContainer container)
        {
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static IContainer RegisterServices(ContainerBuilder builder)
        {
            //Register your Web API controllers.  
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterType<DBHelper>()
                   .As<IDBHelper>()
                   .InstancePerRequest();

            builder.RegisterType<LoggerDB>()
                   .As<ILoggerDB>()
                   .InstancePerRequest();

            builder.RegisterType<Logger>()
                   .As<ILogger>()
                   .InstancePerRequest();

            builder.RegisterType<OrderService>()
                   .As<IOrderService>()
                   .InstancePerRequest();

            builder.RegisterType<CSVParser>()
                   .As<ICSVParser>()
                   .InstancePerRequest();

            //Set the dependency resolver to be Autofac.  
            Container = builder.Build();

            return Container;
        }

    }
}