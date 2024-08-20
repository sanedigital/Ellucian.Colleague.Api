// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.ComponentModel;
using System.Linq;
using System.Net;

using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;

using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Student.Services;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Student Affiliation data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAffiliationsController : BaseCompressedApiController
    {
        private readonly IStudentAffiliationService _studentAffiliationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAffiliationsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentAffiliationService">Repository of type <see cref="IStudentAffiliationService">IStudentAffiliationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAffiliationsController(IAdapterRegistry adapterRegistry, IStudentAffiliationService studentAffiliationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentAffiliationService = studentAffiliationService;
            this._logger = logger;
        }

        /// <summary>
        /// Get a list of Student Affiliations from a list of Student keys
        /// </summary>
        /// <param name="criteria">DTO Object containing List of Student Keys, Affiliation and Term.</param>
        /// <returns>List of StudentAffiliation Objects <see cref="Ellucian.Colleague.Dtos.Student.StudentTerm">StudentAffiliation</see></returns>
        /// <accessComments>
        /// API endpoint is secured with the VIEW.STUDENT.INFORMATION permission code
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/student-affiliations", 1, true, Name = "QueryStudentAffiliations")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentAffiliation>>> QueryStudentAffiliationsAsync([FromBody] StudentAffiliationQueryCriteria criteria)
        {
            if (criteria != null && criteria.StudentIds.Count() <= 0)
            {
                _logger.LogError("Invalid studentIds parameter");
                return CreateHttpResponseException("The studentIds are required.", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentAffiliationService.QueryStudentAffiliationsAsync(criteria));

            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "QueryStudentAffiliations error");
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}
