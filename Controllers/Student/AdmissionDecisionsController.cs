// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Web.Http.ModelBinding;

using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionDecisions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionDecisionsController : BaseCompressedApiController
    {
        private readonly IAdmissionDecisionsService _admissionDecisionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionDecisionsController class.
        /// </summary>
        /// <param name="admissionDecisionsService">Service of type <see cref="IAdmissionDecisionsService">IAdmissionDecisionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionDecisionsController(IAdmissionDecisionsService admissionDecisionsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionDecisionsService = admissionDecisionsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionDecisions
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewAdmissionDecisions, StudentPermissionCodes.UpdateAdmissionDecisions})]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AdmissionDecisions)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/admission-decisions", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionDecisions", IsEedmSupported = true)]
        public async Task<IActionResult> GetAdmissionDecisionsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _admissionDecisionsService.ValidatePermissions(GetPermissionsMetaData());
                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var admissionDecision = GetFilterObject<Dtos.AdmissionDecisions>(_logger, "criteria");
                var filterQualifiers = GetFilterQualifiers(_logger);

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.AdmissionDecisions>>(new List<Dtos.AdmissionDecisions>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                
                string applicationId = string.Empty; DateTimeOffset decidedOn = DateTime.MinValue;
                if (admissionDecision != null)
                {
                    applicationId = admissionDecision.Application != null ? admissionDecision.Application.Id : string.Empty;
                    if (admissionDecision.DecidedOn != DateTime.MinValue)
                    {
                        decidedOn = admissionDecision.DecidedOn;
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _admissionDecisionsService.GetAdmissionDecisionsAsync(page.Offset, page.Limit, 
                    applicationId, decidedOn, filterQualifiers, personFilterValue, bypassCache);

                AddEthosContextProperties(await _admissionDecisionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionDecisionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AdmissionDecisions>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a admissionDecisions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionDecisions</param>
        /// <returns>A admissionDecisions object <see cref="Dtos.AdmissionDecisions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewAdmissionDecisions, StudentPermissionCodes.UpdateAdmissionDecisions})]

        [HeaderVersionRoute("/admission-decisions/{guid}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionDecisionsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionDecisions>> GetAdmissionDecisionsByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _admissionDecisionsService.ValidatePermissions(GetPermissionsMetaData());
                var admissionDecision = await _admissionDecisionsService.GetAdmissionDecisionsByGuidAsync(guid);

                if (admissionDecision != null)
                {

                    AddEthosContextProperties(await _admissionDecisionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionDecisionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { admissionDecision.Id }));
                }


                return admissionDecision;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
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
        /// Create (POST) a new admissionDecisions
        /// </summary>
        /// <param name="admissionDecisions">DTO of the new admissionDecisions</param>
        /// <returns>A admissionDecisions object <see cref="Dtos.AdmissionDecisions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateAdmissionDecisions)]
        [HeaderVersionRoute("/admission-decisions", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionDecisionsV11_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionDecisions>> PostAdmissionDecisionsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionDecisions admissionDecisions)
        {
            if (admissionDecisions == null)
            {
                return CreateHttpResponseException("Request body must contain a valid admission decision.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(admissionDecisions.Id))
            {
                return CreateHttpResponseException("Id is required.");
            }

            if (admissionDecisions.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentNullException("admissionDecisionsDto", "On a post you can not define a GUID.")));
            }

            try
            {
                _admissionDecisionsService.ValidatePermissions(GetPermissionsMetaData());
                ValidateAdmissionDecisions(admissionDecisions);
                //call import extend method that needs the extracted extension data and the config
                await _admissionDecisionsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionDecisionsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the admission decision
                var admissionDecisionReturn = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisions);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _admissionDecisionsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _admissionDecisionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { admissionDecisionReturn.Id }));

                return admissionDecisionReturn;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
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

        private void ValidateAdmissionDecisions(AdmissionDecisions admissionDecisions)
        {

            if (admissionDecisions.Application == null)
            {
                throw new ArgumentNullException("application", "Application is required.");
            }

            if (admissionDecisions.Application != null && string.IsNullOrEmpty(admissionDecisions.Application.Id))
            {
                throw new ArgumentNullException("application.id", "Application id is required.");
            }

            if (admissionDecisions.DecisionType == null)
            {
                throw new ArgumentNullException("decisionType", "Decision type is required.");
            }

            if (admissionDecisions.DecisionType != null && string.IsNullOrEmpty(admissionDecisions.DecisionType.Id))
            {
                throw new ArgumentNullException("decisionType.id", "Decision type id is required.");
            }

            if (admissionDecisions.DecidedOn == null || admissionDecisions.DecidedOn.Equals(default(DateTime)))
            {
                throw new ArgumentNullException("decidedOn", "Decided on is required.");
            }
        }

        /// <summary>
        /// Update (PUT) an existing admissionDecisions
        /// </summary>
        /// <param name="guid">GUID of the admissionDecisions to update</param>
        /// <param name="admissionDecisions">DTO of the updated admissionDecisions</param>
        /// <returns>A admissionDecisions object <see cref="Dtos.AdmissionDecisions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/admission-decisions/{guid}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionDecisionsV11_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionDecisions>> PutAdmissionDecisionsAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionDecisions admissionDecisions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException("PUT",
                IntegrationApiUtility.GetDefaultApiError("Admission decision cannot be updated. Use POST to submit a new admission decision.")));

        }

        /// <summary>
        /// Delete (DELETE) a admissionDecisions
        /// </summary>
        /// <param name="guid">GUID to desired admissionDecisions</param>
        [HttpDelete]
        [Route("/admission-decisions/{guid}", Name = "DefaultDeleteAdmissionDecisions", Order = -10)]
        public async Task<IActionResult> DeleteAdmissionDecisionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
