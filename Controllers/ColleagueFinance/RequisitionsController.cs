// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using System.Linq;
using System.Net.Http;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for requisitions
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class RequisitionsController : BaseCompressedApiController
    {
        private readonly IRequisitionService requisitionService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the RequisitionsController object
        /// </summary>
        /// <param name="requisitionService">Requisition service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RequisitionsController(IRequisitionService requisitionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.requisitionService = requisitionService;
            this.logger = logger;
        }

        #region Requisition(SS) DTO entity methods
        /// <summary>
        /// Retrieves a specified requisition
        /// </summary>
        /// <param name="requisitionId">ID of the requested requisition</param>
        /// <returns>Requisition DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.REQUISITION, and requires access to at least one of the
        /// general ledger numbers on the requisition line items.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/requisitions/{requisitionId}", 1, false, Name = "GetRequisition")]
        public async Task<ActionResult<Requisition>> GetRequisitionAsync(string requisitionId)
        {
            if (string.IsNullOrEmpty(requisitionId))
            {
                string message = "A Requisition ID must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var requisition = await requisitionService.GetRequisitionAsync(requisitionId);
                return requisition;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the requisition.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to get the requisition.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the requisition.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves list of requistion summary
        /// </summary>
        /// <param name="personId">ID logged in user</param>
        /// <returns>list of Requisition Summary DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission VIEW.REQUISITION.
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague Web API 1.30. Use QueryRequisitionSummariesAsync.")]
        [HttpGet]
        [HeaderVersionRoute("/requisitions-summary/{personId}", 1, false, Name = "GetRequisitionsSummaryByPersonId")]
        public async Task<ActionResult<IEnumerable<RequisitionSummary>>> GetRequisitionsSummaryByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                string message = "person Id must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var requisition = await requisitionService.GetRequisitionsSummaryByPersonIdAsync(personId);
                return Ok(requisition);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the requisition summary.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the requisition summary.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create / Update a requisition.
        /// </summary>
        /// <param name="requisitionCreateUpdateRequest">The requisition create update request DTO.</param>        
        /// <returns>The requisition create response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission CREATE.UPDATE.REQUISITION.
        /// </accessComments>        
        [HttpPost]
        [HeaderVersionRoute("/requisitions", 1, false, Name = "PostRequisition")]
        public async Task<ActionResult<Dtos.ColleagueFinance.RequisitionCreateUpdateResponse>> PostRequisitionAsync([FromBody] Dtos.ColleagueFinance.RequisitionCreateUpdateRequest requisitionCreateUpdateRequest)
        {
            if (requisitionCreateUpdateRequest == null)
            {
                return CreateHttpResponseException("Request body must contain a valid requisition.", HttpStatusCode.BadRequest);
            }

            try
            {
                return await requisitionService.CreateUpdateRequisitionAsync(requisitionCreateUpdateRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to create/update the requisition.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to create/update the requisition.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found to create/update the requisition.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to create/update the requisition.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to create/update the requisition.", HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves a specified requisition for modify with line item defaults
        /// </summary>
        /// <param name="requisitionId">ID of the requested requisition</param>
        /// <returns>Modify Requisition DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.REQUISITION, and requires access to at least one of the
        /// general ledger numbers on the requisition line items.
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague Web API 1.28. Use GetRequisitionAsync instead.")]
        [HttpGet]
        [HeaderVersionRoute("/requisitions-modify/{requisitionId}", 1, false, Name = "GetRequisitionForModifyWithLineItemDefaults")]
        public async Task<ActionResult<ModifyRequisition>> GetRequisitionForModifyWithLineItemDefaultsAsync(string requisitionId)
        {
            if (string.IsNullOrEmpty(requisitionId))
            {
                string message = "A Requisition ID must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var modifyRequisitionDto = await requisitionService.GetRequisitionForModifyWithLineItemDefaultsAsync(requisitionId);
                return modifyRequisitionDto;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the requisition.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the requisition.", HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Delete a requisition.
        /// </summary>
        /// <param name="requisitionDeleteRequest">The requisition delete request DTO.</param>        
        /// <returns>The requisition delete response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission DELETE.REQUISITION.
        /// </accessComments>        
        [HttpPost]
        [HeaderVersionRoute("/requisitions-delete", 1, false, Name = "DeleteRequisition")]
        public async Task<ActionResult<Dtos.ColleagueFinance.RequisitionDeleteResponse>> DeleteRequisitionAsync([FromBody] Dtos.ColleagueFinance.RequisitionDeleteRequest requisitionDeleteRequest)
        {
            if (requisitionDeleteRequest == null)
            {
                return CreateHttpResponseException("Request body must contain a valid delete requisition request.", HttpStatusCode.BadRequest);
            }

            try
            {
                return await requisitionService.DeleteRequisitionsAsync(requisitionDeleteRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to delete the requisition.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to delete the requisition.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found to delete the requisition.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to delete the requisition.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to delete the requisition.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves list of requistion summary
        /// </summary>
        /// <param name="filterCriteria">procurement filter criteria</param>
        /// <returns>list of Requisition Summary DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission VIEW.REQUISITION or CREATE.UPDATE.REQUISITION.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/requisition-summaries", 1, true, Name = "QueryRequisitionSummariesAsync")]
        public async Task<ActionResult<IEnumerable<RequisitionSummary>>> QueryRequisitionSummariesAsync([FromBody] Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria filterCriteria)
        {
            if (filterCriteria == null)
            {
                return CreateHttpResponseException("Request body must contain a valid search criteria.", HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await requisitionService.QueryRequisitionSummariesAsync(filterCriteria));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to search requisitions.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to search requisitions.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found to search requisitions.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to search the requisition.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to search the requisition.", HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Requisitions(EEDM) DTO entity methods

        /// <summary>
        /// Return all requisitions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria"></param>
        /// <returns>List of Requisitions <see cref="Dtos.Requisitions"/> objects representing matching requisitions</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewRequisitions,
           ColleagueFinancePermissionCodes.UpdateRequisitions })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Requisitions))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/requisitions", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRequisitions", IsEedmSupported = true)]
        public async Task<IActionResult> GetRequisitionsAsync(Paging page, QueryStringFilter criteria)
        {
            try
            {
                requisitionService.ValidatePermissions(GetPermissionsMetaData());

                var bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObject = GetFilterObject<Dtos.Requisitions>(logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Requisitions>>(new List<Dtos.Requisitions>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await requisitionService.GetRequisitionsAsync(page.Offset, page.Limit, criteriaObject, bypassCache);

                AddEthosContextProperties(
                    await requisitionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Requisitions>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a requisitions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired requisitions</param>
        /// <returns>A requisitions object <see cref="Dtos.Requisitions"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewRequisitions,
           ColleagueFinancePermissionCodes.UpdateRequisitions })]
        [HeaderVersionRoute("/requisitions/{guid}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRequisitionsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Requisitions>> GetRequisitionsByGuidAsync(string guid)
        {
            try
            {
                requisitionService.ValidatePermissions(GetPermissionsMetaData());

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

                AddEthosContextProperties(
                    await requisitionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await requisitionService.GetRequisitionsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing Requisitions
        /// </summary>
        /// <param name="guid">GUID of the requisitions to update</param>
        /// <param name="requisitions">DTO of the updated requisitions</param>
        /// <returns>A Requisitions object <see cref="Dtos.Requisitions"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.UpdateRequisitions })]
        [HeaderVersionRoute("/requisitions/{guid}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRequisitionsV11_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Requisitions>> PutRequisitionsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Requisitions requisitions)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (requisitions == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null requisitions argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(requisitions.Id))
            {
                requisitions.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, requisitions.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                requisitionService.ValidatePermissions(GetPermissionsMetaData());

                //get Data Privacy List
                var dpList = await requisitionService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await requisitionService.ImportExtendedEthosData(await ExtractExtendedData(await requisitionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var requisitionReturn = await requisitionService.UpdateRequisitionsAsync(
                    await PerformPartialPayloadMerge(requisitions, async () => await requisitionService.GetRequisitionsByGuidAsync(guid, true),
                    dpList, logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                    await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return requisitionReturn;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new requisitions
        /// </summary>
        /// <param name="requisitions">DTO of the new requisitions</param>
        /// <returns>A requisitions object <see cref="Dtos.Requisitions"/> in HeDM format</returns>
        [HttpPost]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.UpdateRequisitions })]
        [HeaderVersionRoute("/requisitions", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRequisitionsV11_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Requisitions>> PostRequisitionsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.Requisitions requisitions)
        {
            if (requisitions == null)
            {
                return CreateHttpResponseException("Request body must contain a valid requisitions.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(requisitions.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null requisitions id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                requisitionService.ValidatePermissions(GetPermissionsMetaData());

                //call import extend method that needs the extracted extension data and the config
                await requisitionService.ImportExtendedEthosData(await ExtractExtendedData(await requisitionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var requisitionReturn = await requisitionService.CreateRequisitionsAsync(requisitions);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await requisitionService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { requisitionReturn.Id }));

                return requisitionReturn;
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Delete (DELETE) a requisitions
        /// </summary>
        /// <param name="guid">GUID to desired requisitions</param>
        /// <returns>IActionResult</returns>
        [HttpDelete]
        [PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.DeleteRequisitions })]
        [Route("/requisitions/{guid}", Name = "DefaultDeleteRequisitions", Order = -10)]
        public async Task<IActionResult> DeleteRequisitionsAsync([FromRoute] string guid)
        {
            try
            {
                requisitionService.ValidatePermissions(GetPermissionsMetaData());

                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("id", "guid is a required for delete");
                }
                await requisitionService.DeleteRequisitionAsync(guid);
                return NoContent();
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion
    }
}
