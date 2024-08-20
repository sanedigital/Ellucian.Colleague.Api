// Copyright 2022-2023 Ellucian Company L.P. and its affiliates.Funds Roster Data
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Provides access to Funds Roster Data data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Metadata(ApiDescription = "Provides access to Funds Roster Data.", ApiDomain = "FA")]
    public class FundsRosterController : BaseCompressedApiController
    {
        private readonly IFundsRosterService _fundsRosterService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FundsRosterController class.
        /// </summary>
        /// <param name="FundsRosterService">Funds Roster Data service<see cref="IFundsRosterService">IFundsRosterService</see></param>
        /// <param name="logger">Logger<see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings"></param>
        /// <param name="actionContextAccessor"></param>_fundsRostersService
        public FundsRosterController(IFundsRosterService FundsRosterService, ILogger logger, ApiSettings apiSettings, IActionContextAccessor actionContextAccessor) :
            base(actionContextAccessor, apiSettings)
        {
            _fundsRosterService = FundsRosterService;
            _logger = logger;
        }

        #region GetFundsRosterById
        /// <summary>
        /// Read (GET) a FundsRoster using a Id
        /// </summary>
        /// <param name="id">Id to desired Funds Roster Data</param>
        /// <returns>A fundsRoster object <see cref="FundsRoster"/> in EEDM format</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.FUNDS.ROSTER can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewFundsRoster)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get the Funds Roster Data record by ID.",
            HttpMethodDescription = "Get the Funds Roster Data record by ID.", HttpMethodPermission = "VIEW.FUNDS.ROSTER")]
        [HeaderVersionRoute("/funds-roster/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFundsRosterByIdV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<FundsRoster>> GetFundsRosterByIdAsync([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _fundsRosterService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _fundsRosterService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _fundsRosterService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _fundsRosterService.GetFundsRosterByIdAsync(id);
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


        #region GetFundsRoster
        /// <summary>
        /// Return all FundsRoster
        /// </summary>
        /// <returns>List of FundsRoster <see cref="FundsRoster"/> objects representing all Funds Roster Data</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.FUNDS.ROSTER can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewFundsRoster)]
        [QueryStringFilterFilter("criteria", typeof(FundsRoster))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter()]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get all Funds Roster Data.",
            HttpMethodDescription = "Get all Funds Roster Data records.", HttpMethodPermission = "VIEW.FUNDS.ROSTER")]
        [HeaderVersionRoute("/funds-roster", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFundsRosterV1.0.0", IsEedmSupported = true)]
        public async Task<IActionResult> GetFundsRosterAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }
                var criteriaFilter = GetFilterObject<FundsRoster>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<FundsRoster>>(new List<FundsRoster>(), page, 0, _apiSettings.IncludeLinkSelfHeaders);


                _fundsRosterService.ValidatePermissions(GetPermissionsMetaData());

                var pageOfItems = await _fundsRosterService.GetFundsRosterAsync(page.Offset, page.Limit, criteriaFilter, personFilterValue);

                AddEthosContextProperties(await _fundsRosterService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                              await _fundsRosterService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.FundsId).ToList()));

                return new PagedActionResult<IEnumerable<FundsRoster>>(pageOfItems.Item1, page, pageOfItems.Item2, _apiSettings.IncludeLinkSelfHeaders);

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


        #region PostFundsRosterAsync
        /// <summary>
        /// Create (POST) a new fund roster record
        /// </summary>
        /// <param name="fundsRoster">DTO of the new fund roster record</param>
        /// <returns>FundsRoster object <see cref="Dtos.FinancialAid.FundsRoster"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateFundsRoster)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create (POST) a new fund roster record",
            HttpMethodDescription = "Create (POST) a new fund roster record.", HttpMethodPermission = "UPDATE.FUNDS.ROSTER")]
        [HeaderVersionRoute("/funds-roster", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFundsRosterV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAid.FundsRoster>> PostFundsRosterAsync([FromBody] Dtos.FinancialAid.FundsRoster fundsRoster)
        {
            if (fundsRoster == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null fundsRoster argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(fundsRoster.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null fundsRoster id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (fundsRoster.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(new IntegrationApiException("person",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID must be used in POST operation.")));
            }
            try
            {
                _fundsRosterService.ValidatePermissions(GetPermissionsMetaData());

                //call import extend method that needs the extracted extension data and the config
                await _fundsRosterService.ImportExtendedEthosData(await ExtractExtendedData(await _fundsRosterService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var fundsRosterDto = await _fundsRosterService.PostFundsRosterAsync(fundsRoster);

                //store dataprivacy list and get the extended data to store

                AddEthosContextProperties(await _fundsRosterService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _fundsRosterService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { fundsRosterDto.Id }));

                return fundsRosterDto;
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


        #region PutFundsRosterAsync
        /// <summary>
        /// Update (PUT) an existing fund roster record
        /// </summary>
        /// <param name="id">Id of the fund roster to update</param>
        /// <param name="fundsRoster">DTO of the updated funds roster</param>
        /// <returns>A FundsRoster object <see cref="Dtos.FinancialAid.FundsRoster"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateFundsRoster)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update (PUT) an existing funds roster",
            HttpMethodDescription = "Update (PUT) an existing funds roster.", HttpMethodPermission = "UPDATE.FUNDS.ROSTER")]
        [HeaderVersionRoute("/funds-roster/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFundsRosterV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAid.FundsRoster>> PutFundsRosterAsync([FromRoute] string id, [ModelBinder(typeof(EthosEnabledBinder))] Dtos.FinancialAid.FundsRoster fundsRoster)
        {

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (fundsRoster == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));

            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(fundsRoster.Id))
            {
                fundsRoster.Id = id;
            }

            if (id != fundsRoster.Id)
            {
                return CreateHttpResponseException(new IntegrationApiException("Id mismatch error",
                    IntegrationApiUtility.GetDefaultApiError("The GUID sent in request URL is not the same Id passed in request body.")));
            }

            try
            {
                _fundsRosterService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _fundsRosterService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _fundsRosterService.ImportExtendedEthosData(await ExtractExtendedData(await _fundsRosterService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                // get original DTO
                Dtos.FinancialAid.FundsRoster origFundsRoster = null;
                try
                {
                    origFundsRoster = await _fundsRosterService.GetFundsRosterByIdAsync(id);
                }
                catch (KeyNotFoundException ex)
                {
                    origFundsRoster = null;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //get the merged DTO.
                var mergedFundsRoster =
                    await PerformPartialPayloadMerge(fundsRoster, origFundsRoster, dpList, _logger);

                var integrationApiException = new IntegrationApiException();
                
                if (origFundsRoster != null && origFundsRoster.Status == FundsRosterStatus.complete) 
                {
                     integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Record cannot be updated", message: "Update to record with status complete is not allowed."));
                }

                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.PersonId != mergedFundsRoster.PersonId)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Person ID cannot be updated", message: "Update to personId is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.AcademicYear != mergedFundsRoster.AcademicYear)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Academic Year cannot be updated", message: "Update to AcademicYear is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.FundType != mergedFundsRoster.FundType)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Fund Type cannot be updated", message: "Update to Fund Type is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.UsdeOpeId != mergedFundsRoster.UsdeOpeId)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "USDE/OPE Id cannot be updated", message: "Update to UsdeOpeId is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Fall != null && mergedFundsRoster.Fall != null && origFundsRoster.Fall.AwardFund != mergedFundsRoster.Fall.AwardFund)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Fall awardFund cannot be updated", message: "Update to awardFund is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Fall != null && mergedFundsRoster.Fall != null && origFundsRoster.Fall.AwardPeriod != mergedFundsRoster.Fall.AwardPeriod)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Fall award period cannot be updated", message: "Update to awardPeriod is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Spring != null && mergedFundsRoster.Spring != null && origFundsRoster.Spring.AwardFund != mergedFundsRoster.Spring.AwardFund)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Spring awardFund cannot be updated", message: "Update to awardFund is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Spring != null && mergedFundsRoster.Spring != null && origFundsRoster.Spring.AwardPeriod != mergedFundsRoster.Spring.AwardPeriod)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Spring award period cannot be updated", message: "Update to awardPeriod is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Summer != null && mergedFundsRoster.Summer != null && origFundsRoster.Summer.AwardFund != mergedFundsRoster.Summer.AwardFund)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Summer awardFund cannot be updated", message: "Update to awardFund is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Summer != null && mergedFundsRoster.Summer != null && origFundsRoster.Summer.AwardPeriod != mergedFundsRoster.Summer.AwardPeriod)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Summer award period cannot be updated", message: "Update to awardPeriod is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Winter != null && mergedFundsRoster.Winter != null && origFundsRoster.Winter.AwardFund != mergedFundsRoster.Winter.AwardFund)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Winter awardFund cannot be updated", message: "Update to awardFund is not allowed."));
                }
                if (origFundsRoster != null && mergedFundsRoster != null && origFundsRoster.Winter != null && mergedFundsRoster.Winter != null && origFundsRoster.Winter.AwardPeriod != mergedFundsRoster.Winter.AwardPeriod)
                {
                    integrationApiException.AddError(new IntegrationApiError("Global.Internal.Error", description: "Winter award period cannot be updated", message: "Update to awardPeriod is not allowed."));
                }

                if (integrationApiException.Errors != null && integrationApiException.Errors.Any())
                {
                    throw integrationApiException;
                }

                // put service
                var fundsRosterReturn = await _fundsRosterService.PutFundsRosterAsync(id, mergedFundsRoster);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _fundsRosterService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _fundsRosterService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { fundsRosterReturn.Id }));

                return fundsRosterReturn;
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


        #region DeleteFundsRosterAsync
        /// <summary>
        /// Delete (DELETE) an existing funds roster
        /// </summary>
        /// <param name="id">Id of the fund roster to update</param>
        [HttpDelete]
        [HeaderVersionRoute("/funds-roster/{id}", Name = "DefaultDeleteFundsRoster")]
        public async Task<ActionResult> DeleteFundsRosterAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion


    }

}
