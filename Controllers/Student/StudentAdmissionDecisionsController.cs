// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for Student Admission Decisions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAdmissionDecisionsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionDecisionController class.
        /// </summary>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAdmissionDecisionsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Student Admission Decisions
        /// </summary>
        /// <returns>All <see cref="Dtos.StudentAdmissionDecisions">StudentAdmissionDecisions.</see></returns>
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.StudentAdmissionDecisions>>> GetStudentAdmissionDecisionsAsync()
        {
            return new List<StudentAdmissionDecisions>();
        }

        /// <summary>
        /// Retrieve (GET) an existing Student Admission Decision
        /// </summary>
        /// <param name="guid">GUID of the studentAdmissionDecision to get</param>
        /// <returns>A studentAdmissionDecision object <see cref="Dtos.StudentAdmissionDecisions"/> in EEDM format</returns>
        [HttpGet]
        public async Task<ActionResult<Dtos.StudentAdmissionDecisions>> GetStudentAdmissionDecisionByGuidAsync([FromQuery] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No admission decision was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Updates an StudentAdmissionDecisions.
        /// </summary>
        /// <param name="studentAdmissionDecisions"><see cref="Dtos.StudentAdmissionDecisions">StudentAdmissionDecisions</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.StudentAdmissionDecisions">StudentAdmissionDecisions</see></returns>
        [HttpPut]
        public async Task<ActionResult<Dtos.StudentAdmissionDecisions>> PutStudentAdmissionDecisionsAsync([FromBody] Dtos.StudentAdmissionDecisions studentAdmissionDecisions)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a StudentAdmissionDecisions.
        /// </summary>
        /// <param name="studentAdmissionDecisions"><see cref="Dtos.StudentAdmissionDecisions">StudentAdmissionDecisions</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.StudentAdmissionDecisions">StudentAdmissionDecisions</see></returns>
        [HttpPost]
        public async Task<ActionResult<Dtos.StudentAdmissionDecisions>> PostStudentAdmissionDecisionsAsync([FromBody] Dtos.StudentAdmissionDecisions studentAdmissionDecisions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing StudentAdmissionDecisions
        /// </summary>
        /// <param name="guid">Id of the StudentAdmissionDecisions to delete</param>
        [HttpDelete]
        public async Task<ActionResult<Dtos.StudentAdmissionDecisions>> DeleteStudentAdmissionDecisionsAsync(string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
