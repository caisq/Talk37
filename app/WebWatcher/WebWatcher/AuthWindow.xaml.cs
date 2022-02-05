using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

// Based on code sample at https://github.com/googlesamples/oauth-apps-for-windows, which
// was released under the Apache 2.0 license.

namespace WebWatcher
{
    public class UserInfo
    {
        public readonly string userId;
        public readonly string userEmail;
        public readonly string userGivenName;
        public readonly string userFamilyName;
        public readonly string refreshToken;
        public UserInfo(string userId,
                        string userEmail,
                        string userGivenName,
                        string userFamilyName,
                        string refreshToken)
        {
            this.userId = userId;
            this.userEmail = userEmail;
            this.userGivenName = userGivenName;
            this.userFamilyName = userFamilyName;
            this.refreshToken = refreshToken;
        }
    }
    public partial class AuthWindow : Window
    {
        private const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private Func<string, Task<int>> newAccessTokenCallback;
        public AuthWindow()
        {
            InitializeComponent();
        }

        public async void TryGetAccessTokenUsingRefreshToken(
            Func<string, Task<int>> newAccessTokenCallback)
        {
            UserInfo userInfo = LoadUserInfo();
            if (userInfo == null)
            {
                ShowAndGetAccessToken(newAccessTokenCallback);
                return;
            }
            string refreshToken = userInfo.refreshToken;
            if (refreshToken == null || refreshToken == "")
            {
                ShowAndGetAccessToken(newAccessTokenCallback);
                return;
            }

            string clientId = Environment.GetEnvironmentVariable("SPEAKFASTER_WEBVIEW_CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("SPEAKFASTER_WEBVIEW_CLIENT_SECRET");
            // TODO(cais): Refactor into helper method.
            string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
            string tokenRequestBody = string.Format(
                "client_id={0}&client_secret={1}&refresh_token={2}&grant_type=refresh_token",
                clientId,
                clientSecret,
                refreshToken);

            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    Debug.WriteLine(responseText);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string accessToken = tokenEndpointDecoded["access_token"];
                    Debug.WriteLine($"Access token from refresh token: {accessToken}");
                    _ = await newAccessTokenCallback(accessToken);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        Debug.WriteLine("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync();
                            Debug.WriteLine(responseText);
                        }
                    }

                }
                ShowAndGetAccessToken(newAccessTokenCallback);
                return;
            }
        }

        private void ShowAndGetAccessToken(Func<string, Task<int>> newAccessTokenCallback)
        {
            this.newAccessTokenCallback = newAccessTokenCallback;
            Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Show();
                    PerformSignIn();
                }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            PerformSignIn();
        }

        private async void PerformSignIn() { 
            string state = RandomDataBase64url(32);
            string code_verifier = RandomDataBase64url(32);
            string code_challenge = Base64urlencodeNoPadding(Sha256(code_verifier));
            const string code_challenge_method = "S256";

            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            HttpListener http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            Debug.WriteLine($"HTTP server started at {redirectURI}");
            http.Start();

            string clientId = Environment.GetEnvironmentVariable("SPEAKFASTER_WEBVIEW_CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("SPEAKFASTER_WEBVIEW_CLIENT_SECRET");
            Debug.Assert(clientId != null && clientId != "");
            // scope=openid
            string authorizationRequest = string.Format(
                "{0}?response_type=code&scope=email%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                Uri.EscapeDataString(redirectURI),
                clientId,
                state,
                code_challenge,
                code_challenge_method);

            // Opens request in the browser.
            _ = Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            HttpListenerContext context = await http.GetContextAsync();

            // Brings this app back to the foreground.
            _ = Activate();

            //// Sends an HTTP response to the browser.
            //var response = context.Response;
            // Sends an HTTP response to the browser.
            HttpListenerResponse response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                Debug.WriteLine(string.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                Debug.WriteLine("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // extracts the code
            string code = context.Request.QueryString.Get("code");
            string incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                Debug.WriteLine(string.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }
            Debug.WriteLine("Authorization code: " + code);

            PerformCodeExchange(code, code_verifier, redirectURI, clientId, clientSecret);
        }

        private async void PerformCodeExchange(string code,
                                               string code_verifier, 
                                               string redirectURI,
                                               string clientId,
                                               string clientSecret)
        {
            Debug.WriteLine("Exchanging code for tokens...");

            // builds the  request
            string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                Uri.EscapeDataString(redirectURI),
                clientId,
                code_verifier,
                clientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    Debug.WriteLine(responseText);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string accessToken = tokenEndpointDecoded["access_token"];
                    string refreshToken = tokenEndpointDecoded["refresh_token"];
                    UserInfo userInfo = await UserInfoCall(accessToken, refreshToken);
                    SaveUserInfo(userInfo);
                    if (this.newAccessTokenCallback != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                Hide();
                            }));
                        _ = this.newAccessTokenCallback(accessToken);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        Debug.WriteLine("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync();
                            Debug.WriteLine(responseText);
                        }
                    }

                }
            }
        }

        private void SaveUserInfo(UserInfo userInfo)
        {
            string appDataFilePath = GetAppDataFilePath();
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ");
            Dictionary<string, string> appDataObject = new Dictionary<string, string>
            {
                { "refresh_token", userInfo.refreshToken },
                { "refresh_token_creation_timestamp", timestamp },
                { "user_id", userInfo.userId },
                { "user_email", userInfo.userEmail },
                { "user_given_name", userInfo.userGivenName },
                { "user_family_name", userInfo.userFamilyName },
            };
            string jsonString = JsonConvert.SerializeObject(appDataObject);
            File.WriteAllText(appDataFilePath, jsonString);
        }

        private UserInfo LoadUserInfo()
        {
            string appDataFilePath = GetAppDataFilePath();
            if (!File.Exists(appDataFilePath))
            {
                return null;
            }
            string txt = File.ReadAllText(appDataFilePath);
            Dictionary<string, string> appDataObject =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(txt);
            return new UserInfo(
                appDataObject["user_id"],
                appDataObject["user_email"],
                appDataObject["user_given_name"],
                appDataObject["user_family_name"],
                appDataObject["refresh_token"]);
        }

        private string GetAppDataFilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string dirPath = Path.Combine(path, appName);
            if (!File.Exists(dirPath))
            {
                _ = Directory.CreateDirectory(dirPath);
            }
            return Path.Combine(path, appName, "app-data.json");
        }

        private async Task<UserInfo> UserInfoCall(string accessToken, string refreshToken)
        {
            Debug.WriteLine("Making API Call to Userinfo...");

            // builds the request
            string userinfoRequestURI = "https://www.googleapis.com/oauth2/v3/userinfo";
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", accessToken));
            userinfoRequest.ContentType = "application/json";
            userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            // gets the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
            string userinfoResponseText;
            using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
            {
                // reads response body
                userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
                Debug.WriteLine(userinfoResponseText);
            }
            Dictionary<string, string> userInfo =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(userinfoResponseText);
            return new UserInfo(
                userInfo["sub"],
                userInfo["email"], 
                userInfo["given_name"],
                userInfo["family_name"],
                refreshToken);
        }

        private static string RandomDataBase64url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64urlencodeNoPadding(bytes);
        }

        private static string Base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);
            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");
            return base64;
        }

        private static byte[] Sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        private static int GetRandomUnusedPort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

    }
}
