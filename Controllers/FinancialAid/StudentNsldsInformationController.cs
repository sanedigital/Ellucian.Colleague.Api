// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Net;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Api.Utility;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// StudentNsldsInformationController class
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentNsldsInformationController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IStudentNsldsInformationService studentNsldsInformationService;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry">adapter registry</param>
        /// <param name="studentNsldsInformationService">student nslds information service</param>
        /// <param name="logger">logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentNsldsInformationController(IAdapterRegistry adapterRegistry, IStudentNsldsInformationService studentNsldsInformationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.studentNsldsInformationService = studentNsldsInformationService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets student NSLDS related information
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve nslds information</param>
        /// <returns>StudentNsldsInformation DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/nslds-information", 1, true, Name = "GetStudentNsldsInformation")]
        public async Task<ActionResult<StudentNsldsInformation>> GetStudentNsldsInformationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentNullException("studentId")));
            }            
            try
            {
                return Ok(await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student NSLDS information forbidden. See log for details.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException(string.Format("No StudentNsldsInformation was found for student {0}", studentId), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(string.Format("Unknown error occured while trying to retrieve StudentNsldsInformation for student {0}. See log for details", studentId));
            }
        }
    }
}
