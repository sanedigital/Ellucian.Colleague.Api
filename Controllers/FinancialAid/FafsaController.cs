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
using System.Net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes FAFSA data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FafsaController : BaseCompressedApiController
    {
        private readonly IFafsaService fafsaService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for the FafsaController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="fafsaService">fafsaService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FafsaController(IAdapterRegistry adapterRegistry, IFafsaService fafsaService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.fafsaService = fafsaService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves many FAFSA objects at once using a FafsaQueryCriteria object. This endpoint gets the federally flagged FAFSA 
        /// object for each student/awardYear.
        /// </summary>
        /// <param name="criteria">criteria object including a comma delimited list of IDs from request body</param>
        /// <returns>List of <see cref="Fafsa">Objects</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/fafsa", 1, true, Name = "Fafsa")]
        public async Task<ActionResult<IEnumerable<Fafsa>>> QueryFafsaByPostAsync([FromBody] FafsaQueryCriteria criteria)
        {
            try
            {
                return Ok(await fafsaService.QueryFafsaAsync(criteria));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a list of all FAFSAs for the given student id
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id for whom to retrieve FAFSAs</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of FAFSA objects assigned to the student</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/fafsas", 1, true, Name = "AllStudentFafsas")]
        public async Task<ActionResult<IEnumerable<Fafsa>>> GetStudentFafsasAsync([FromRoute] string studentId, [FromQuery]bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("StudentId is required in request");
            }
            try
            {
                return Ok(await fafsaService.GetStudentFafsasAsync(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pex)
            {
                logger.LogError(pex, "Permisions exception getting data for student {0}", studentId);
                return CreateHttpResponseException("You do not have permission to get FAFSAs for this student", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting FAFSAs for student {0}", studentId);
                return CreateHttpResponseException("Unknown error occurred. See log for details.");
            }
        }
    }
}
