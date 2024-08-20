// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to course Section data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionsMaximumController : BaseCompressedApiController
    {
        private readonly ISectionCoordinationService _sectionCoordinationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionsController class.
        /// </summary>
        /// <param name="sectionCoordinationService">Service of type <see cref="ISectionCoordinationService">ISectionCoordinationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionsMaximumController(ISectionCoordinationService sectionCoordinationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sectionCoordinationService = sectionCoordinationService;
            this._logger = logger;
        }

        #region HeDM Methods

        /// <summary>
        /// Return a list of SectionMaximum objects based on selection criteria.
        /// </summary>
        /// <param name="page">Section page Contains ...page...</param>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevels">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningInstitutionUnits">Section Department equal to (guid)</param>
        /// <returns>List of SectionMaximum <see cref="Dtos.SectionMaximum2"/> objects representing matching SectionMaximum</returns>
        [HttpGet]
        [ValidateQueryStringFilter(new string[] { "title", "startOn", "endOn", "code", "number", "instructionalPlatform", "academicPeriod", "academicLevels", "course", "site", "status", "owningInstitutionUnits" }, false, true)] 
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetHedmSectionsMaximumV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmSectionsMaximum2Async(Paging page, [FromQuery] string title = "", [FromQuery] string startOn = "", [FromQuery] string endOn = "",
            [FromQuery] string code = "", [FromQuery] string number = "", [FromQuery] string instructionalPlatform = "", [FromQuery] string academicPeriod = "",
            [FromQuery] string academicLevels = "", [FromQuery] string course = "", [FromQuery] string site = "", [FromQuery] string status = "", [FromQuery] string owningInstitutionUnits = "")
        {
            string criteria = title + startOn + endOn + code + number + instructionalPlatform + academicPeriod + academicLevels + course + site + status + owningInstitutionUnits;
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException("title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels, course, site, status, owningInstitutionUnits", 
                    "No criteria specified for selection of sections");
            }
            //valid query parameter but empty argument
            if (string.IsNullOrEmpty(criteria.Replace("\"", "")))
            {
                return new PagedActionResult<IEnumerable<Dtos.SectionMaximum2>>(new List<Dtos.SectionMaximum2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            if ((!string.IsNullOrEmpty(status)) && (!ValidEnumerationValue(typeof(SectionStatus2), status)))
            {
                throw new ColleagueWebApiException(string.Concat("'", status, "' is an invalid enumeration value. "));
            }

            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                //AddDataPrivacyContextProperty((await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var pageOfItems = await _sectionCoordinationService.GetSectionsMaximum2Async(page.Offset, page.Limit, title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels, course, site, status, owningInstitutionUnits);
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedActionResult<IEnumerable<Dtos.SectionMaximum2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Return a list of SectionMaximum objects based on selection criteria.
        /// </summary>
        /// <param name="page">Section page Contains ...page...</param>
        /// <param name="criteria"> filter criteria</param>
        /// <returns>List of SectionMaximum <see cref="Dtos.SectionMaximum3"/> objects representing matching SectionMaximum</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.SectionMaximumFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections", 8, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetHedmSectionsMaximumV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmSectionsMaximum3Async(Paging page, QueryStringFilter criteria)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                   number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                   course = string.Empty, site = string.Empty, status = string.Empty;
            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();

            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaObj = GetFilterObject<Dtos.Filters.SectionMaximumFilter>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = ((criteriaObj.InstructionalPlatform != null) 
                        && (criteriaObj.InstructionalPlatform.Detail != null) 
                        && (!string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Detail.Id)))
                        ? criteriaObj.InstructionalPlatform.Detail.Id : string.Empty;
                    academicPeriod = ((criteriaObj.AcademicPeriod != null)
                        && (criteriaObj.AcademicPeriod.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.AcademicPeriod.Detail.Id)))
                        ? criteriaObj.AcademicPeriod.Detail.Id : string.Empty;
                    course = ((criteriaObj.Course != null)
                        && (criteriaObj.Course.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.Course.Detail.Id)))
                        ? criteriaObj.Course.Detail.Id : string.Empty;
                    site = ((criteriaObj.Site != null)
                        && (criteriaObj.Site.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.Site.Detail.Id)))
                        ? criteriaObj.Site.Detail.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status != SectionStatus2.NotSet))
                        ? criteriaObj.Status.ToString() : string.Empty;
                    if ((criteriaObj.AcademicLevels != null) && (criteriaObj.AcademicLevels.Any()))
                    {
                        var academiclevel = new List<string>();
                        foreach (var acadLevel in criteriaObj.AcademicLevels)
                        {
                            if ((acadLevel != null) && (acadLevel.Detail != null))
                            {
                                academiclevel.Add(acadLevel.Detail.Id);
                            }
                        }
                        academicLevels = academiclevel;
                    }
                    if ((criteriaObj.OwningOrganizations != null) && (criteriaObj.OwningOrganizations.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningOrganizations)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.Detail != null))
                            {
                                organizations.Add(owningInstitutionUnit.Detail.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.SectionMaximum3>>(new List<Dtos.SectionMaximum3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                                
                var pageOfItems = await _sectionCoordinationService.GetSectionsMaximum3Async(page.Offset, page.Limit, title, startOn, endOn, code, 
                    number, instructionalPlatform, 
                    academicPeriod, academicLevels, course, site, status, owningOrganizations);
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedActionResult<IEnumerable<Dtos.SectionMaximum3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Return a list of SectionMaximum objects based on selection criteria.
        /// </summary>
        /// <param name="page">Section page Contains ...page...</param>
        /// <param name="criteria"> filter criteria</param>
        /// <returns>List of SectionMaximum <see cref="Dtos.SectionMaximum4"/> objects representing matching SectionMaximum</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.SectionMaximum4))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections", "11", false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetHedmSectionsMaximumV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmSectionsMaximum4Async(Paging page, QueryStringFilter criteria)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                 number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                 course = string.Empty, site = string.Empty, status = string.Empty;
            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();


            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaObj = GetFilterObject<Dtos.SectionMaximum4>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = ((criteriaObj.InstructionalPlatform != null)
                        && (criteriaObj.InstructionalPlatform.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Detail.Id)))
                        ? criteriaObj.InstructionalPlatform.Detail.Id : string.Empty;
                    academicPeriod = ((criteriaObj.AcademicPeriod != null)
                        && (criteriaObj.AcademicPeriod.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.AcademicPeriod.Detail.Id)))
                        ? criteriaObj.AcademicPeriod.Detail.Id : string.Empty;
                    course = ((criteriaObj.Course != null)
                        && (criteriaObj.Course.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.Course.Detail.Id)))
                        ? criteriaObj.Course.Detail.Id : string.Empty;
                    site = ((criteriaObj.Site != null)
                        && (criteriaObj.Site.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.Site.Detail.Id)))
                        ? criteriaObj.Site.Detail.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;
                    if ((criteriaObj.AcademicLevels != null) && (criteriaObj.AcademicLevels.Any()))
                    {
                        var academiclevel = new List<string>();
                        foreach (var acadLevel in criteriaObj.AcademicLevels)
                        {
                            if ((acadLevel != null) && (acadLevel.Detail != null))
                            {
                                academiclevel.Add(acadLevel.Detail.Id);
                            }
                        }
                        academicLevels = academiclevel;
                    }
                    if ((criteriaObj.OwningOrganizations != null) && (criteriaObj.OwningOrganizations.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningOrganizations)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.Detail != null))
                            {
                                organizations.Add(owningInstitutionUnit.Detail.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.SectionMaximum4>>(new List<Dtos.SectionMaximum4>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _sectionCoordinationService.GetSectionsMaximum4Async(page.Offset, page.Limit, title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels, course, site, status, owningOrganizations);
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.SectionMaximum4>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Return a list of SectionMaximum objects based on selection criteria.
        /// </summary>
        /// <param name="page">Section page Contains ...page...</param>
        /// <param name="criteria"> filter criteria</param>
        /// <returns>List of SectionMaximum <see cref="Dtos.SectionMaximum5"/> objects representing matching SectionMaximum</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.SectionMaximum5))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections", "16.0.0", false, RouteConstants.HedtechIntegrationSectionsMaximumMediaTypeFormat, Name = "GetHedmSectionsMaximumV16.0.0", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmSectionsMaximum5Async(Paging page, QueryStringFilter criteria)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                 number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty, scheduleAcademicPeriod = string.Empty,
                 course = string.Empty, site = string.Empty, status = string.Empty, reportingAcademicPeriod = string.Empty;
            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>(), instructors = new List<string>();


            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaObj = GetFilterObject<Dtos.SectionMaximum5>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = ((criteriaObj.InstructionalPlatform != null)
                        && (criteriaObj.InstructionalPlatform.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Detail.Id)))
                        ? criteriaObj.InstructionalPlatform.Detail.Id : string.Empty;
                    academicPeriod = ((criteriaObj.AcademicPeriod != null)
                        && (criteriaObj.AcademicPeriod.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.AcademicPeriod.Detail.Id)))
                        ? criteriaObj.AcademicPeriod.Detail.Id : string.Empty;
                    scheduleAcademicPeriod = ((criteriaObj.ScheduleAcademicPeriod != null)
                        && (criteriaObj.ScheduleAcademicPeriod.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.ScheduleAcademicPeriod.Detail.Id)))
                        ? criteriaObj.ScheduleAcademicPeriod.Detail.Id : string.Empty;
                    reportingAcademicPeriod = ((criteriaObj.ReportingAcademicPeriod != null)
                        && (!string.IsNullOrEmpty(criteriaObj.ReportingAcademicPeriod.Id)))
                        ? criteriaObj.ReportingAcademicPeriod.Id : string.Empty;
                    course = ((criteriaObj.Course != null)
                        && (criteriaObj.Course.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.Course.Detail.Id)))
                        ? criteriaObj.Course.Detail.Id : string.Empty;
                    site = ((criteriaObj.Site != null)
                        && (criteriaObj.Site.Detail != null)
                        && (!string.IsNullOrEmpty(criteriaObj.Site.Detail.Id)))
                        ? criteriaObj.Site.Detail.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;
                    if ((criteriaObj.AcademicLevels != null) && (criteriaObj.AcademicLevels.Any()))
                    {
                        var academiclevel = new List<string>();
                        foreach (var acadLevel in criteriaObj.AcademicLevels)
                        {
                            if ((acadLevel != null) && (acadLevel.Detail != null) && (!string.IsNullOrEmpty(acadLevel.Detail.Id)))
                            {
                                academiclevel.Add(acadLevel.Detail.Id);
                            }
                        }
                        academicLevels = academiclevel;
                    }
                    if ((criteriaObj.InstructorRoster != null) && (criteriaObj.InstructorRoster.Any()))
                    {
                        var instrs = new List<string>();
                        foreach (var instr in criteriaObj.InstructorRoster)
                        {
                            if ((instr != null) && (instr.Instructor != null) && (instr.Instructor.Detail != null) && (!string.IsNullOrEmpty(instr.Instructor.Detail.Id)))
                            {
                                instrs.Add(instr.Instructor.Detail.Id);
                            }
                        }
                        instructors = instrs;
                    }
                    if ((criteriaObj.OwningOrganizations != null) && (criteriaObj.OwningOrganizations.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningOrganizations)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.Detail != null) && (!string.IsNullOrEmpty(owningInstitutionUnit.Detail.Id)))
                            {
                                organizations.Add(owningInstitutionUnit.Detail.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.SectionMaximum5>>(new List<Dtos.SectionMaximum5>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                
                var pageOfItems = await _sectionCoordinationService.GetSectionsMaximum5Async(page.Offset, page.Limit, title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, reportingAcademicPeriod, academicLevels, course, site, status, owningOrganizations, instructors, scheduleAcademicPeriod, bypassCache);
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.SectionMaximum5>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A SectionMaximum object <see cref="Dtos.SectionMaximum"/> in HeDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections/{id}", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetHedmSectionMaximumByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionMaximum2>> GetHedmSectionMaximumByGuid2Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddDataPrivacyContextProperty((await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                new List<string>() { id }));
                return await _sectionCoordinationService.GetSectionMaximumByGuid2Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A SectionMaximum object <see cref="Dtos.SectionMaximum3"/> in HeDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections/{id}", 8, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetHedmSectionMaximumByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionMaximum3>> GetHedmSectionMaximumByGuid3Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddDataPrivacyContextProperty((await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                new List<string>() { id }));
                return await _sectionCoordinationService.GetSectionMaximumByGuid3Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A SectionMaximum object <see cref="Dtos.SectionMaximum4"/> in HeDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections/{id}", "11", false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetHedmSectionMaximumByGuidV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionMaximum4>> GetHedmSectionMaximumByGuid4Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                new List<string>() { id }));
                return await _sectionCoordinationService.GetSectionMaximumByGuid4Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A SectionMaximum object <see cref="Dtos.SectionMaximum5"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections/{id}", "16.0.0", false, RouteConstants.HedtechIntegrationSectionsMaximumMediaTypeFormat, Name = "GetHedmSectionMaximumByGuidV16.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionMaximum5>> GetHedmSectionMaximumByGuid5Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddDataPrivacyContextProperty((await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                new List<string>() { id }));
                return await _sectionCoordinationService.GetSectionMaximumByGuid5Async(id, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new SectionMaximum
        /// </summary>
        /// <param name="sectionMaximum">DTO of the new section</param>
        /// <returns>A SectionMaximum object <see cref="Dtos.SectionMaximum"/> in HeDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/sections", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PostHedmSectionMaximumV6")]
        [HeaderVersionRoute("/sections", 8, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PostHedmSectionMaximumV8")]
        [HeaderVersionRoute("/sections", "11", false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PostHedmSectionMaximumV11")]
        [HeaderVersionRoute("/sections", "16.0.0", false, RouteConstants.HedtechIntegrationSectionsMaximumMediaTypeFormat, Name = "PostHedmSectionMaximumV16.0.0")]
        public async Task<ActionResult<Dtos.SectionMaximum>> PostHedmSectionMaximumAsync(Dtos.SectionMaximum sectionMaximum)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="id">GUID of the section to update</param>
        /// <param name="sectionMaximum">DTO of the updated section</param>
        /// <returns>A SectionMaximum object <see cref="Dtos.SectionMaximum"/> in HeDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/sections/{id}", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PutHedmSectionMaximumV6")]
        [HeaderVersionRoute("/sections/{id}", 8, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PutHedmSectionMaximumV8")]
        [HeaderVersionRoute("/sections/{id}", "11", false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PutHedmSectionMaximumV11")]
        [HeaderVersionRoute("/sections/{id}", "16.0.0", false, RouteConstants.HedtechIntegrationSectionsMaximumMediaTypeFormat, Name = "PutHedmSectionMaximumV16.0.0")]
        public async Task<ActionResult<Dtos.SectionMaximum>> PutHedmSectionMaximumAsync([FromRoute] string id, [FromBody] Dtos.SectionMaximum sectionMaximum)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a section
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section"/> in HeDM format</returns>
        [HttpDelete]
        [HeaderVersionRoute("/sections/{id}", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "DeleteHedmSectionMaximumV6")]
        [HeaderVersionRoute("/sections/{id}", 8, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "DeleteHedmSectionMaximumV8")]
        [HeaderVersionRoute("/sections/{id}", "11", false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "DeleteHedmSectionMaximumV11")]
        [HeaderVersionRoute("/sections/{id}", "16.0.0", false, RouteConstants.HedtechIntegrationSectionsMaximumMediaTypeFormat, Name = "DeleteHedmSectionMaximumV16.0.0")]
        public async Task<IActionResult> DeleteHedmSectionMaximumByGuidAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        
        #endregion

    }
}
