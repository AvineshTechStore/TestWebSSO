namespace BPLSWebSSOApp
{
    public class AuthConfigs
    {
        public const string OpenIDConnect = "OpenIDConnect";

        public string ClientId { get; set; } = String.Empty;
        public string ClientSecret { get; set; } = String.Empty;
        public string Issuer { get; set; } = String.Empty;
        public string Scope { get; set; } = String.Empty;
        public string PostLogoutRedirectUri { get; set; } = String.Empty;
        public string TokenEndpoint { get; set; } = String.Empty;
        public string RedirectURL { get; set; } = String.Empty;
    }
}
