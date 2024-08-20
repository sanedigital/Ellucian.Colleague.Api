// Copyright 2016-2024 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for general ledger transactions for the Ellucian Data Model.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerTransactionsController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerTransactionService generalLedgerTransactionService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GeneralLedgerTransactionController object
        /// </summary>
        /// <param name="generalLedgerTransactionService">General Ledger Transaction service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GeneralLedgerTransactionsController(IGeneralLedgerTransactionService generalLedgerTransactionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.generalLedgerTransactionService = generalLedgerTransactionService;
            this.logger = logger;
        }


        ////////////////////////////////////////
        ////// Ethos Model Version 12 APIs /////
        ///////////// GetById3Async ////////////
        /////////////   Get3Async   ////////////
        ///////////// Update3Async  ////////////
        ///////////// Create3Async  ////////////
        ////////////////////////////////////////

        #region Ethos Model Version 12 APIs 

        /// <summary>
        /// Retrieves a specified general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>A GeneralLedgerTransaction DTO</returns>
        [HttpGet, PermissionsFilter(ColleagueFinancePermissionCodes.CreateGLPostings)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions/{id}", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetGeneralLedgerTransactionDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction3>> GetById3Async([FromRoute] string id)
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
                generalLedgerTransactionService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var generalLedgerTransaction = await generalLedgerTransactionService.GetById3Async(id);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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


        /// <summary>
        /// Retrieves all general ledger transactions for the data model version 12
        /// Paging implemented in 2024.
        /// </summary>
        /// <param name="page">limit and offset values</param>
        /// <returns>A Collection of GeneralLedgerTransactions <see cref="Dtos.GeneralLedgerTransaction3"/> delimited by the limit and offset values.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(ColleagueFinancePermissionCodes.CreateGLPostings)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/general-ledger-transactions", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllGeneralLedgerTransactionDefault", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.GeneralLedgerTransaction3>>> Get3Async(Paging page)
        {
            try
            {
                generalLedgerTransactionService.ValidatePermissions(GetPermissionsMetaData());

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

                var pageOfItems = await generalLedgerTransactionService.Get3Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.GeneralLedgerTransaction3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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


        /// <summary>
        /// Update a single general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, PermissionsFilter(ColleagueFinancePermissionCodes.CreateGLPostings)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions/{id}", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGeneralLedgerTransactionV12_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction3>> Update3Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction3 generalLedgerDto)
        {
            try
            {
                generalLedgerTransactionService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is a required for update");
                }
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (string.IsNullOrEmpty(generalLedgerDto.Id))
                {
                    generalLedgerDto.Id = id.ToUpperInvariant();
                }
                if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("id", "Invalid id value. Nil GUID cannot be used in PUT operation.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Update3Async(id, generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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


        /// <summary>
        /// Create a single general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.CreateGLPostings)]
        [HeaderVersionRoute("/general-ledger-transactions", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGeneralLedgerTransactionV12_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction3>> Create3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction3 generalLedgerDto)
        {
            try
            {
                generalLedgerTransactionService.ValidatePermissions(GetPermissionsMetaData());
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (generalLedgerDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentException("Non-empty general ledger transaction id not allowed in POST operation. You cannot update an existing general ledger transaction via POST.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Create3Async(generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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



        /// <summary>
        /// Delete a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/general-ledger-transactions/{id}", Name = "DeleteGeneralLedgerTransaction", Order = -10)]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        ////////////////////////////////////////
        ////// Ethos Model Version 8 APIs //////
        //////////// GetById2Async /////////////
        ////////////   Get2Async   /////////////
        //////////// Create2Async  /////////////
        //////////// Update2Async  /////////////
        ////////////////////////////////////////

        #region Ethos Model Version 8 APIs   

        /// <summary>
        /// Retrieves a specified general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>A GeneralLedgerTransaction DTO</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetGeneralLedgerTransactionV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction2>> GetById2Async([FromRoute] string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var generalLedgerTransaction = await generalLedgerTransactionService.GetById2Async(id);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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


        /// <summary>
        /// Retrieves all general ledger transactions for the data model version 8
        /// </summary>
        /// <returns>A Collection of GeneralLedgerTransactions</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/general-ledger-transactions", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllGeneralLedgerTransactionV8", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.GeneralLedgerTransaction2>>> Get2Async()
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
                var generalLedgerTransactions = await generalLedgerTransactionService.Get2Async();

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        generalLedgerTransactions.Select(i => i.Id).ToList()));

                return Ok(generalLedgerTransactions);
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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


        /// <summary>
        /// Create a single general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGeneralLedgerTransactionV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction2>> Create2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction2 generalLedgerDto)
        {
            try
            {
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Create2Async(generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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


        /// <summary>
        /// Update a single general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPut]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGeneralLedgerTransactionV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction2>> Update2Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction2 generalLedgerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is a required for update");
                }
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (string.IsNullOrEmpty(generalLedgerDto.Id))
                {
                    generalLedgerDto.Id = id.ToUpperInvariant();
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Update2Async(id, generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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



        ////////////////////////////////////////
        ////// Ethos Model Version 6 APIs //////
        ///////////// GetByIdAsync /////////////
        /////////////   GetAsync   /////////////
        ///////////// CreateAsync  /////////////
        ///////////// UpdateAsync  /////////////
        ////////////////////////////////////////

        #region Ethos Model Version 6 APIs

        /// <summary>
        /// Retrieves a specified general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>A GeneralLedgerTransaction DTO</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetGeneralLedgerTransaction", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction>> GetByIdAsync([FromRoute] string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var generalLedgerTransaction = await generalLedgerTransactionService.GetByIdAsync(id);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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

        /// <summary>
        /// Retrieves all general ledger transactions for the data model version 6
        /// </summary>
        /// <returns>A Collection of GeneralLedgerTransactions</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/general-ledger-transactions", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllGeneralLedgerTransaction", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.GeneralLedgerTransaction>>> GetAsync()
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
                var generalLedgerTransactions = await generalLedgerTransactionService.GetAsync();

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        generalLedgerTransactions.Select(i => i.Id).ToList()));

                return Ok(generalLedgerTransactions);
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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

        /// <summary>
        /// Create a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGeneralLedgerTransaction", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction>> CreateAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction generalLedgerDto)
        {
            try
            {
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.CreateAsync(generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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

        /// <summary>
        /// Update a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPut]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/general-ledger-transactions/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGeneralLedgerTransaction", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GeneralLedgerTransaction>> UpdateAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction generalLedgerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is a required for update");
                }
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (string.IsNullOrEmpty(generalLedgerDto.Id))
                {
                    generalLedgerDto.Id = id.ToUpperInvariant();
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.UpdateAsync(id, generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
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
            catch (ArgumentNullException e)
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
            catch (ArgumentOutOfRangeException e)
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
            catch (FormatException e)
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
