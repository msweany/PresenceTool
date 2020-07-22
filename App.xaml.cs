using Microsoft.Identity.Client;
using System.Windows;
using System.Configuration;

namespace presence_tool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    
    // To change from Microsoft public cloud to a national cloud, use another value of AzureCloudInstance
    public partial class App : Application
    {
        static App()
        {
            _clientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority($"{Instance}{Tenant}")
                .WithDefaultRedirectUri()
                .Build();
            TokenCacheHelper.EnableSerialization(_clientApp.UserTokenCache);
        }

        // value set in app.config
        private static string ClientId = ConfigurationManager.AppSettings["clientID"];

        //  for any Work or School accounts, set tenant to use organizations
        private static string Tenant = "organizations";
        private static string Instance = "https://login.microsoftonline.com/";
        private static IPublicClientApplication _clientApp ;

        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }

    }
}
