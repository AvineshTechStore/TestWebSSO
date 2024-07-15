using BPLSWebSSOApp.Models;
using Microsoft.AspNetCore.Mvc;
using OidcClientDemoApplication;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace BPLSWebSSOApp.Controllers
{
    public class LoginController : Controller
    {
        private StringBuilder _sbClaims = new StringBuilder();
        public LoginModel _loginModel;
        public LoginController(IConfiguration configuration)
        {
            _loginModel = new LoginModel(new TokenClient(configuration));
        }

        public IActionResult Login()
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
    }
}
