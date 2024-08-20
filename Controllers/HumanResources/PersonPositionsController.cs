// Copyright 2016-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes PersonPosition data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    [Metadata(ApiDescription = "Provides positions assigned to a person", ApiDomain = "Human Resources")]
    public class PersonPositionsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPersonPositionService personPositionService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="personPositionService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonPositionsController(ILogger logger, IPersonPositionService personPositionService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.personPositionService = personPositionService;
        }

        /// <summary>
        /// Get PersonPosition objects. This endpoint returns objects based on the current
        /// user's permissions.
        /// Example: If the current user/user who has proxy is an employee, this endpoint returns that employee's/proxy's PersonPositions
        /// Example: If the current user/user who has proxy is a manager, this endpoint returns all the PersonPositions of the employees reporting to the manager
        /// Example: If the current user is an admin, this endpoint returns the PersonPositions for the effectivePersonId
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns>A list of PersonPosition objects</returns>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService), primaryGuidParameters: new[] { "effectivePersonId" })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/person-positions", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonPositionsAsyncV1.0.0",IsEthosEnabled =true)]
        [HeaderVersionRoute("/person-positions", 1, false, Name = "GetPersonPositions")]        
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a list of positions assigned to a user.",
            HttpMethodDescription = "Gets a list of PersonPostion objects.")]
        public async Task<ActionResult<IEnumerable<PersonPosition>>> GetPersonPositionsAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                var result = await personPositionService.GetPersonPositionsAsync(effectivePersonId, lookupStartDate);
                return Ok(result);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                var genericErrorMessage = "Unknown error occurred while getting person positions";
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(genericErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}
