// Copyright 2022-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
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
    /// Provides access to Aid Applications data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Metadata(ApiDescription = "Provides access to aid applications.", ApiDomain = "FA")]
    public class AidApplicationsController : BaseCompressedApiController
    {
        private readonly IAidApplicationsService _aidApplicationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AidApplicationsController class.
        /// </summary>
        /// <param name="aidApplicationsService">Aid Applications service<see cref="IAidApplicationsService">IAidApplicationsService</see></param>
        /// <param name="logger">Logger<see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings"></param>
        /// <param name="actionContextAccessor"></param>
        public AidApplicationsController(IAidApplicationsService aidApplicationsService, ILogger logger, ApiSettings apiSettings, IActionContextAccessor actionContextAccessor) :
            base(actionContextAccessor, apiSettings)
        {
            _aidApplicationsService = aidApplicationsService;
            _logger = logger;
        }

        #region GetAidApplicationsByIdAsync
        /// <summary>
        /// Read (GET) a AidApplications using a Id
        /// </summary>
        /// <param name="id">Id to desired aid applications</param>
        /// <returns>A aidApplications object <see cref="AidApplications"/> in EEDM format</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPLICATIONS can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewAidApplications)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get the aid applications by ID",
        HttpMethodDescription = "Get the aid applications by ID.", HttpMethodPermission = "VIEW.AID.APPLICATIONS")]
        [HeaderVersionRoute("/aid-applications/{id}", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAidApplicationsByIdV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAid.AidApplications>> GetAidApplicationsByIdAsync([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The ID must be specified in the request URL.")));
            }
            try
            {
                _aidApplicationsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _aidApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _aidApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _aidApplicationsService.GetAidApplicationsByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion

        #region GetAidApplicationsAsync
        /// <summary>
        /// Return all AidApplications
        /// </summary>
        /// <returns>List of AidApplications <see cref="AidApplications"/> objects representing all aid applications</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPLICATIONS can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewAidApplications)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.FinancialAid.AidApplications))]
        [ValidateQueryStringFilter()]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get all aid applications by criteria",
        HttpMethodDescription = "Get all aid applications record matching the criteria.", HttpMethodPermission = "VIEW.AID.APPLICATIONS")]
        [HeaderVersionRoute("/aid-applications", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAidApplicationsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.FinancialAid.AidApplications>>> GetAidApplicationsAsync(Paging page, QueryStringFilter criteria)
        {
            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaFilter = GetFilterObject<Dtos.FinancialAid.AidApplications>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.FinancialAid.AidApplications>>(new List<Dtos.FinancialAid.AidApplications>(), page, 0, _apiSettings.IncludeLinkSelfHeaders);

                _aidApplicationsService.ValidatePermissions(GetPermissionsMetaData());

                var pageOfItems = await _aidApplicationsService.GetAidApplicationsAsync(page.Offset, page.Limit, criteriaFilter);

                AddEthosContextProperties(await _aidApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                              await _aidApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.FinancialAid.AidApplications>>(pageOfItems.Item1, page, pageOfItems.Item2, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion

        #region PostAidApplicationsAsync
        /// <summary>
        /// Create (POST) a new aid applications record
        /// </summary>
        /// <param name="aidApplications">DTO of the new aid applications</param>
        /// <returns>AidApplications object <see cref="Dtos.FinancialAid.AidApplications"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateAidApplications)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create (POST) a new aid applications",
        HttpMethodDescription = "Create (POST) a new aid applications.", HttpMethodPermission = "UPDATE.AID.APPLICATIONS")]
        [HeaderVersionRoute("/aid-applications", "1.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAidApplicationsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAid.AidApplications>> PostAidApplicationsAsync([FromBody] Dtos.FinancialAid.AidApplications aidApplications)
        {
            if (aidApplications == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null aidApplications argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _aidApplicationsService.ValidatePermissions(GetPermissionsMetaData());

                //call import extend method that needs the extracted extension data and the config
                await _aidApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var aidApplicationsDto = await _aidApplicationsService.PostAidApplicationsAsync(aidApplications);

                //store dataprivacy list and get the extended data to store

                AddEthosContextProperties(await _aidApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { aidApplicationsDto.Id }));

                return aidApplicationsDto;
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
        #endregion

        #region PutAidApplicationsAsync
        /// <summary>
        /// Update (PUT) an existing aid applications record
        /// </summary>
        /// <param name="id">Id of the aid applications to update</param>
        /// <param name="aidApplications">DTO of the updated aid applications</param>
        /// <returns>A AidApplications object <see cref="Dtos.FinancialAid.AidApplications"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateAidApplications)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update (PUT) an existing aid applications",
        HttpMethodDescription = "Update (PUT) an existing aid applications.", HttpMethodPermission = "UPDATE.AID.APPLICATIONS")]
        [HeaderVersionRoute("/aid-applications/{id}", "1.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAidApplicationsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAid.AidApplications>> PutAidApplicationsAsync([FromRoute] string id, [ModelBinder(typeof(EthosEnabledBinder))] Dtos.FinancialAid.AidApplications aidApplications)
        {

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The Id must be specified in the request URL.")));
            }
            if (aidApplications == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));

            }
            if (string.IsNullOrEmpty(aidApplications.Id))
            {
                aidApplications.Id = id;
            }

            if (id != aidApplications.Id)
            {
                return CreateHttpResponseException(new IntegrationApiException("Id mismatch error",
                    IntegrationApiUtility.GetDefaultApiError("The Id sent in request URL is not the same Id passed in request body.")));
            }
            if (!string.IsNullOrEmpty(aidApplications.AppDemoID) && (aidApplications.AppDemoID != aidApplications.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid Appdemo Id",
                IntegrationApiUtility.GetDefaultApiError("The appDemoId needs to match Id.")));
            }
            try
            {
                _aidApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _aidApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _aidApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                // get original DTO
                Dtos.FinancialAid.AidApplications origAidApplications = null;
                try
                {
                    origAidApplications = await _aidApplicationsService.GetAidApplicationsByIdAsync(id);
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
                var mergedAidApplications =
                    await PerformPartialPayloadMerge(aidApplications, origAidApplications, dpList, _logger);

                var integrationApiException = new IntegrationApiException();

                if (origAidApplications != null && mergedAidApplications != null && origAidApplications.PersonId != mergedAidApplications.PersonId)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Person ID cannot be updated", message: "Update to personId is not allowed."));
                }
                if (mergedAidApplications != null && origAidApplications.AidYear != mergedAidApplications.AidYear)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Aid Year cannot be updated", message: "Update to aidYear is not allowed."));
                }
                if (mergedAidApplications != null && origAidApplications.ApplicationType != mergedAidApplications.ApplicationType)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Application Type cannot be updated", message: "Update to aidApplicationType is not allowed."));
                }
                if (mergedAidApplications != null && origAidApplications.ApplicantAssignedId != mergedAidApplications.ApplicantAssignedId)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Assigned Id cannot be updated", message: "Update to applicantAssignedId is not allowed."));
                }

                if (integrationApiException.Errors != null && integrationApiException.Errors.Any())
                {
                    throw integrationApiException;
                }

                // put service
                var aidApplicationsReturn = await _aidApplicationsService.PutAidApplicationsAsync(id, mergedAidApplications);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { aidApplicationsReturn.Id }));

                return aidApplicationsReturn;
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
        #endregion

        #region DeleteAidApplicationsAsync
        /// <summary>
        /// Delete (DELETE) an existing aid applications
        /// </summary>
        /// <param name="id">Id of the aid applications to update</param>
        [HttpDelete]
        [HeaderVersionRoute("/aid-applications/{id}", Name = "DefaultDeleteAidApplications")]
        public async Task<ActionResult> DeleteAidApplicationsAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
