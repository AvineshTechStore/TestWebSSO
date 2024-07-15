using BPLSWebSSOApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using OidcClientDemoApplication;
using Serilog;
using System.Text;

namespace BPLSWebSSOApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public LoginModel _loginModel;
        private StringBuilder _sbClaims = new StringBuilder();
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _loginModel = new LoginModel(new TokenClient(configuration));
        }

        public IActionResult Index()
        {
            return GetClaims();
        }

        public IActionResult Login()
        {
            return GetClaims();
        }

        private IActionResult GetClaims()
        {
            try
            {
                Log.Information("#Info:-------------------------------------------------------------------------------------------");
                Log.Information("Received response from IDP server..");

                ClaimsPrincipal user = this.User;
                var givenName = user.FindFirstValue("given_name");
                var familyName = user.FindFirstValue("family_name");
                var email = user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                _loginModel.Username = $"{givenName} {familyName}";
                _loginModel.GivenName = givenName;
                _loginModel.FamilyName = familyName;
                _loginModel.Email = email;

                _sbClaims.Append($"GivenName:{givenName}, FamilyName:{familyName}, Email:{email}");
                Log.Information(_sbClaims.ToString());
                Log.Information("###:-------------------------------------------------------------------------------------------");

                return View(_loginModel);
            }
            catch (Exception ex)
            {
                Log.Information("#Error:-------------------------------------------------------------------------------------------");
                Log.Information($"After redirect back from IDP server, error received: {ex.Message}");
                Log.Information("###:-------------------------------------------------------------------------------------------");

                return new EmptyResult();
            }
        }

        public async Task OnPostLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
