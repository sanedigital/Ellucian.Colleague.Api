// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
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
    public class ColleagueBasicAuthorizationOptions : AuthenticationSchemeOptions
    {

    }

    /// <summary>
    /// Provides an HTTP module that enables basic authentication  with Colleague API.
    /// </summary>
    public class ColleagueBasicAuthenticationHandler : AuthenticationHandler<ColleagueBasicAuthorizationOptions>
    {
        private readonly ILogger _logger;
        private readonly ISessionRepository _sessionRepository;
        private const string AuthorizationHeaderKey = "authorization";
        private const string AuthorizationScheme = "basic";
        private const string ProductNameHeaderKey = "X-ProductName";
        private const string ProductVersionHeaderKey = "X-ProductVersion";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="sessionRepository"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        public ColleagueBasicAuthenticationHandler(IOptionsMonitor<ColleagueBasicAuthorizationOptions> options, ISessionRepository sessionRepository,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger(typeof(ILogger));
            _sessionRepository = sessionRepository;
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
                        ClaimsPrincipal principal = null;

                        // check for an existing token
                        var existingSessionTuple = await TryGetExistingSessionAsync(authHeaderValue.Parameter);
                        bool tokenExists = existingSessionTuple.success;
                        principal = existingSessionTuple.principal;

                        if (tokenExists)
                        {
                            SetPrincipal(Context, principal);
                            return AuthenticateResult.Success(new AuthenticationTicket(principal, "Basic"));

                        }
                        else
                        {
                            //login if no token
                            try
                            {
                                var authenticationTuple = await AuthenticateUserAsync(authHeaderValue.Parameter, Context.Request.Headers);
                                bool validated = authenticationTuple.isValidated;
                                principal = authenticationTuple.principal;

                                if (validated)
                                {
                                    SetPrincipal(Context, principal);
                                    return AuthenticateResult.Success(new AuthenticationTicket(principal, "Basic"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("password has expired"))
                                {
                                    WriteResponse(Context, HttpStatusCode.Forbidden);
                                }
                                else
                                {
                                    WriteResponse(Context, HttpStatusCode.Unauthorized);
                                }

                                // we're done... this will end the request in the pipeline!
                                return AuthenticateResult.Fail(ex.Message);
                            }
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
        /// <param name="credentials">The base64 encoded basic authentication credentials.</param>
        /// <param name="headers"></param>
        /// <returns>
        /// Tuple result:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful authentication.
        /// </returns>
        /// <exception cref="System.FormatException">Invalid basic credentials format</exception>
        private async Task<(bool isValidated, ClaimsPrincipal principal)> AuthenticateUserAsync(string credentials, IHeaderDictionary headers)
        {
            ClaimsPrincipal principal = null;
            bool validated = false;
            string username = null;
            string password = null;

            // parse username/password
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));
                int separator = credentials.IndexOf(':'); // use the first occrance of a colon as the separator
                username = credentials.Substring(0, separator);
                password = credentials.Substring(separator + 1);
            }
            catch
            {
                throw new FormatException("Invalid basic credentials format");
            }


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
                jwt = await _sessionRepository.LoginAsync(username, password);
            }
            catch (Exception e)
            {
                // need to let this bubble up so calling code can return proper HTTP response
                _logger.LogError(e, "Basic AuthenticateUser error");
                throw;
            }
            finally
            {
                if (_logger.IsEnabled(LogLevel.Debug) && sw != null)
                {
                    sw.Stop();
                    _logger.LogDebug(string.Format("Basic AuthenticateUser: {0} seconds", sw.Elapsed.ToString()));
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

            return new(validated, principal);
        }

        /// <summary>
        /// Attempts to retrieve an existing Colleague session as an asynchronous transaction
        /// using the provided basic credentials.
        /// </summary>
        /// <param name="credentials">The base64 encoded basic authentication credentials.</param>
        /// <returns>
        /// Tuple:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful retrieval of an existing session.
        /// </returns>
        private async Task<(bool success, ClaimsPrincipal principal)> TryGetExistingSessionAsync(string credentials)
        {
            ClaimsPrincipal principal = null;
            bool success = false;

            Stopwatch sw = null;
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    sw = new Stopwatch();
                    sw.Start();
                }
                string jwt = await _sessionRepository.GetTokenAsync(credentials);
                principal = JwtHelper.CreatePrincipal(jwt);
                if (principal != null)
                {
                    success = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Basic TryGetExistingSession error");
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
            return new(success, principal);
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
        /// Writes the provided status code and string content to the HTTP response stream.
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="statusCode"><see cref="HttpStatusCode"/> to return.</param>
        private static void WriteResponse(HttpContext context, HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;
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

    }

  
}
