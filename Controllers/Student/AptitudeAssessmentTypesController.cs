// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using System.Net.Http;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AptitudeAssessmentTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AptitudeAssessmentTypesController : BaseCompressedApiController
    {
        private readonly IAptitudeAssessmentTypesService _aptitudeAssessmentTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AptitudeAssessmentTypesController class.
        /// </summary>
        /// <param name="aptitudeAssessmentTypesService">Service of type <see cref="IAptitudeAssessmentTypesService">IAptitudeAssessmentTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AptitudeAssessmentTypesController(IAptitudeAssessmentTypesService aptitudeAssessmentTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _aptitudeAssessmentTypesService = aptitudeAssessmentTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all aptitudeAssessmentTypes
        /// </summary>
        /// <returns>List of AptitudeAssessmentTypes <see cref="Dtos.AptitudeAssessmentTypes"/> objects representing matching aptitudeAssessmentTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/aptitude-assessment-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAptitudeAssessmentTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessmentTypes>>> GetAptitudeAssessmentTypesAsync()
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var aptitudeAssessmentTypes = await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesAsync(bypassCache);

                if (aptitudeAssessmentTypes != null && aptitudeAssessmentTypes.Any())
                {
                    AddEthosContextProperties(await _aptitudeAssessmentTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _aptitudeAssessmentTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              aptitudeAssessmentTypes.Select(a => a.Id).ToList()));
                }
                return Ok(aptitudeAssessmentTypes);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a aptitudeAssessmentTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired aptitudeAssessmentTypes</param>
        /// <returns>A aptitudeAssessmentTypes object <see cref="Dtos.AptitudeAssessmentTypes"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/aptitude-assessment-types/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAptitudeAssessmentTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AptitudeAssessmentTypes>> GetAptitudeAssessmentTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var aptitudeAssessemetTypeDto = await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesByGuidAsync(guid);
                if (aptitudeAssessemetTypeDto == null)
                {
                    return NoContent();
                }
                AddEthosContextProperties(
                   await _aptitudeAssessmentTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _aptitudeAssessmentTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return Ok(aptitudeAssessemetTypeDto);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new aptitudeAssessmentTypes
        /// </summary>
        /// <param name="aptitudeAssessmentTypes">DTO of the new aptitudeAssessmentTypes</param>
        /// <returns>A aptitudeAssessmentTypes object <see cref="Dtos.AptitudeAssessmentTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/aptitude-assessment-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAptitudeAssessmentTypesV6")]
        public async Task<ActionResult<Dtos.AptitudeAssessmentTypes>> PostAptitudeAssessmentTypesAsync([FromBody] Dtos.AptitudeAssessmentTypes aptitudeAssessmentTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing aptitudeAssessmentTypes
        /// </summary>
        /// <param name="guid">GUID of the aptitudeAssessmentTypes to update</param>
        /// <param name="aptitudeAssessmentTypes">DTO of the updated aptitudeAssessmentTypes</param>
        /// <returns>A aptitudeAssessmentTypes object <see cref="Dtos.AptitudeAssessmentTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/aptitude-assessment-types/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAptitudeAssessmentTypesV6")]
        public async Task<ActionResult<Dtos.AptitudeAssessmentTypes>> PutAptitudeAssessmentTypesAsync([FromRoute] string guid, [FromBody] Dtos.AptitudeAssessmentTypes aptitudeAssessmentTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a aptitudeAssessmentTypes
        /// </summary>
        /// <param name="guid">GUID to desired aptitudeAssessmentTypes</param>
        [HttpDelete]
        [Route("/aptitude-assessment-types/{guid}", Name = "DefaultDeleteAptitudeAssessmentTypes")]
        public async Task<IActionResult> DeleteAptitudeAssessmentTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
