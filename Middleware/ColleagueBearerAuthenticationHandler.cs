// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;

namespace Ellucian.Colleague.Api.Middleware
{

    /// <summary>
    /// Allows for the type for the handler
    /// </summary>
    public class ColleagueBearerAuthorizationOptions : AuthenticationSchemeOptions
    {

    }

    /// <summary>
    /// Provides an HTTP module that enables basic authentication  with Colleague API.
    /// </summary>
    public class ColleagueBearerAuthenticationHandler : AuthenticationHandler<ColleagueBearerAuthorizationOptions>
    {
        private readonly ILogger _logger;
        private readonly ISessionRepository _sessionRepository;
        private readonly ISettingsRepository _settingsRepository;
        private const string AuthorizationHeaderKey = "authorization";
        private const string AuthorizationScheme = "bearer";
        private const string ProductNameHeaderKey = "X-ProductName";
        private const string ProductVersionHeaderKey = "X-ProductVersion";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="sessionRepository"></param>
        /// <param name="settingsRepository"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        public ColleagueBearerAuthenticationHandler(IOptionsMonitor<ColleagueBearerAuthorizationOptions> options, ISessionRepository sessionRepository, ISettingsRepository settingsRepository,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
             : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger(typeof(ILogger)) ;
            _sessionRepository = sessionRepository;
            _settingsRepository = settingsRepository;
        }

        /// <summary>
        /// The middleware action to see if we are OK handling this request
        /// </summary>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // read header
            var request = Context.Request;
            var authHeader = request.Headers[AuthorizationHeaderKey];

            if (!string.IsNullOrEmpty(authHeader))
            {
                // read header value
                AuthenticationHeaderValue authHeaderValue = null;
                if (AuthenticationHeaderValue.TryParse(authHeader, out authHeaderValue))
                {
                    // value present, proceed if basic auth scheme
                    if (authHeaderValue != null &&
                        authHeaderValue.Scheme.Equals(AuthorizationScheme, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(authHeaderValue.Parameter))
                    {
                        var settings = _settingsRepository.Get();

                        // Validate the Token
                        var validToken = await ValidateCurrentToken(Context, authHeaderValue.Parameter, settings);

                        if (validToken)
                        {
                            // Get Person Login ID from GUID
                            var handler = new JwtSecurityTokenHandler();
                            var jsonToken = handler.ReadJwtToken(authHeaderValue.Parameter);
                            var personGuid = jsonToken.Subject;

                            // Get Oauth Proxy User Name and Password
                            string proxyId = string.Empty;
                            string password = string.Empty;
                            if (settings != null && settings.OauthSettings != null)
                            {
                                proxyId = settings.OauthSettings.OauthProxyLogin;
                                password = settings.OauthSettings.OauthProxyPassword;
                            }

                            // check for an existing token
                            Tuple<bool, string, ClaimsPrincipal> existingSessionTuple = await TryGetExistingSessionAsync(proxyId, password, personGuid);
                            bool tokenExists = existingSessionTuple.Item1;
                            var username = existingSessionTuple.Item2;
                            var principal = existingSessionTuple.Item3;

                            if (tokenExists && principal != null)
                            {
                                SetPrincipal(Context, principal);
                                return AuthenticateResult.Success(new AuthenticationTicket(principal, "Bearer"));
                            }
                            else
                            {
                                //login if no token
                                try
                                {
                                    if (string.IsNullOrEmpty(proxyId) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(personGuid))
                                    {
                                        throw new Exception("Login failed: The Oauth Token represents an invalid username or password. Please try again.");
                                    }
                                    Tuple<bool, ClaimsPrincipal> authenticationTuple = await AuthenticateUserAsync(proxyId, password, username, personGuid, Context.Request.Headers);
                                    bool validated = authenticationTuple.Item1;
                                    principal = authenticationTuple.Item2;

                                    if (validated && principal != null)
                                    {
                                        SetPrincipal(Context, principal);
                                        return AuthenticateResult.Success(new AuthenticationTicket(principal, "Bearer"));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message.Contains("password has expired"))
                                    {
                                        WriteResponse(Context, HttpStatusCode.Forbidden, ex.Message);
                                    }
                                    else
                                    {
                                        WriteResponse(Context, HttpStatusCode.Unauthorized, ex.Message);
                                    }
                                    // we're done... this will end the request in the pipeline!
                                    return AuthenticateResult.Fail(ex.Message);
                                }
                            }
                        }
                        else
                        {
                            return AuthenticateResult.Fail("Invalid bearer token");
                        }
                    }
                }
            }
            return AuthenticateResult.NoResult();

        }

