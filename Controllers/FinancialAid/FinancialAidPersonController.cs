// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.Base;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

using Ellucian.Colleague.Api;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// FinancialAidPerson controller class that contains methods to work with
    /// financial aid persons data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidPersonController : BaseCompressedApiController
    {
        private readonly IFinancialAidPersonService financialAidPersonService;
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private const string _restrictedHeaderName = "X-Content-Restricted";
        private const string _restrictedHeaderValue = "partial";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="financialAidPersonService"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidPersonController(IFinancialAidPersonService financialAidPersonService, IAdapterRegistry adapterRegistry, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.financialAidPersonService = financialAidPersonService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Searches for financial aid persons based on the specified criteria
        /// </summary>
        /// <accessComments>
        /// Users who have VIEW.STUDENT.INFORMATION permission can request other users' data.
        /// </accessComments>
        /// <param name="criteria"></param>
        /// <returns>Set of found(if any) Person DTOs</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/financial-aid-persons", 1, true, Name = "QueryFinancialAidPersonsByPost")]
        public async Task<ActionResult<IEnumerable<Person>>> QueryFinancialAidPersonsByPostAsync([FromBody]FinancialAidPersonQueryCriteria criteria)
        {
            if (criteria == null)
            {
                return CreateHttpResponseException("criteria cannot be null");
            }

            if ((criteria.FinancialAidPersonIds == null || !criteria.FinancialAidPersonIds.Any())
                && string.IsNullOrEmpty(criteria.FinancialAidPersonQueryKeyword))
            {
                return CreateHttpResponseException("criteria must contain either a list of person ids or a query keyword");
            }

            try
            {
                var privacyWrapper = await financialAidPersonService.SearchFinancialAidPersonsAsync(criteria);
                var faPersons = privacyWrapper.Dto as IEnumerable<Person>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append(_restrictedHeaderName, _restrictedHeaderValue);
                }
                return Ok(faPersons);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, "Current User does not have correct permissions");
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, "Could not get financial aid person for the specified criteria");
                return CreateHttpResponseException(ae.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown Exception occurred while getting financial aid persons for specified criteria. See log for details.");
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}
