// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Microsoft.Extensions.Logging;
using Ellucian.Dmi.Client;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Offers login, logout, and change password services.
    /// </summary>
    public class SessionController : BaseCompressedApiController
    {
        private readonly ISessionRepository sessionRepository;
        private readonly ILogger logger;
        private readonly ISessionRecoveryService sessionRecoveryService;

        /// <summary>
        /// SessionsController constructor
        /// </summary>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="sessionRepository">Repository of type <see cref="ISessionRepository">ISessionRepository</see></param>
        /// <param name="sessionRecoveryService">Session recovery service</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SessionController(ILogger logger, ISessionRepository sessionRepository, ISessionRecoveryService sessionRecoveryService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.sessionRepository = sessionRepository;
            this.sessionRecoveryService = sessionRecoveryService;
        }

        /// <summary>
        /// Log in to Colleague.
        /// </summary>
        /// <param name="credentials">From Body, Login <see cref="Credentials">Credentials</see></param>
        /// <returns><see cref="string">string</see> with JSON Web Token</returns>
        [Obsolete("Obsolete as of API version 1.12, use PostLogin2Async instead")]
        [HttpPost]
        [HeaderVersionRoute("/session/login", 1, false, Name = "PostSessionLogin")]
        public async Task<ActionResult<string>> PostLoginAsync([FromBody] Credentials credentials)
        {
            bool hasName = Request.Headers.TryGetValue("X-ProductName", out StringValues nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValue("X-ProductVersion", out StringValues versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new ContentResult() { Content = await sessionRepository.LoginAsync(credentials.UserId, credentials.Password), StatusCode = StatusCodes.Status200OK };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    if (lex.ErrorCode == "10017")
                        return StatusCode(StatusCodes.Status403Forbidden, "You must change your password. Please choose a new password.");
                    else
                        return StatusCode(StatusCodes.Status403Forbidden, "Your password has expired.  Please choose a new password.");
                }
                else
                {
                    return Unauthorized(lex.Message);
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Log in to Colleague.
        /// </summary>
        /// <param name="credentials">From Body, Login <see cref="Credentials">Credentials</see></param>
        /// <returns><see cref="string">string</see> A string representing the Colleague Web API session token to be used with all requests requiring authorization, 
        /// or one of the following failure responses: 
        /// HttpStatusCode.Forbidden : Password has expired, HttpStatusCode.Unauthorized : Invalid credentials provided, or HttpStatusCode.NotFound : Listener not found or not responding
        /// </returns>
        [HttpPost]
        [HeaderVersionRoute("/session/login", 2, true, Name = "PostSessionLogin2")]
        public async Task<ActionResult<string>> PostLogin2Async([FromBody] Credentials credentials)
        {
            bool hasName = Request.Headers.TryGetValue("X-ProductName", out StringValues nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValue("X-ProductVersion", out StringValues versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new ContentResult() { Content = await sessionRepository.LoginAsync(credentials.UserId, credentials.Password), StatusCode = StatusCodes.Status200OK };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    logger.LogInformation(lex, "Login attempt failed due to expired password.");
                    return StatusCode(StatusCodes.Status403Forbidden, lex.Message + "Error: " + lex.ErrorCode);
                }
                // Check if login failure is due to reaching the maximum number of login attempts.
                else if (lex.ErrorCode == "10014")
                {
                    logger.LogInformation(lex, "Login attempt failed due to too many incorrect login attempts for User: " + credentials.UserId);
                    return Unauthorized(lex.Message + "Error: " + lex.ErrorCode);
                }
                else
                {
                    return Unauthorized(lex.Message);
                }
            }
            catch (ColleagueDmiConnectionException cdce)
            {
                logger.LogError("Login attempt failed with ColleagueDmiConnectionException: " + cdce.Message);
                return NotFound("Listener was not found or was unresponsive.");
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Log in to Colleague using proxy authentication.
        /// </summary>
        /// <param name="proxyCredentials">From Body, <see cref="ProxyCredentials">ProxyCredentials</see></param>
        /// <returns><see cref="string">ActionResult</see> with JSON Web Token</returns>
        [Obsolete("Obsolete as of API version 1.12, use PostProxyLogin2Async instead")]
        [HttpPost]
        [HeaderVersionRoute("/session/proxy-login", 1, false, Name = "PostSessionProxyLogin")]
        public async Task<ActionResult<string>> PostProxyLoginAsync([FromBody] ProxyCredentials proxyCredentials)
        {
            bool hasName = Request.Headers.TryGetValue("X-ProductName", out StringValues nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValue("X-ProductVersion", out StringValues versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new ContentResult()
                {
                    Content = await sessionRepository.ProxyLoginAsync(
                        proxyCredentials.ProxyId, proxyCredentials.ProxyPassword, proxyCredentials.UserId),
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, lex.Message);
                }
                else
                {
                    return Unauthorized(lex.Message);
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Log in to Colleague using proxy authentication.
        /// </summary>
        /// <param name="proxyCredentials">From Body, <see cref="ProxyCredentials">ProxyCredentials</see></param>
        /// <returns><see cref="ActionResult">ActionResult</see> A string representing the Colleague Web API session token to be used with all requests requiring authorization, 
        /// or one of the following failure responses: 
        /// HttpStatusCode.Forbidden : Password has expired, HttpStatusCode.Unauthorized : Invalid credentials provided, or HttpStatusCode.NotFound : Listener not found or not responding
        /// </returns>
        /// <note>This request supports anonymous access. No Colleague entity data is exposed via this anonymous request. See :ref:`anonymousapis` for additional information.</note>
        [HttpPost]
        [HeaderVersionRoute("/session/proxy-login", 2, true, Name = "PostSessionProxyLogin2")]
        public async Task<ActionResult<string>> PostProxyLogin2Async([FromBody] ProxyCredentials proxyCredentials)
        {
            bool hasName = Request.Headers.TryGetValue("X-ProductName", out StringValues nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValue("X-ProductVersion", out StringValues versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return Content(await sessionRepository.ProxyLoginAsync(
                        proxyCredentials.ProxyId, proxyCredentials.ProxyPassword, proxyCredentials.UserId));
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    logger.LogInformation(lex, lex.Message);
                    return StatusCode(StatusCodes.Status403Forbidden, lex.Message);
                }
                else
                {
                    return Unauthorized(lex.Message);
                }
            }
            catch (ColleagueDmiConnectionException cdce)
            {
                logger.LogError("Login attempt failed with ColleagueDmiConnectionException: " + cdce.Message);
                return NotFound("Listener was not found or was unresponsive.");
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Log out of Colleague
        /// </summary>
        [HttpPost]
        [HeaderVersionRoute("/session/logout", 1, true, Name = "PostSessionLogout")]
        public async Task<IActionResult> PostLogoutAsync()
        {
            /*
             * API 1.2 changes:
             * 1) Token should no longer be sent to this action using a query string parameter.
             *    The standard X-CustomCredentials header must be used.
             * 2) Token argument has been removed so that the API self-doc does not show it with
             *    the endpoint URI.
             * 3) To ensure logout if the query string parameter is used, this action will parse
             *    the query string from the raw request in order to still process the logout, 
             *    BUT will result in a bad request response and a log entry stating that the
             *    token query string parameter was supplied.
             */

            string token = null;
            string tokenParameter = "token";

            // Fetch the token from the credentials header
            Request.Headers.TryGetValue(Client.ColleagueApiClient.CredentialsHeaderKey, out StringValues xCustomCredentialsHeader);
            if (!StringValues.IsNullOrEmpty(xCustomCredentialsHeader) && xCustomCredentialsHeader.Any())
            {
                token = xCustomCredentialsHeader.First();
            }

            // Check to see if the token was passed via the query string and if present, use it, but also log as an error!
            var tokenParameterUsed = false;
            var queryParameters = Request.Query;
            if (queryParameters != null && queryParameters.Count > 0 && queryParameters.Keys.Contains(tokenParameter))
            {
                if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(queryParameters[tokenParameter]))
                {
                    token = queryParameters[tokenParameter];
                }
                tokenParameterUsed = true;
                logger.LogError("session/logout: token parameter passed via the query string from {0}! The token must be supplied using the credentials header.", Request.Host);
            }

            // logout
            try
            {
                await sessionRepository.LogoutAsync(token);
            }
            catch (Exception ex)
            {
                // Log it but don't re-throw
                logger.LogError(ex.ToString());
            }

            // if the token query parameter was used, issue an error
            if (tokenParameterUsed)
            {
                return CreateHttpResponseException("Logout called using an deprecated request format. The logout request was processed", HttpStatusCode.BadRequest);
            }

            return NoContent();
        }

        /// <summary>
        /// Gets token of user
        /// </summary>
        /// <param name="session"></param>
        /// <returns>User's token</returns>
        [HttpPost]
        [HeaderVersionRoute("/session/token", 1, true, Name = "PostSessionToken")]
        public async Task<ActionResult<string>> PostTokenAsync([FromBody] LegacyColleagueSession session)
        {
            return await sessionRepository.GetTokenAsync(session.SecurityToken, session.ControlId);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request"><see cref="ChangePassword">ChangePassword</see> request</param>
        /// <returns>NoContent <see cref="IActionResult">IActionResult</see> if successful</returns>
        /// <exception> <see cref="IActionResult">IActionResult</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest when passed data is not acceptable</exception>
        [HttpPost]
        [HeaderVersionRoute("/session/change-password", 1, true, Name = "PostSessionChangepassword")]
        public async Task<IActionResult> PostNewPasswordAsync([FromBody] ChangePassword request)
        {
            try
            {
                await sessionRepository.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            // no exceptions? no problem?
            return NoContent();
        }

        /// <summary>
        /// Obtains new JWT that includes new proxy access granted by the specified proxy subject to the current user. This also updates
        /// the web session token in Colleague for the current user. Note: "proxy subject" means the user that has granted
        /// proxy access to the current user.
        /// </summary>
        /// <param name="proxySubject">The proxy subject. Only the ID is required. If this ID is empty, or is 
        /// the same as the Current user ID, then any previously assigned proxy subject claims will be removed.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        [HeaderVersionRoute("/session/proxy-subjects", 1, true, Name = "PutSessionProxyAccess")]
        public async Task<IActionResult> PutSessionProxySubjectsAsync([FromBody] ProxySubject proxySubject)
        {
            try
            {
                string proxySubjectID = (proxySubject == null) ?
                                            string.Empty :
                                            (proxySubject.Id ?? string.Empty);

                string currentUserId = "";
                var currentUserPrincipal = User;
                var currentUserIdClaim = currentUserPrincipal.Identities.First().Claims.FirstOrDefault(
                    c => c.Type == Ellucian.Web.Security.ClaimConstants.PersonId);
                if (currentUserIdClaim != null)
                {
                    currentUserId = currentUserIdClaim.Value;
                }

                if (currentUserId == proxySubjectID)
                {
                    // If proxy subject ID is the current user's ID (proxying oneself), then the proxy subject claims  
                    // are to beremoved from the token. To achieve this, an empty string must be passed in to the repo method.
                    proxySubjectID = "";
                }

                return Content(await sessionRepository.SetProxySubjectAsync(proxySubjectID, currentUserPrincipal));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest(string.Format("Problem occurred obtaining proxy access for proxy subject {0}, please contact the system administrator.", proxySubject.Id));
            }
        }

        /// <summary>
        /// Executes a request for a reset password token to be emailed to the user to enable them to complete a password reset.
        /// </summary>
        /// <param name="tokenRequest">The reset password token request</param>
        /// <returns>A successful 202 response. For security reasons, failures will not be reported.</returns>
        /// <accessComments>User must have ADMIN.RESET.ALL.PASSWORDS permission to reset passwords.</accessComments>
        [HttpPost]
        [Authorize]
        [HeaderVersionRoute("/session/password-reset-token-request", 1, true, Name = "PostResetPasswordTokenRequest")]
        public async Task<IActionResult> PostResetPasswordTokenRequestAsync([FromBody] PasswordResetTokenRequest tokenRequest)
        {
            try
            {
                await sessionRecoveryService.RequestPasswordResetTokenAsync(tokenRequest.UserId, tokenRequest.EmailAddress);
                return Accepted();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to request password reset token request.");
                return Accepted();
            }
        }

        /// <summary>
        /// Request recovery of a user ID (user name) given supporting information and email.
        /// </summary>
        /// <param name="userIdRecoveryRequest">User ID Recovery Request</param>
        /// <returns>A successful 202 response. For security reasons, failures will not be reported.</returns>
        /// <accessComments>User must have ADMIN.RESET.ALL.PASSWORDS permission to reset passwords.</accessComments>
        [HttpPost]
        [Authorize]
        [HeaderVersionRoute("/session/recover-user-id", 1, true, Name = "PostUserIdRecoveryRequest")]
        public async Task<IActionResult> PostUserIdRecoveryRequestAsync([FromBody] UserIdRecoveryRequest userIdRecoveryRequest)
        {
            try
            {
                await sessionRecoveryService.RequestUserIdRecoveryAsync(userIdRecoveryRequest.FirstName, userIdRecoveryRequest.LastName, userIdRecoveryRequest.EmailAddress);
                return Accepted();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to recover user ID.");
                return Accepted();
            }
        }

        /// <summary>
        /// Resets a password using a password reset token
        /// </summary>
        /// <param name="resetPassword">Reset password</param>
        /// <returns></returns>
        /// <accessComments>User must have ADMIN.RESET.ALL.PASSWORDS permission to reset passwords.</accessComments>
        [HttpPost]
        [Authorize]
        [HeaderVersionRoute("/session/reset-password", 1, true, Name = "PostResetPassword")]
        public async Task<IActionResult> PostResetPasswordAsync([FromBody] ResetPassword resetPassword)
        {
            try
            {
                if (resetPassword == null || string.IsNullOrEmpty(resetPassword.UserId) || string.IsNullOrEmpty(resetPassword.ResetToken) || string.IsNullOrEmpty(resetPassword.NewPassword))
                {
                    throw new ArgumentNullException("resetPassword");
                }
                await sessionRecoveryService.ResetPasswordAsync(resetPassword.UserId, resetPassword.ResetToken, resetPassword.NewPassword);
                return Ok();
            }
            catch (PasswordComplexityException pce)
            {
                return CreateHttpResponseException("Password complexity failure");
            }
            catch (PasswordUsedException pue)
            {
                return CreateHttpResponseException("Password used recently failure");
            }
            catch (PasswordResetTokenExpiredException prtee)
            {
                return CreateHttpResponseException("Password reset token expired failure");
            }
            catch (Web.Security.PermissionsException pe)
            {
                return CreateHttpResponseException("Unable to reset password");
            }
            catch (Exception e)
            {
                return CreateHttpResponseException("Unable to reset password");
            }
        }

        /// <summary>
        /// Sync user's Colleague web token's timeout on app server side.
        /// </summary>
        /// <returns></returns>
        /// <note>Leave the body empty for this PUT request.</note>
        [HttpPut]
        [Authorize]
        [HeaderVersionRoute("/session/sync", 1, true, Name = "PutSync")]
        public async Task<IActionResult> PutSyncAsync()
        {
            try
            {
                var currentUserPrincipal = User;
                var currentUserTokenClaim = currentUserPrincipal.Identities.First().Claims.FirstOrDefault(
                    c => c.Type == Ellucian.Web.Security.ClaimConstants.SecurityTokenControlId);

                if (currentUserTokenClaim == null
                    || string.IsNullOrWhiteSpace(currentUserTokenClaim.Value)
                    || currentUserTokenClaim.Value.Split('*').Length != 2)
                {
                    logger.LogError("Unable to sync session due to invalid token claim.");
                    return BadRequest();
                }
                string secToken = currentUserTokenClaim.Value.Split('*')[0];
                string controlId = currentUserTokenClaim.Value.Split('*')[1];
                await sessionRepository.SyncSessionAsync(secToken, controlId);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unexpected error occurred syncing web session.");
                return BadRequest();
            }
        }

        /// <summary>
        /// Requests a Multifactor Token (one-time password) be emailed to the user.
        /// Returns a temporary session token that must be included with the follow-up
        /// call to verify the multifactor token with the emailed OTP.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/session/multifactor-token", 1, true, Name = "MultifactorTokenRequest")]
        public async Task<IActionResult> RequestMultifactorTokenAsync([FromBody] Credentials credentials)
        {
            bool hasName = Request.Headers.TryGetValue("X-ProductName", out StringValues nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValue("X-ProductVersion", out StringValues versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                var response = await sessionRepository.GetMultifactorTokenAsync(credentials.UserId, credentials.Password);
                return new ContentResult() { Content = response };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    logger.LogInformation(lex, "Login attempt failed due to expired password.");
                    return CreateHttpResponseException(lex.Message + "Error: " + lex.ErrorCode, HttpStatusCode.Forbidden);
                }
                // Check if login failure is due to reaching the maximum number of login attempts.
                else if (lex.ErrorCode == "10014")
                {
                    logger.LogInformation(lex, "Login attempt failed due to too many incorrect login attempts for User: " + credentials.UserId);
                    return Unauthorized(lex.Message + "Error: " + lex.ErrorCode);
                }
                else
                {
                    return Unauthorized(lex.Message);
                }
            }
            catch (ColleagueDmiConnectionException cdce)
            {
                logger.LogError("Login attempt failed with ColleagueDmiConnectionException: " + cdce.Message);
                return NotFound("Listener was not found or was unresponsive.");
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Verify the multifactor token with the provided temporary session token
        /// and the emailed one-time password.
        /// Successful requests will result in a full session being created and 
        /// a response of the session JWT.
        /// </summary>
        /// <returns>Session JWT</returns>
        [HttpPost]
        [HeaderVersionRoute("/session/verify-multifactor-token", 1, true, Name = "VerifyMultifactorToken")]
        public async Task<ActionResult<string>> VerifyMultifactorTokenAsync([FromBody] CredentialsWithMultifactorToken credentialsWithMfaToken)
        {
            bool hasName = Request.Headers.TryGetValue("X-ProductName", out StringValues nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValue("X-ProductVersion", out StringValues versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new ContentResult()
                {
                    Content = await sessionRepository.LoginWithMultifactorAsync(
                        credentialsWithMfaToken.UserId,
                        credentialsWithMfaToken.ServiceToken, credentialsWithMfaToken.MultifactorOneTimePassword)
                };
            }
            catch (VerifyMultifactorException vme)
            {
                logger.LogInformation(vme, "Verify multifactor token failure: (" + vme.StatusCode + ") " + vme.Message);
                return Unauthorized(vme.StatusCode + ": " + vme.Message);
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    logger.LogInformation(lex, lex.Message);
                    return CreateHttpResponseException(lex.Message, HttpStatusCode.Forbidden);
                }
                else
                {
                    return Unauthorized(lex.Message);
                }
            }
            catch (ColleagueDmiConnectionException cdce)
            {
                logger.LogError("Login attempt failed with ColleagueDmiConnectionException: " + cdce.Message);
                return NotFound("Listener was not found or was unresponsive.");
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
