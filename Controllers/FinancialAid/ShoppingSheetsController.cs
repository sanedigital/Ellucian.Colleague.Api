// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes access to Student-specific Financial Aid Shopping Sheet data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ShoppingSheetsController : BaseCompressedApiController
    {
        private readonly IShoppingSheetService shoppingSheetService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Dependency Injection constructor for ShoppingSheetsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="shoppingSheetService">ShoppingSheetService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ShoppingSheetsController(IAdapterRegistry adapterRegistry, IShoppingSheetService shoppingSheetService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.shoppingSheetService = shoppingSheetService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all shopping sheet resources for the given student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get shopping sheets</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of all shopping sheets for the given student</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/shopping-sheets", 1, false, Name = "GetShoppingSheets")]
        public async Task<ActionResult<IEnumerable<ShoppingSheet>>> GetShoppingSheetsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(await shoppingSheetService.GetShoppingSheetsAsync(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("User does not have access rights to student {0}", studentId);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred getting shopping sheet resources");
                return CreateHttpResponseException("Unknown error occurred getting shopping sheet resources");
            }
        }

        /// <summary>
        /// Get all shopping sheet resources for the given student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId"></param>
        /// <param name="getActiveYearsOnly"></param>
        /// <returns>A list of all shopping sheets for the given student</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/shopping-sheets", 2, false, Name = "GetShoppingSheets2")]
        public async Task<ActionResult<IEnumerable<ShoppingSheet2>>> GetShoppingSheets2Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(await shoppingSheetService.GetShoppingSheets2Async(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("User does not have access rights to student {0}", studentId);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred getting shopping sheet resources");
                return CreateHttpResponseException("Unknown error occurred getting shopping sheet resources");
            }
        }

        /// <summary>
        /// Get all shopping sheet resources for the given student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId"></param>
        /// <param name="getActiveYearsOnly"></param>
        /// <returns>A list of all shopping sheets for the given student</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/shopping-sheets", 3, true, Name = "GetShoppingSheets3")]
        public async Task<ActionResult<IEnumerable<ShoppingSheet3>>> GetShoppingSheets3Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(await shoppingSheetService.GetShoppingSheets3Async(studentId, getActiveYearsOnly));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("User does not have access rights to student {0}", studentId);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred getting shopping sheet resources");
                return CreateHttpResponseException("Unknown error occurred getting shopping sheet resources");
            }
        }


    }
}
