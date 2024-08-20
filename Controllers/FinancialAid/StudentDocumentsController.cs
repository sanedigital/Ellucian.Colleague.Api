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
    /// The StudentDocumentsController exposes a student's financial aid documents
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentDocumentsController : BaseCompressedApiController
    {
        private readonly IStudentDocumentService StudentDocumentService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Dependency Injection constructor for StudentDocumentsController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="studentDocumentService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentDocumentsController(IAdapterRegistry adapterRegistry, IStudentDocumentService studentDocumentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            StudentDocumentService = studentDocumentService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all of a student's financial aid documents across all award years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Id of the student for whom to get documents</param>
        /// <returns>A list of StudentDocument objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception> 
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/documents", 1, true, Name = "GetStudentDocumentsAsync")]
        public async Task<ActionResult<IEnumerable<StudentDocument>>> GetStudentDocumentsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await StudentDocumentService.GetStudentDocumentsAsync(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentDocuments resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("StudentDocuments", studentId);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentDocuments resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }
        
    }
}
