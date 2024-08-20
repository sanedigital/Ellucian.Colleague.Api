// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Middleware
{
    /// <summary>
    /// Allows for the type for the handler
    /// </summary>
    public class ColleagueJwtAuthorizationOptions : AuthenticationSchemeOptions
    {
        
    }

    /// <summary>
    /// Provides an HTTP module that enables basic authentication  with Colleague API.
    /// </summary>
    public class ColleagueJwtAuthenticationHandler : AuthenticationHandler<ColleagueJwtAuthorizationOptions>
    {
        private readonly ILogger _logger;
        private readonly JwtHelper _jwtHelper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        /// <param name="jwtHelper"></param>
        public ColleagueJwtAuthenticationHandler(IOptionsMonitor<ColleagueJwtAuthorizationOptions> options, ILoggerFactory logger, 
            UrlEncoder encoder, ISystemClock clock, JwtHelper jwtHelper)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger(typeof(ILogger));
            _jwtHelper = jwtHelper;
        }

        /// <summary>
        /// Sets the current principal.
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="principal">The principal to set.</param>
        private void SetPrincipal(HttpContext context, ClaimsPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (context != null)
            {
                context.User = principal;
            }
        }

        /// <summary>
        /// See if this user has JWT to authenticate
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // We only accept our claims-based authentication 
            // via the JWT string, which is the X-CustomCredentials header. 
            // Any other authentication will be invalidated.
            
            var jwt = Context.Request.Headers[Client.ColleagueApiClient.CredentialsHeaderKey];
            if (!string.IsNullOrEmpty(jwt))
            {
                try
                {
                    var principal = JwtHelper.CreatePrincipal(jwt);
                    _jwtHelper.ValidateAntiForgeryClaim(principal.Identities.First().Claims);
                    SetPrincipal(Context, principal);
                    return AuthenticateResult.Success(new AuthenticationTicket(principal, "JWT"));
                }
                catch (TokenValidationException tve)
                {
                    _logger.LogError(tve, "Invalid session token detected");
                    Context.User = null;
                    return AuthenticateResult.Fail("Invalid session token detected");
                }
                catch (Exception exc)
                {
                    // Invalidate the user in an unexpected event to be safe.
                    Context.User = null;
                    throw;
                }
            }
            else
            {
                // X-CustomCredentials was not present, but the principal could have been set by another auth handler, so as long as it's JWT let it through.
                if (Context.User != null && Context.User.Identity?.AuthenticationType != null)
                {
                    if (!Context.User.Identity.AuthenticationType.Equals("JWT", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Context.User = null;
                        return AuthenticateResult.Fail("Invalid authentication scheme");
                    }
                    else
                    {
                        return AuthenticateResult.Success(new AuthenticationTicket(Context.User, "JWT"));
                    }
                }
            }
            return AuthenticateResult.NoResult();
        }
    }
    
}
