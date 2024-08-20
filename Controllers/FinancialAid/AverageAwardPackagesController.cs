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
    /// Exposes the AverageAwardPackage data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AverageAwardPackagesController : BaseCompressedApiController
    {
        private readonly IAverageAwardPackageService AverageAwardPackageService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for the AverageAwardPackageController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="averageAwardPackageService">averageAwardPackageService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AverageAwardPackagesController(IAdapterRegistry adapterRegistry, IAverageAwardPackageService averageAwardPackageService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            AverageAwardPackageService = averageAwardPackageService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the list of award package averages for the predefined award categories.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The studentId for which to get the award package data</param>
        /// <returns>A list of average award packages that apply to the student</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/average-award-packages", 1, true, Name = "GetAverageAwardPackages")]
        public async Task<ActionResult<IEnumerable<AverageAwardPackage>>> GetAverageAwardPackagesAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null");
            }
            try
            {
                return Ok(await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AverageAwardPackages resource forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AverageAwardPackages resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AverageAwardPackages. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

    }
}
