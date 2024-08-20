// Copyright 2022-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Provides access to Aid Application Results data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Metadata(ApiDescription = "Provides access to aid application results.", ApiDomain = "FA")]
    public class AidApplicationResultsController : BaseCompressedApiController
    {
        private readonly IAidApplicationResultsService _aidApplicationResultsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AidApplicationResultsController class.
        /// </summary>
        /// <param name="aidApplicationResultsService">Aid Application Results service<see cref="IAidApplicationResultsService">IAidApplicationResultsService</see></param>
        /// <param name="logger">Logger<see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings"></param>
        /// <param name="actionContextAccessor"></param>
        public AidApplicationResultsController(IAidApplicationResultsService aidApplicationResultsService, ILogger logger, ApiSettings apiSettings, IActionContextAccessor actionContextAccessor) :
            base(actionContextAccessor, apiSettings)
        {
            _aidApplicationResultsService = aidApplicationResultsService;
            _logger = logger;
        }


        /// <summary>
        /// Read (GET) a AidApplicationResults using a Id
        /// </summary>
        /// <param name="id">Id to desired aid application results</param>
        /// <returns>A aidApplicationResults object <see cref="AidApplicationResults"/> in EEDM format</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPLICATION.RESULTS can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewAidApplicationResults)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get all aid application results records by criteria.",
        HttpMethodDescription = "Get all aid application results records matching the criteria.", HttpMethodPermission = "VIEW.AID.APPLICATION.RESULTS")]
        [HeaderVersionRoute("/aid-application-results/{id}", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAidApplicationResultsByIdV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<AidApplicationResults>> GetAidApplicationResultsByIdAsync([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The ID must be specified in the request URL.")));
            }
            try
            {
                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _aidApplicationResultsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Return all AidApplicationResults
        /// </summary>
        /// <returns>List of AidApplicationResults <see cref="AidApplicationResults"/> objects representing all aid application results</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPLICATION.RESULTS can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewAidApplicationResults)]
        [QueryStringFilterFilter("criteria", typeof(AidApplicationResults))]
        [ValidateQueryStringFilter()]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get all aid application results by criteria",
        HttpMethodDescription = "Get all aid application results record matching the criteria.", HttpMethodPermission = "VIEW.AID.APPLICATION.RESULTS")]
        [HeaderVersionRoute("/aid-application-results", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAidApplicationResultsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<AidApplicationResults>>> GetAidApplicationResultsAsync(Paging page, QueryStringFilter criteria)
        {
            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaFilter = GetFilterObject<AidApplicationResults>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<AidApplicationResults>>(new List<AidApplicationResults>(), page, 0, _apiSettings.IncludeLinkSelfHeaders);


                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());

                var pageOfItems = await _aidApplicationResultsService.GetAidApplicationResultsAsync(page.Offset, page.Limit, criteriaFilter);

                AddEthosContextProperties(await _aidApplicationResultsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                              await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<AidApplicationResults>>(pageOfItems.Item1, page, pageOfItems.Item2, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) aid application results
        /// </summary>
        /// <param name="aidApplicationResults">DTO of the new aidApplicationResults</param>
        /// <returns>An aidApplicationResults object <see cref="AidApplicationResults"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)),
            PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create (POST) aid application results record.",
            HttpMethodDescription = "Create (POST) a new aid application results record.", HttpMethodPermission = "UPDATE.AID.APPLICATION.RESULTS")]
        [HeaderVersionRoute("/aid-application-results", "1.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAidApplicationResultsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<AidApplicationResults>> PostAidApplicationResultsAsync([FromBody] AidApplicationResults aidApplicationResults)
        {
            if (aidApplicationResults == null)
            {
                throw new IntegrationApiException("Null aidApplicationResults argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required."));
            }
            try
            {
                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());

                //call import extend method that needs the extracted extension data and the config
                await _aidApplicationResultsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationResultsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var aidApplicationResultsDto = await _aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResults);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationResultsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { aidApplicationResultsDto.Id }));

                return aidApplicationResultsDto;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing aid application results
        /// </summary>
        /// <param name="id">Id of the aid application results to update</param>
        /// <param name="aidApplicationResults">DTO of the updated aid application results</param>
        /// <returns>A aidApplicationResults object <see cref="AidApplicationResults"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update (PUT) aid application results record.",
            HttpMethodDescription = "Update (PUT) an existing aid application results record.", HttpMethodPermission = "UPDATE.AID.APPLICATION.RESULTS")]
        [HeaderVersionRoute("/aid-application-results/{id}", "1.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAidApplicationResultsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<AidApplicationResults>> PutAidApplicationResultsAsync([FromRoute] string id, [ModelBinder(typeof(EthosEnabledBinder))] AidApplicationResults aidApplicationResults)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The Id must be specified in the request URL.")));
            }
            if (aidApplicationResults == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));
            }

            if (string.IsNullOrEmpty(aidApplicationResults.Id))
            {
                aidApplicationResults.Id = id;
            }
            if (string.IsNullOrEmpty(aidApplicationResults.AppDemoId))
            {
                aidApplicationResults.AppDemoId = id;
            }

            if (id != aidApplicationResults.Id)
            {
                return CreateHttpResponseException(new IntegrationApiException("Id mismatch error",
                    IntegrationApiUtility.GetDefaultApiError("The Id sent in request URL is not the same Id passed in request body.")));
            }
            if (!string.IsNullOrEmpty(aidApplicationResults.AppDemoId) && (aidApplicationResults.AppDemoId != aidApplicationResults.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid Appdemo Id",
                IntegrationApiUtility.GetDefaultApiError("The AppDemoId needs to match with Id.")));
            }
            try
            {
                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _aidApplicationResultsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _aidApplicationResultsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationResultsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                // get original DTO
                AidApplicationResults origAidApplicationResults = null;
                try
                {
                    origAidApplicationResults = await _aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //get the merged DTO. 

                var mergedResults =
                    await PerformPartialPayloadMerge(aidApplicationResults, origAidApplicationResults, dpList, _logger);

                if (origAidApplicationResults != null && mergedResults != null && mergedResults.PersonId != origAidApplicationResults.PersonId)
                {
                    throw new IntegrationApiException("Person ID cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to personId is not allowed."));
                }

                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.AidYear != mergedResults.AidYear)
                {
                    throw new IntegrationApiException("aidYear cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to aidYear is not allowed."));
                }
                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.ApplicationType != mergedResults.ApplicationType)
                {
                    throw new IntegrationApiException("applicationType cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to applicationType is not allowed."));
                }
                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.ApplicantAssignedId != mergedResults.ApplicantAssignedId)
                {
                    throw new IntegrationApiException("applicantAssignedId cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to applicantAssignedId is not allowed."));
                }
                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.AppDemoId != mergedResults.AppDemoId)
                {
                    throw new IntegrationApiException("AppDemoId cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to appDemoId is not allowed."));
                }
                var resultsReturn = await _aidApplicationResultsService.PutAidApplicationResultsAsync(id, mergedResults);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationResultsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { resultsReturn.Id }));

                return resultsReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Delete (DELETE) an existing aid application results
        /// </summary>
        /// <param name="id">Id of the aid application results to update</param>
        [HttpDelete]
        [HeaderVersionRoute("/aid-application-results/{id}", Name = "DefaultDeleteAidApplicationResults")]
        public async Task<ActionResult> DeleteAidApplicationResultsAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
