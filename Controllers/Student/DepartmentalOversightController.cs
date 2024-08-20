// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;



namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to departmental oversight data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class DepartmentalOversightController : BaseCompressedApiController
    {
        private readonly IDepartmentalOversightService _departmentalOversightService;
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentalOversightService">Service of type <see cref="IDepartmentalOversightService">IDepartmentalOversightService</see> </param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DepartmentalOversightController(IDepartmentalOversightService departmentalOversightService,
            ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _departmentalOversightService = departmentalOversightService;
            this._logger = logger;
        }

        /// <summary>
        /// Search sections by their name or by assigned faculty ID/name
        /// The Departmental oversight can do this search for the sections under his/her Departments
        /// </summary>
        /// <param name="criteria">DeptOversightSearchCriteria object </param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden exception returned if user does not have permission to access this data.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest exception returned for any other unexpected error.</exception>
        /// <returns> a list of DeptOversightSearchResult objects to be displayed in Self service</returns>
        /// <accessComments>
        /// Any authenticated user who has the View.Person.Information permission can access this end point
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/departmental-oversight", 1, true, Name = "QueryDepartmentalOversight")]
        public async Task<ActionResult<IEnumerable<DeptOversightSearchResult>>>QueryDepartmentalOversightByPostAsync([FromBody] DeptOversightSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            _logger.LogInformation("Entering QueryDepartmentalOversightByPostAsync");
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                var deptOversightSearchResults = await _departmentalOversightService.SearchAsync(criteria, pageSize, pageIndex);
                              
                watch.Stop();
                _logger.LogInformation("QueryDepartmentalOversightByPostAsync... completed in " + watch.ElapsedMilliseconds.ToString());

                return Ok((IEnumerable<DeptOversightSearchResult>)deptOversightSearchResults);
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving sections for the departmental oversight person";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the departmental oversight permissions asynchronous.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns><see cref="DepartmentalOversightPermissions">Departmental oversight permissions for the current user</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/departmental-oversight/permissions", 1, true, Name = "GetDepartmentalOversightPermissions")]
        public async Task<ActionResult<DepartmentalOversightPermissions>> GetDepartmentalOversightPermissionsAsync()
        {
            try
            {
                return await _departmentalOversightService.GetDepartmentalOversightPermissionsAsync();
            }
            catch (ColleagueSessionExpiredException tex)
            {
                var message = "Session has expired while retrieving sections for the departmental oversight permissions";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = "An error occurred while retrieving departmental oversight permissions.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves Departmental Oversight and Faculty details.
        /// </summary>
        /// <param name="criteria">DeptOversightDetailsCriteria object </param>
        /// <returns>List of <see cref="DepartmentalOversight">DepartmentalOversight and Faculty</see></returns>
        /// <accessComments>
        /// 1. A faculty or departmental oversight user who is assigned to the requested course section can view details like first name, last name of the person performing the crud operations.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/departmental-oversight-details", 1, true, Name = "QueryDepartmentalOversightDetails")]
        public async Task<ActionResult<IEnumerable<DepartmentalOversight>>> QueryDepartmentalOversightDetailsAsync([FromBody] DeptOversightDetailsCriteria criteria)
        {
            if (string.IsNullOrEmpty(criteria.SectionId))
            {
                string errorText = "SectionId must be present";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            try
            {
                if (criteria.Ids != null)
                {
                    bool useCache = true;
                    if (Request.GetTypedHeaders().CacheControl != null)
                    {
                        if (Request.GetTypedHeaders().CacheControl.NoCache)
                        {
                            useCache = false;
                        }
                    }
                    return Ok(await _departmentalOversightService.QueryDepartmentalOversightAsync(criteria.Ids, criteria.SectionId, useCache));
                }
                else
                {
                    throw new ArgumentNullException("Ids", "IDs cannot be empty/null for departmental oversight details retrieval.");
                }
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving departmental oversight details.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                var message = "User does not have appropriate permissions to retrieve departmental oversight details.";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "An error occurred while retrieving departmental oversight details.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.InternalServerError);
            }
        }

    }
}
