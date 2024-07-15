using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OidcClientDemoApplication;
using System.Security.Claims;

namespace BPLSWebSSOApp.Models
{
    public class LoginModel:PageModel
    {
        public string? Username { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Email { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        private readonly TokenClient tokenClient;
        public LoginModel(TokenClient tokenClient)
        {
            this.tokenClient = tokenClient;
        }
        public async Task<IActionResult> OnPostRefreshToken()
        {
            await this.tokenClient.RefreshAccessToken(this.HttpContext);
            this.AccessToken = await this.tokenClient.GetAccessToken(this.HttpContext);
            this.RefreshToken = await this.tokenClient.GetRefreshToken(this.HttpContext);
            return Page();
        }
        public async Task OnPostLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
