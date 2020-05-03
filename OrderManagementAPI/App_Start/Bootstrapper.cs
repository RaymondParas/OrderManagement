using System.Web.Http;

namespace OrderManagementAPI.App_Start
{
    public class Bootstrapper
    {

        public static void Run()
        {
            //Configure AutoFac  
            AutofacWebAPIConfig.Initialize(GlobalConfiguration.Configuration);
        }

    }
}