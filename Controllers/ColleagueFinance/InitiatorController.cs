// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

using System.ComponentModel;
using Ellucian.Web.License;

using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using System.Net;
using Ellucian.Web.Security;
using System;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for initiator
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class InitiatorController : BaseCompressedApiController
    {
        private readonly IInitiatorService initiatorService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the InitiatorController object
        /// </summary>
        /// <param name="initiatorService">Initiator service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InitiatorController(IInitiatorService initiatorService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.initiatorService = initiatorService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the list of initiators based on keyword search.
        /// </summary>
        /// <param name="queryKeyword">parameter for passing search keyword</param>
        /// <returns>The initiator search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.34. Use QueryInitiatorByKeywordAsync.")]
        [HttpGet]
        [HeaderVersionRoute("/initiator/{queryKeyword}", 1, true, Name = "SearchInitiator")]
        public async Task<ActionResult<IEnumerable<Initiator>>> GetInitiatorByKeywordAsync(string queryKeyword)
        {

            if (string.IsNullOrEmpty(queryKeyword))
            {
                string message = "query keyword is required to query.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var initiatorSearchResults = await initiatorService.QueryInitiatorByKeywordAsync(queryKeyword);
                return Ok(initiatorSearchResults);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the initiator info.");
                return CreateHttpResponseException("Insufficient permissions to get the initiator info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to search initiator.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to search initiator");
                return CreateHttpResponseException("Unable to search initiator", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the list of initiators based on keyword search.
        /// </summary>
        /// <param name="criteria">KeywordSearchCriteria parameter for passing search keyword</param>
        /// <returns> The initiator search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/initiator", 1, true, Name = "QuerySearchInitiator")]
        public async Task<ActionResult<IEnumerable<Initiator>>> QueryInitiatorByKeywordAsync([FromBody] KeywordSearchCriteria criteria)
        {
            if (criteria == null || string.IsNullOrEmpty(criteria.Keyword))
            {
                string message = "query keyword is required to query.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var initiatorSearchResults = await initiatorService.QueryInitiatorByKeywordAsync(criteria.Keyword);
                return Ok(initiatorSearchResults);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the initiator info.");
                return CreateHttpResponseException("Insufficient permissions to get the initiator info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to search initiator.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to search initiator");
                return CreateHttpResponseException("Unable to search initiator", HttpStatusCode.BadRequest);
            }
        }
    }
}
