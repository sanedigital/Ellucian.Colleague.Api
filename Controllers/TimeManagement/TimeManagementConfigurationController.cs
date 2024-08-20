// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// Time Management Configuration Controller routes requests for configurations for time management
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class TimeManagementConfigurationController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly ITimeManagementConfigurationService timeManagementConfigurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Time Management Configuration Controller constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="timeManagementConfigurationService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TimeManagementConfigurationController(IAdapterRegistry adapterRegistry, ILogger logger, ITimeManagementConfigurationService timeManagementConfigurationService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.timeManagementConfigurationService = timeManagementConfigurationService;
        }

        /// <summary>
        /// Get Time Management Configuration for environment.
        /// 
        /// A succesful request will return a status code of 200 and a TimeManagementConfiguration object.
        /// An unsuccesful request will return a status code of 400
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A TimeManagementConfiguration</returns>
        /// <note>TimeManagementConfiguration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/time-management-configuration", 1, true, Name = "GetTimeManagementConfigurationAsync")]
        public async Task<ActionResult<TimeManagementConfiguration>> GetTimeManagementConfigurationAsync()
        {
            try
            {
                logger.LogDebug("************Start - Process to get time management configuration - Start************");
                var config = await timeManagementConfigurationService.GetTimeManagementConfigurationAsync();
                logger.LogDebug("************End - Process to get time management configuration is successful - End************");
                return config;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (Exception e)
            {
                logger.LogError(e, "Unknown error");
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}
