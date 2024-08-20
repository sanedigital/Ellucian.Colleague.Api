// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using Ellucian.Web.Security;
using System.Net;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes Student FA Checklist Items
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidChecklistsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IStudentChecklistService studentChecklistService;
        private readonly ILogger logger;

        /// <summary>
        /// StudentChecklistItemController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentChecklistService">studentChecklistService</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentFinancialAidChecklistsController(IAdapterRegistry adapterRegistry, IStudentChecklistService studentChecklistService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.studentChecklistService = studentChecklistService;
            this.logger = logger;
        }

        /// <summary>
        /// Create a student's Financial Aid Checklist for the specified award year. 
        /// </summary>
        /// <accessComments>
        /// Users may make changes to create their own data.
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to create a checklist</param>
        /// <param name="year">The year for which to create a checklist</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>An HttpResponseMessage with the Location header that specifies URL of the created resource, and the Content property of the object set to the created checklist</returns>
        /// <exception cref="HttpResponseException">400: Thrown if either of the arguments are null or empty, or some other error occurred while creating the checklist</exception>
        /// <exception cref="HttpResponseException">403: Thrown if you don't have permission to create a checklist for the specified student</exception>
        /// <exception cref="HttpResponseException">409: Thrown if a Checklist already exists for this student and year. The Location header specifies the URL to get the existing resource.</exception>"
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/financial-aid-checklists/{year}", 1, true, Name = "CreateStudentFinancialAidChecklistAsync")]
        public async Task<ActionResult<StudentFinancialAidChecklist>> CreateStudentFinancialAidChecklistAsync([FromRoute]string studentId, [FromQuery] string year, [FromQuery] bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId argument is required");
            }

            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year argument is required");
            }

            try
            {
                var newStudentChecklist = await studentChecklistService.CreateStudentChecklistAsync(studentId, year, getActiveYearsOnly);
                return Created(Url.Link("GetStudentFinancialAidChecklistAsync", new { studentId = studentId, year = year }), newStudentChecklist);
            }

            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to create a checklist resource for student {0}", studentId);
                logger.LogError(pex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }

            catch (ExistingResourceException erex)
            {
                logger.LogError(erex, erex.Message);
                SetResourceLocationHeader("GetStudentFinancialAidChecklistAsync", new { studentId = studentId, id = erex.ExistingResourceId });
                return CreateHttpResponseException("Cannot create resource that already exists. See log for details.", System.Net.HttpStatusCode.Conflict);
            }

            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Exception encountered while creating the FinancialAidChecklist resource: " + Environment.NewLine + ae.Message);
            }

            catch (Exception ex)
            {
                var message = "Unknown error occurred creating student checklist items";
                logger.LogError(ex, message);
                return CreateHttpResponseException(message + Environment.NewLine + ex.Message );
            }
        }

        /// <summary>
        /// Get a student's Financial Aid Checklist for a given year.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get the checklist</param>
        /// <param name="year">The year for which to get the checklist</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A StudentFinancialAidChecklist DTO</returns>
        /// <exception cref="HttpResponseException">403: Thrown if you don't have permission to get a checklist for the specified student</exception>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/financial-aid-checklists/{year}", 1, true, Name = "GetStudentFinancialAidChecklistAsync")]
        public async Task<ActionResult<StudentFinancialAidChecklist>> GetStudentFinancialAidChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId argument is required");
            }

            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year argument is required");
            }

            try
            {
                return await studentChecklistService.GetStudentChecklistAsync(studentId, year, getActiveYearsOnly);
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to get checklist item resources for student {0}", studentId);
                logger.LogError(pex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                var message = string.Format("Unknown error occurred getting StudentFinancialAidChecklist resources for id {0} awardYear", studentId, year);
                logger.LogError(ex, message);
                return CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get a student's Financial Aid Checklists for all award years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get checklist items</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentFinancialAidChecklist DTOs</returns>
        /// <exception cref="HttpResponseException">403: Thrown if you don't have permission to get checklists for the specified student</exception>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/financial-aid-checklists", 1, true, Name = "GetAllStudentChecklistsAsync")]
        public async Task<ActionResult<IEnumerable<StudentFinancialAidChecklist>>> GetAllStudentFinancialAidChecklistsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId argument is required");
            }

            try
            {
                return Ok(await studentChecklistService.GetAllStudentChecklistsAsync(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to get StudentFinancialAidChecklist resources for student {0}", studentId);
                logger.LogError(pex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                var message = string.Format("Unknown error occurred getting StudentFinancialAidChecklist resources for id {0}", studentId);
                logger.LogError(ex, message);
                return CreateHttpResponseException(message + Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// Gets a parent's profile for PLUS MPNs
        /// </summary>
        /// <accessComments>
        /// Students can only access the profile of parents specifically assigned to their account on PREL
        /// Proxy users can only access the profiles of parents associated to students for whom they are proxies
        /// FA admin users with the VIEW.FINANCIAL.AID.INFORMATION permission code are able to access parent profiles
        /// </accessComments>
        /// <param name="parentId">The Colleague PERSON id of the parent for whom to get a PROFILE dto for</param>
        /// <param name="studentId">The Colleague PERSON id of the student for whom the parent relates to</param>
        /// <returns>A PROFILE dto for the parent ID</returns>  
        [HttpGet]
        [HeaderVersionRoute("/students/{parentId}/{studentId}/financial-aid-checklists", 1, true, Name = "GetFaProfileAsync")]
        public async Task<ActionResult<Dtos.Base.Profile>> GetFaProfileAsync(string parentId, string studentId)
        {
            try
            {
                bool useCache = true;
                if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }
                return Ok(await studentChecklistService.GetMpnProfileAsync(parentId, studentId, useCache));
            }
            catch (ArgumentNullException anex)
            {
                return CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }

        }

        /// <summary>
        /// Retrieves a student's housing option for a given year
        /// </summary>
        /// <note>
        /// Uses the ST-VAL table FAFSA.HOUSING.CODES09 to translate the housing code
        /// </note>
        /// <remarks>
        /// The housing code is stored in the CS.ACYR record in field 78 (CS.HOUSING.CODE)
        /// </remarks>
        /// <param name="studentId">The Colleague PERSON ID to retrieve a housing option for</param>
        /// <param name="awardYear">The Financial Aid award year to retrieve a housing option for</param>
        /// <returns>A HousingOption object containing the year and housing code for the student</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/financial-aid-checklists/{studentId}/{awardYear}", 1, true, Name = "GetHousingOptionAsync")]
        public async Task<ActionResult<HousingOption>> GetHousingOption([FromRoute] string studentId, [FromRoute] string awardYear)
        {
            try
            {
                return await studentChecklistService.GetSetHousingOptionAsync(studentId, awardYear, null, "G");
            }
            catch (ColleagueException ex)
            {
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Sets a student's housing option for a given year
        /// </summary>
        /// <note>
        /// Uses the ST-VAL table FAFSA.HOUSING.CODES09 to translate the housing code
        /// </note>
        /// <remarks>
        /// The housing code is stored in the CS.ACYR record in field 78 (CS.HOUSING.CODE)
        /// </remarks>
        /// <param name="studentId">The Colleague PERSON ID to set the housing option for</param>
        /// <param name="awardYear">The Financial Aid award year to set the housing option for</param>
        /// <param name="housingOption">The housing option to set for the specified student/year</param>
        /// <param name="forceOption">Either Y/N, indicates to force an update to CS.HOUSING.CODE even if it is already populated. Otherwise, the value will only be set if it is currently null</param>
        /// <returns>A HousingOption object containing the year and housing code for the student, either updated with the new value or the current (unmodified) one</returns>
        [HttpPost]
        [HeaderVersionRoute("/students/financial-aid-checklists/{studentId}/{awardYear}/{housingOption}/{forceOption}", 1, true, Name = "SetHousingOptionAsync")]
        public async Task<ActionResult<HousingOption>> SetHousingOption([FromRoute] string studentId, [FromRoute] string awardYear, [FromRoute] string housingOption, [FromRoute] string forceOption)
        {
            try
            {
                var setOption = (forceOption == "Y" ? "F" : "S");
                return await studentChecklistService.GetSetHousingOptionAsync(studentId, awardYear, housingOption, setOption);
            }
            catch (ColleagueException ex)
            {
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }
}
