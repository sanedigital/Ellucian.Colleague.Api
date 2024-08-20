// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Exceptions;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.ColleagueFinance;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to ProcurementReceipts
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ProcurementReceiptsController : BaseCompressedApiController
    {
        private readonly IProcurementReceiptsService _procurementReceiptsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ProcurementReceiptsController class.
        /// </summary>
        /// <param name="procurementReceiptsService">Service of type <see cref="IProcurementReceiptsService">IProcurementReceiptsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProcurementReceiptsController(IProcurementReceiptsService procurementReceiptsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _procurementReceiptsService = procurementReceiptsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all ProcurementReceipts
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">QueryStringFilter</param>
        /// <returns>List of ProcurementReceipts <see cref="Dtos.ProcurementReceipts"/> objects representing matching procurementReceipts</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewProcurementReceipts, ColleagueFinancePermissionCodes.CreateProcurementReceipts })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.ProcurementReceipts))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/procurement-receipts", "13.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetProcurementReceipts", IsEedmSupported = true)]
        public async Task<IActionResult> GetProcurementReceiptsAsync(Paging page, QueryStringFilter criteria)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                _procurementReceiptsService.ValidatePermissions(GetPermissionsMetaData());
                var criteriaValues = GetFilterObject<Dtos.ProcurementReceipts>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.ProcurementReceipts>>(new List<Dtos.ProcurementReceipts>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _procurementReceiptsService.GetProcurementReceiptsAsync(page.Offset, page.Limit, criteriaValues, bypassCache);

                AddEthosContextProperties(
                  await _procurementReceiptsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _procurementReceiptsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.ProcurementReceipts>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a ProcurementReceipts using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired procurementReceipts</param>
        /// <returns>A procurementReceipts object <see cref="Dtos.ProcurementReceipts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewProcurementReceipts, ColleagueFinancePermissionCodes.CreateProcurementReceipts })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/procurement-receipts/{guid}", "13.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetProcurementReceiptsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ProcurementReceipts>> GetProcurementReceiptsByGuidAsync(string guid)
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
                _procurementReceiptsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _procurementReceiptsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _procurementReceiptsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync(guid);
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
        /// Create (POST) a new procurementReceipts
        /// </summary>
        /// <param name="procurementReceipts">DTO of the new procurementReceipts</param>
        /// <returns>A procurementReceipts object <see cref="Dtos.ProcurementReceipts"/> in EEDM format</returns>
        [HttpPost, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.CreateProcurementReceipts })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/procurement-receipts", "13.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostProcurementReceiptsV13_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ProcurementReceipts>> PostProcurementReceiptsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.ProcurementReceipts procurementReceipts)
        {
            if (procurementReceipts == null)
            {
                return CreateHttpResponseException("Request body must contain a valid procurementReceipts.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(procurementReceipts.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null procurementReceipts id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (procurementReceipts.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                _procurementReceiptsService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _procurementReceiptsService.ImportExtendedEthosData(await ExtractExtendedData(await _procurementReceiptsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var procurementReceipt=  await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReceipts);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _procurementReceiptsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _procurementReceiptsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { procurementReceipt.Id }));

                return procurementReceipt;
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing procurementReceipts
        /// </summary>
        /// <param name="guid">GUID of the procurementReceipts to update</param>
        /// <param name="procurementReceipts">DTO of the updated procurementReceipts</param>
        /// <returns>A procurementReceipts object <see cref="Dtos.ProcurementReceipts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/procurement-receipts/{guid}", "13.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutProcurementReceiptsV13_1_0")]
        public async Task<ActionResult<Dtos.ProcurementReceipts>> PutProcurementReceiptsAsync([FromRoute] string guid, [FromBody] Dtos.ProcurementReceipts procurementReceipts)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a procurementReceipts
        /// </summary>
        /// <param name="guid">GUID to desired procurementReceipts</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/procurement-receipts/{guid}", Name = "DefaultDeleteProcurementReceipts", Order = -10)]
        public async Task<IActionResult> DeleteProcurementReceiptsAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
