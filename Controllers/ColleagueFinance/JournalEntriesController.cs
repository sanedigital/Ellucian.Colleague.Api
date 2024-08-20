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
using Ellucian.Web.Security;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for journal entries
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class JournalEntriesController : BaseCompressedApiController
    {
        private readonly IJournalEntryService journalEntryService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the JournalEntriesController object
        /// </summary>
        /// <param name="journalEntryService">Journal Entry service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public JournalEntriesController(IJournalEntryService journalEntryService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.journalEntryService = journalEntryService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified journal entry
        /// </summary>
        /// <param name="journalEntryId">The requested journal entry ID</param>
        /// <returns>A Journal Entry DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.JOURNAL.ENTRY, and requires access to at least one of the
        /// general ledger numbers on the journal entry.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/journal-entries/{journalEntryId}", 1, true, Name = "GetJournalEntry")]
        public async Task<ActionResult<JournalEntry>> GetJournalEntryAsync(string journalEntryId)
        {
            if (string.IsNullOrEmpty(journalEntryId))
            {
                string message = "A Journal Number must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var journalEntry = await journalEntryService.GetJournalEntryAsync(journalEntryId);
                return journalEntry;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the journal entry.", HttpStatusCode.Forbidden);
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
                logger.LogDebug(csee, "Session expired - unable to get the journal entry.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the journal entry.", HttpStatusCode.BadRequest);
            }
        }
    }
}
