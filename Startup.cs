using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BPLSWebSSOApp
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        private readonly ILogger<Startup> _logger;

        //public Startup(IWebHostEnvironment environment, IConfiguration config, ILogger<Startup> logger)
        public Startup(IWebHostEnvironment environment, IConfiguration config)
        {
            Environment = environment;
            Configuration = config;
            //_logger = logger;// loggerFactory.CreateLogger<Startup>();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
            // Configure the HTTP request pipeline.

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSerilogRequestLogging();
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Login}/{id?}"
                    );
            });
        }
        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

                if (DisallowsSameSiteNone(userAgent))
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }
        private static bool DisallowsSameSiteNone(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking stack
            if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }
        private void LogInfo(string keyOrTitle, string value)
        {
            Log.Information($"{keyOrTitle}:{value}");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });
            #endregion

            services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
           .AddCookie()
            #region
           //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
           //{

           //    // Use the strongest setting in production, which also enables HTTP on developer workstations
           //    options.Cookie.SameSite = SameSiteMode.Strict;
           //})
            #endregion
           .AddOpenIdConnect(options =>
           {

               #region
               // Use the same settings for temporary cookies
               //options.NonceCookie.SameSite = SameSiteMode.Unspecified;
               //options.CorrelationCookie.SameSite = SameSiteMode.Strict;
               #endregion

               try
               {
                   LogInfo("#Info:", "-------------------------------------------------------------------------------------------");
                   LogInfo("Authentication Properties", "Information provided in middle layer....");

                   options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   LogInfo("SignInScheme", options.SignInScheme);

                   options.Authority = Configuration.GetValue<string>("OpenIdConnect:Issuer");
                   LogInfo("Authority", options.Authority?.ToString());

                   options.ClientId = Configuration.GetValue<string>("OpenIdConnect:ClientId");
                   LogInfo("ClientId", options.ClientId?.ToString());

                   options.ClientSecret = Configuration.GetValue<string>("OpenIdConnect:ClientSecret");
                   LogInfo("ClientSecret", options.ClientSecret?.ToString());

                   options.CallbackPath = Configuration.GetValue<string>("OpenIdConnect:RedirectURL");
                   LogInfo("CallbackPath", options.CallbackPath.ToString());

                   options.ResponseType = OpenIdConnectResponseType.Code;
                   LogInfo("ResponseType", options.ResponseType);

                   options.ResponseMode = OpenIdConnectResponseMode.Query;
                   LogInfo("ResponseMode", options.ResponseMode);

                   string? scopeString = Configuration.GetValue<string>("OpenIDConnect:Scope");
                   options.Scope.Clear();
                   scopeString?.Split(" ", StringSplitOptions.TrimEntries).ToList().ForEach(scope =>
                   {
                       options.Scope.Add(scope);
                   });
                   LogInfo("Scope", scopeString.ToString());

                   options.SaveTokens = true;
                   LogInfo("SaveTokens", "CookieAuthenticationDefaults.AuthenticationScheme");

                   options.GetClaimsFromUserInfoEndpoint = true;
                   LogInfo("GetClaimsFromUserInfoEndpoint", "true");

                   options.RequireHttpsMetadata = false;
                   LogInfo("RequireHttpsMetadata", "false");

                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidIssuer = options.Authority,
                       NameClaimType = "name"
                   };
                   LogInfo("###:", "-------------------------------------------------------------------------------------------");
               }
               catch (Exception ex)
               {
                   LogInfo("#Error:", "-------------------------------------------------------------------------------------------");
                   LogInfo("MiddleLayer Auth Config Error", ex.Message);
                   LogInfo("###:", "-------------------------------------------------------------------------------------------");
               }
               #region
               //options.Events = new OpenIdConnectEvents
               //{
               //    OnRedirectToIdentityProvider = context => {

               //        context.ProtocolMessage.RedirectUri = Configuration.GetValue<string>("OpenIdConnect:RedirectURL");
               //        context.ProtocolMessage.PostLogoutRedirectUri = Configuration.GetValue<string>("OpenIdConnect:PostLogoutRedirectUri");

               //        return System.Threading.Tasks.Task.FromResult(0);
               //    }
               //};

               // This example gets user information for display from the user info endpoint


               //options.Events = new OpenIdConnectEvents
               //{
               //    OnTicketReceived = e =>
               //    {
               //        e.ReturnUri = Configuration.GetValue<string>("OpenIdConnect:RedirectURL");
               //        return Task.CompletedTask;
               //    }
               //};

               //options.Events.OnRedirectToIdentityProvider = (context) =>
               //{
               //    context.ProtocolMessage.State = Guid.NewGuid().ToString();
               //    return Task.CompletedTask;
               //};

               //// Handle the post logout redirect URI
               //options.Events.OnRedirectToIdentityProviderForSignOut = (context) =>
               //{
               //    context.ProtocolMessage.PostLogoutRedirectUri = Configuration.GetValue<string>("OpenIdConnect:PostLogoutRedirectUri");
               //    return Task.CompletedTask;
               //};

               //// Save tokens issued to encrypted cookies
               //// Set this in developer setups if the OpenID Provider uses plain HTTP
               /* Uncomment to debug HTTP requests from the web backend to the Identity Server
                  Run a tool such as MITM proxy to view the request and response messages
               /*options.BackchannelHttpHandler = new HttpClientHandler()
               {
                   Proxy = new WebProxy("http://127.0.0.1:8888"),
                   UseProxy = true,
               };*/
               #endregion
           });

            services.AddAuthorization();
            services.AddControllersWithViews();
        }
    }
}