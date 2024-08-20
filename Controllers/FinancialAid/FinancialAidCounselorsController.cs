// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Controller class for FinancialAidCounselors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidCounselorsController : BaseCompressedApiController
    {
        private readonly IFinancialAidCounselorService financialAidCounselorService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for FinancialAidCounselorsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidCounselorService">FinancialAidCounselorService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidCounselorsController(IAdapterRegistry adapterRegistry, IFinancialAidCounselorService financialAidCounselorService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.financialAidCounselorService = financialAidCounselorService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Get a FinancialAidCounselor object for the given counselorId
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="counselorId">Colleague PERSON id of the counselor to get</param>
        /// <returns>FinancialAidCounselor object</returns>
        /// <exception cref="HttpResponseException">400, Thrown if the counselor id is null or empty, or if some unknown error occurs</exception>
        /// <exception cref="HttpResponseException">403, Thrown if the access to the counselor resource is forbidden</exception>
        /// <exception cref="HttpResponseException">404, Thrown if the counselor with the given id cannot be found or is not an active staff member</exception>
        /// <note>Staff Entity is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-counselors/{counselorId}", 1, true, Name = "GetFinancialAidCounselors")]
        public ActionResult<FinancialAidCounselor> GetCounselor(string counselorId)
        {
            if (string.IsNullOrEmpty(counselorId))
            {
                return CreateHttpResponseException("counselorId cannot be null");
            }
            try
            {
                return Ok(financialAidCounselorService.GetCounselor(counselorId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to FinancialAidCounselor forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("FinancialAidCounselor", counselorId);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateNotFoundException("FinancialAidCounselor", counselorId);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting FinancialAidCounselor resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get FinancialAidCounselor DTOs list for given counselor ids.
        /// If a specified record is not found to be a valid staff type, that does not cause an exception, instead,
        /// item is not returned in a list
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="criteria">Query criteria</param>
        /// <returns>List of FinancialAidCounselor DTOs</returns>
        /// <note>Staff is cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/financial-aid-counselors", 1, true, Name = "QueryFinancialAidCounselors")]
        public async Task<ActionResult<IEnumerable<FinancialAidCounselor>>> QueryFinancialAidCounselorsAsync([FromBody]FinancialAidCounselorQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            try
            {
                return Ok(await financialAidCounselorService.GetCounselorsByIdAsync(criteria.FinancialAidCounselorIds));
            }
            catch (PermissionsException pex)
            {
                logger.LogError(pex, pex.Message);
                return CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message);
            }
            
        }
    }
}
