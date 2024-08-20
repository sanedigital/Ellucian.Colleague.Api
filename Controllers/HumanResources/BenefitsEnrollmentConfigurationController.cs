// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to benefits enrollment configuration items
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class BenefitsEnrollmentConfigurationController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IBenefitsEnrollmentConfigurationService benefitsEnrollmentConfigurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="benefitsEnrollmentConfigurationService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BenefitsEnrollmentConfigurationController(IBenefitsEnrollmentConfigurationService benefitsEnrollmentConfigurationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.benefitsEnrollmentConfigurationService = benefitsEnrollmentConfigurationService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the configurations for benefits enrollment
        /// </summary>
        /// <returns>BenefitsEnrollmentConfiguration</returns>
        /// <accessComments>Any authenticated user can get this resource</accessComments>
        /// <note>BenefitsEnrollmentConfiguration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/benefits-enrollment-configuration", 1, true, Name = "GetBenefitsEnrollmentConfigurationAsync")]
        public async Task<ActionResult<BenefitsEnrollmentConfiguration>> GetBenefitsEnrollmentConfigurationAsync()
        {
            try
            {
                return await benefitsEnrollmentConfigurationService.GetBenefitsEnrollmentConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.InternalServerError);
            }
        }
    }
}
