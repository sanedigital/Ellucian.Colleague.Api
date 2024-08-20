// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.ComponentModel;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to identity profile roles
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class IdentityProfileRolesController : BaseCompressedApiController
    {
        private readonly IIdentityProfileRolesService _identityProfileRolesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProfileRolesController"/>.
        /// </summary>
        /// <param name="identityProfileRolesService">Identity role service instance</param>
        /// <param name="logger">Logger intance</param>
        /// <param name="actionContextAccessor">Context accessor</param>
        /// <param name="apiSettings">Api settings</param>
        public IdentityProfileRolesController(IIdentityProfileRolesService identityProfileRolesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _identityProfileRolesService = identityProfileRolesService;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all active identity roles.
        /// </summary>
        /// <returns>List of all identity role titles</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/identity-profile-roles", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetIdentityProfileRoles", IsEedmSupported = true)]
        public async Task<IActionResult> GetIdentityProfileRolesAsync()
        {
            try
            {
                var bypassCache = Request.GetTypedHeaders().CacheControl?.NoCache ?? false;

                var identityProfileRoles = await _identityProfileRolesService.GetIdentityProfileRolesAsync();

                AddEthosContextProperties(
                    await _identityProfileRolesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _identityProfileRolesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), identityProfileRoles.InstitutionalRoles.Select(role => role.Key)));

                return Ok(identityProfileRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.InternalServerError);
            }
        }
    }
}
