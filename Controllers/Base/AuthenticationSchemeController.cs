// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Endpoints for authentication schemes
    /// </summary>
    // Lack of authorize attribute is intentional.
    // The endpoints for retrieving the authorization scheme need to be called
    // before the user has logged in.
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AuthenticationSchemeController : BaseCompressedApiController
    {
        private readonly IAuthenticationSchemeService authenticationSchemeService;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for Authentication Scheme controller
        /// </summary>
        /// <param name="authenticationSchemeService">Authentication scheme service</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AuthenticationSchemeController(IAuthenticationSchemeService authenticationSchemeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.authenticationSchemeService = authenticationSchemeService;
            this.logger = logger;

        }

        /// <summary>
        /// Retrieves the authentication scheme for the given username.
        /// </summary>
        /// <param name="username">Username for which to retrieve the authentication scheme</param>
        /// <returns>Authentication scheme for the given username. Null if the user did not have an authentication scheme defined.</returns>
        /// <accessComments>This endpoint is accessible anonymously.</accessComments>
        /// <note>This request supports anonymous access. No Colleague entity data is exposed via this anonymous request. See :ref:`anonymousapis` for additional information.</note>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "username")]
        [HeaderVersionRoute("/authentication-scheme", 1, true, Name = "GetAuthenticationSchemeAsync")]
        public async Task<ActionResult<AuthenticationScheme>> GetAuthenticationSchemeAsync([FromQuery]string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentNullException("username");
                }
                return await authenticationSchemeService.GetAuthenticationSchemeAsync(username);
            }
            catch (Exception e)
            {
                logger.LogError(e, string.Format("Failed to retrieve authentication scheme for user: {0}", username));
                return CreateHttpResponseException("Could not retrieve authentication scheme.");
            }

        }
    }
}
