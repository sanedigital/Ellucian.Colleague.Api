// Copyright 2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Client;

namespace Ellucian.Web.Http.Utilities
{
    /// <summary>
    /// SystemSystemLoginUtilities that contains static properties useful for interacting with the API as a security system login.
    /// </summary>
    public static class SecuritySystemLoginUtilities
    {
        /// <summary>
        /// Gets a service client for use in security system applications that require elevated privileges the user
        /// themselves will not have.
        /// </summary>
        /// <returns>An API client authenticated with security system login credentials</returns>
        public static ColleagueApiClient GetSecuritySystemLoginApiClient(string cookie, ILogger logger)
        {
            LocalUserUtilities.ParseCookie(cookie, out string baseUrl, out string token);
            var client = new ColleagueApiClient(baseUrl, 2, logger)
            {
                Credentials = token
            };
            return client;
        }
    }

}