        /// <summary>
        /// Attempts to perform a standard login against Colleague using the provided basic credentials as
        /// an asynchronous transaction.
        /// </summary>
        /// <param name="proxyId">The Oauth Proxy UserName.</param>
        /// <param name="password">The Oauth Proxy password.</param>
        /// <param name="personGuid">The person guid for context extracted from the token.</param>
        /// <param name="username">The person userID for logging into Colleague.</param>
        /// <param name="headers">Context Header Values from Http Request.</param>
        /// <returns>
        /// Tuple result:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful authentication.
        /// </returns>
        /// <exception cref="System.FormatException">Invalid basic credentials format</exception>
        private async Task<Tuple<bool, ClaimsPrincipal>> AuthenticateUserAsync(string proxyId, string password, string username, string personGuid,  IHeaderDictionary headers)
        {
            ClaimsPrincipal principal = null;
            bool validated = false;

            // execute login
            Stopwatch sw = null;
            string jwt = null;
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    sw = new Stopwatch();
                    sw.Start();
                }
                string product = null;
                string version = null;

                GetProductNameAndVersion(headers, out product, out version);
                _sessionRepository.ProductName = product;
                _sessionRepository.ProductVersion = version;
                jwt = await _sessionRepository.ProxyLoginAsync(proxyId, password, username, personGuid);
            }
            catch (Exception e)
            {
                // need to let this bubble up so calling code can return proper HTTP response
                _logger.LogError(e, "Bearer AuthenticateUser error");
                throw;
            }
            finally
            {
                if (_logger.IsEnabled(LogLevel.Debug) && sw != null)
                {
                    sw.Stop();
                    _logger.LogDebug(string.Format("Bearer AuthenticateUser: {0} seconds", sw.Elapsed.ToString()));
                }
            }

            // get principal
            try
            {
                if (!string.IsNullOrEmpty(jwt))
                {
                    principal = JwtHelper.CreatePrincipal(jwt);
                    if (principal != null)
                    {
                        validated = true;
                    }
                }
            }
            catch
            {
                validated = false;
            }

            return new Tuple<bool, ClaimsPrincipal>(validated, principal);
        }

        /// <summary>
        /// Attempts to retrieve an existing Colleague session as an asynchronous transaction
        /// using the provided basic credentials.
        /// </summary>
        /// <param name="proxyId">The Oauth Proxy ID from settings.config.</param>
        /// <param name="password">The Oauth Password from settings.config.</param>
        /// <param name="personGuid">The GUID for the person from the credentials subject used for context.</param>
        /// <returns>
        /// Tuple:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful retrieval of an existing session.
        /// </returns>
        private async Task<Tuple<bool, string, ClaimsPrincipal>> TryGetExistingSessionAsync(string proxyId, string password, string personGuid)
        {
            ClaimsPrincipal principal = null;
            bool success = false;
            string username = "";

            Stopwatch sw = null;
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    sw = new Stopwatch();
                    sw.Start();
                }

                var jwtTuple = await _sessionRepository.GetOauthProxyTokenAsync(proxyId, password, personGuid);
                string jwt = jwtTuple.Item1;
                username = jwtTuple.Item2;

                if (!string.IsNullOrEmpty(jwt))
                {
                    principal = JwtHelper.CreatePrincipal(jwt);
                    if (principal != null)
                    {
                        success = true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Bearer TryGetExistingSession error");
                success = false;
            }
            finally
            {
                if (_logger.IsEnabled(LogLevel.Debug) && sw != null)
                {
                    sw.Stop();
                    _logger.LogDebug(string.Format("Basic TryGetExistingSession: {0} seconds", sw.Elapsed.ToString()));
                }
            }
            return new Tuple<bool, string, ClaimsPrincipal>(success, username, principal);
        }

        /// <summary>
        /// Sets the current principal.
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="principal">The principal to set.</param>
        private static void SetPrincipal(HttpContext context, ClaimsPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (context != null)
            {
                context.User = principal;
            }
        }

        /// <summary>
        /// Writes the provided status code and string content to the HTTP response stream.
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="statusCode"><see cref="HttpStatusCode"/> to return.</param>
        /// <param name="content">String content to return.</param>
        private static void WriteResponse(HttpContext context, HttpStatusCode statusCode, string content)
        {
            context.Response.StatusCode = (int)statusCode;
            if (!string.IsNullOrEmpty(content))
            {
                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.WriteAsync(content);
            }
        }

        /// <summary>
        /// Gets the values of the product name and product version headers. Both must be present otherwise
        /// default values are returned.
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="productName">The value of the product name header.</param>
        /// <param name="productVersion">The value of the product version header.</param>
        private static void GetProductNameAndVersion(IHeaderDictionary headers, out string productName, out string productVersion)
        {
            productName = null;
            productVersion = null;

            string productNameHeaderValue = headers[ProductNameHeaderKey];
            string productVersionHeaderValue = headers[ProductVersionHeaderKey];

            if (!string.IsNullOrEmpty(productNameHeaderValue))
            {
                productName = productNameHeaderValue;
            }

            if (!string.IsNullOrEmpty(productVersionHeaderValue))
            {
                productVersion = productVersionHeaderValue;
            }

            // both need to be supplied, if not, use defaults
            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productVersion))
            {
                productName = "WebApi";
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                productVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }
        }

        /// <summary>
        /// Validate the input security token
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token">Jwt Bearer Security Token</param>
        /// <param name="settings"></param>
        /// <returns>
        /// boolean: Validation, True if successful, False if not.
        /// </returns>
        private async Task<bool> ValidateCurrentToken(HttpContext context, string token, Settings settings)
        {
            Stopwatch sw = null;

            var myIssuer = "https://oauth.prod.10005.elluciancloud.com";
            var myAudience = "https://elluciancloud.com";

            if (settings != null && settings.OauthSettings != null)
            {
                myIssuer = settings.OauthSettings.OauthIssuerUrl;
            }

            try
            {
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{myIssuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

                var tokenHandler = new JwtSecurityTokenHandler();
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    sw = new Stopwatch();
                    sw.Start();
                }
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = myIssuer,
                    ValidAudience = myAudience,
                    IssuerSigningKeys = openIdConfig != null ? openIdConfig.SigningKeys : null
                };
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                if (ex.Message.Contains("expired"))
                {
                    WriteResponse(context, HttpStatusCode.Unauthorized, "Login failed: The oauth token has expired.");
                }
                else
                {
                    WriteResponse(context, HttpStatusCode.Unauthorized, "Login failed: The oauth token validation failed.");
                }
                // we're done... this will end the request in the pipeline!
                return false;
            }
            finally
            {
                if (_logger.IsEnabled(LogLevel.Debug) && sw != null)
                {
                    sw.Stop();
                    _logger.LogDebug(string.Format("Oauth Token Validation: {0} seconds", sw.Elapsed.ToString()));
                }
            }

            return true;
        }

        /// <summary>
        /// Get a requested claim type from the bearer security token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        private static string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadJwtToken(token);

            var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return stringClaimValue;
        }
    }
}
