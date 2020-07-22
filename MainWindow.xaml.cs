using Microsoft.Identity.Client;
using System;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace presence_tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        // Set the API Endpoint to Graph 'presence' default endpoint 
        // * changed in CallGraphButton_Click() if userID is set in app.config
        string graphAPIEndpoint = "https://graph.microsoft.com/beta/me/presence";

        //Set the scope for API call to user.read
        string[] scopes = new string[] { "user.read" };

        private static System.Timers.Timer aTimer;

        public MainWindow()
        {
            InitializeComponent();
            // kick off the request for auth or presence
            CallGraphButton_Click("Script", null);
            SetTimer();
        }

        private void SetTimer(){
            // Create a timer, interval determined by app.config
            // value set in app.config
            var timeInMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["timerSeconds"]) * 1000;
            aTimer = new System.Timers.Timer(timeInMilliseconds);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e){
            this.Dispatcher.Invoke(() =>
            {
                CallGraphButton_Click("Script", null);
            });
        }

        /// <summary>
        /// Call CallGraphButton - to acquire a token requiring user to sign-in
        /// </summary>
        /// 
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            // if a user ID is set, assume the app is registered in a different AAD than the user we are monitoring
            if (ConfigurationManager.AppSettings["userID"] != null){
                // value set in app.config
                graphAPIEndpoint = "https://graph.microsoft.com/beta/users/" + ConfigurationManager.AppSettings["userID"] + "/presence";
            }

            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;
            ResultText.Text = string.Empty;
            TokenInfoText.Text = string.Empty;

            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await app.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await app.AcquireTokenInteractive(scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle) // optional, used to center the browser on the window
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
                }
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
                return;
            }

            if (authResult != null)
            {
                ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                DisplayBasicTokenInfo(authResult);
                this.SignOutButton.Visibility = Visibility.Visible;
                this.CallGraphButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                
                // isolate "activity" from response
                var content = await response.Content.ReadAsStringAsync();
                var activity = content.Split('"');
                Console.WriteLine(ConfigurationManager.AppSettings["clientID"]);
                // call local API and pass it the activity result
                // value set in app.config
                var url2 = ""+ ConfigurationManager.AppSettings["apiPath"] + activity[15] + "";
                var httpClient2 = new System.Net.Http.HttpClient();
                System.Net.Http.HttpResponseMessage response2;
                try{
                    var request2 = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url2);
                    response2 = await httpClient.SendAsync(request2);
                    var content2 = await response2.Content.ReadAsStringAsync();
                    Console.WriteLine(content);
                }
                catch (Exception ex){
                    Console.WriteLine(ex.ToString());
                }

                Console.WriteLine(activity[15]);
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var accounts = await App.PublicClientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await App.PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
                    this.ResultText.Text = "User has signed-out";
                    this.CallGraphButton.Visibility = Visibility.Visible;
                    this.SignOutButton.Visibility = Visibility.Collapsed;
                }
                catch (MsalException ex)
                {
                    ResultText.Text = $"Error signing-out user: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Display basic information contained in the token
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"Username: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }
    }
}
