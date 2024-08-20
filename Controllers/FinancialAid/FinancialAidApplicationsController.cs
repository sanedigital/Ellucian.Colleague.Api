// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Provides access to FinancialAidApplications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidApplicationsController : BaseCompressedApiController
    {
        private readonly IFinancialAidApplicationService financialAidApplicationService;
        //private readonly IFinancialAidApplicationService2 financialAidApplicationService2;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidApplicationsController class.
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>   
        /// <param name="financialAidApplicationService">FinancialAidApplicationService</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidApplicationsController(IAdapterRegistry adapterRegistry,
            IFinancialAidApplicationService financialAidApplicationService,
            ILogger logger,
            IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.financialAidApplicationService = financialAidApplicationService;
            //this.financialAidApplicationService2 = financialAidApplicationService2;
            this.logger = logger;
        }        

        /// <summary>
        /// Obsolete as of API version 1.7. Deprecated. Use FAFSA and ProfileApplication endpoints instead.
        /// Get a list of FinancialAidApplication objects for all years the student has application data in Colleague.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The studentId for which to get the award data</param>
        /// <returns>A list of FinancialAidApplication DTOs</returns>
        [Obsolete("Obsolete as of API version 1.7. Deprecated. Get Financial Aid Applications using GET /students/{studentId}/fafsas and GET /students/{studentId}/profile-applications")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/financial-aid-applications", 1, false, Name = "GetFinancialAidApplications")]
        public ActionResult<IEnumerable<FinancialAidApplication>> GetFinancialAidApplications(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null");
            }
            try
            {
                return Ok(financialAidApplicationService.GetFinancialAidApplications(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to financial aid applications resource forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find financial aid applications resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting financial aid applications. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

    }
}
