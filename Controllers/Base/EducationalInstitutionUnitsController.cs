// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using System.Net;
using Ellucian.Web.Http.Filters;
using System.Linq;
using Newtonsoft.Json.Linq;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Educational Institution Units
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EducationalInstitutionUnitsController : BaseCompressedApiController
    {
        private readonly IEducationalInstitutionUnitsService _educationalInstitutionUnitsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EducationalInstitutionUnitsController class.
        /// </summary>
        /// <param name="educationalInstitutionUnitsService">Service of type <see cref="IEducationalInstitutionUnitsService">IEducationalInstitutionUnitsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EducationalInstitutionUnitsController(IEducationalInstitutionUnitsService educationalInstitutionUnitsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _educationalInstitutionUnitsService = educationalInstitutionUnitsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all educationalInstitutionUnits
        /// </summary>      
        ///  <param name="criteria">JSON formatted selection criteria.</param>
        /// <param name="department">Indicates whether a department is active or inactive</param>
        /// <param name="type">Old Filter on type = department, school, division</param>
        /// <returns>List of EducationalInstitutionUnits <see cref="Dtos.EducationalInstitutionUnits3"/> objects representing matching educationalInstitutionUnits</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.EducationalInstitutionUnits3))]
        [QueryStringFilterFilter("type", typeof(Dtos.EducationalInstitutionUnits3))]
        [QueryStringFilterFilter("department", typeof(Dtos.Filters.DepartmentFilter))]
        [HttpGet]
        [HeaderVersionRoute("/educational-institution-units", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionUnitsDefault", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.EducationalInstitutionUnits3>>> GetEducationalInstitutionUnits3Async(QueryStringFilter criteria,
            QueryStringFilter department, [FromQuery] string type = "")
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaObject = GetFilterObject<Dtos.EducationalInstitutionUnits3>(_logger, "criteria");
            var departmentObject = GetFilterObject<Dtos.Filters.DepartmentFilter>(_logger, "department");

            if (CheckForEmptyFilterParameters())
                return new List<Dtos.EducationalInstitutionUnits3>();

            var educationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet;
            if (criteriaObject != null && criteriaObject.Type != Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet)
                educationalInstitutionUnitType = criteriaObject.Type;

            var departmentStatus = Dtos.EnumProperties.Status.NotSet;
            if (departmentObject != null && departmentObject.department != null && departmentObject.department.status != Dtos.EnumProperties.Status.NotSet)
                departmentStatus = departmentObject.department.status;

            try
            {
                if (!string.IsNullOrEmpty(type))
                {
                    switch (type.ToLower())
                    {
                        case "department":
                            educationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Department;
                            break;
                        case "division":
                            educationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Division;
                            break;
                        case "school":
                            educationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.School;
                            break;
                        case "college":
                            return new List<Dtos.EducationalInstitutionUnits3>();
                        case "institute":
                            return new List<Dtos.EducationalInstitutionUnits3>();
                        case "facility":
                            return new List<Dtos.EducationalInstitutionUnits3>();
                        default:
                            throw new ArgumentException("Invalid filter value provided.");
                    }
                }
                var instUnits = await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits3Async(bypassCache, educationalInstitutionUnitType, departmentStatus);

                if (instUnits != null)
                {

                    AddEthosContextProperties(await _educationalInstitutionUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _educationalInstitutionUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              instUnits.Select(iu => iu.Id).ToList()));
                }
                return Ok(instUnits);
                
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
        /// Return Educational-Institution-Unit using type filter
        /// </summary>
        /// <param name="type">Type of Educational-Institution-Unit ex:"school", "division", "department"</param>
        /// <returns>List of EducationalInstitutionUnits <see cref="Dtos.EducationalInstitutionUnits"/> objects representing matching educationalInstitutionUnits</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("type", typeof(Dtos.EducationalInstitutionUnits2), IgnoreFilters = true)]
        [ValidateQueryStringFilter(new string[] { "type" }, false, true)]
        [HttpGet]
        [HeaderVersionRoute("/educational-institution-units", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionUnitsV7", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.EducationalInstitutionUnits2>>> GetEducationalInstitutionUnits2Async([FromQuery] string type = "")
        {
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
                var instUnits = await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByType2Async(type, bypassCache);

                if (instUnits != null)
                {

                    AddEthosContextProperties(await _educationalInstitutionUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _educationalInstitutionUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              instUnits.Select(iu => iu.Id).ToList()));
                }
                return Ok(instUnits);

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
        /// Return Educational-Institution-Unit using type filter
        /// </summary>
        /// <param name="type">Type of Educational-Institution-Unit ex:"school", "division", "department"</param>
        /// <returns>List of EducationalInstitutionUnits <see cref="Dtos.EducationalInstitutionUnits"/> objects representing matching educationalInstitutionUnits</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("type", typeof(Dtos.EducationalInstitutionUnits), IgnoreFilters = true)]
        [ValidateQueryStringFilter(new string[] { "type" }, false, true)]
        [HttpGet]
        [HeaderVersionRoute("/educational-institution-units", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionUnitsV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.EducationalInstitutionUnits>>> GetEducationalInstitutionUnitsAsync([FromQuery] string type = "")
        { 
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
                var instUnits = await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByTypeAsync(type, bypassCache);

                if (instUnits != null)
                {
                    AddEthosContextProperties(await _educationalInstitutionUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _educationalInstitutionUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              instUnits.Select(iu => iu.Id).ToList()));
                }
                return Ok(instUnits);
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
        /// Read (GET) an Educational-Institution-Unit using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired educationalInstitutionUnits</param>
        /// <returns>An EducationalInstitutionUnits object <see cref="Dtos.EducationalInstitutionUnits3"/> in HEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/educational-institution-units/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionUnitsByGuidDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EducationalInstitutionUnits3>> GetEducationalInstitutionUnitsByGuid3Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
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

                var instUnit = await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(guid, bypassCache);

                if (instUnit != null)
                {
                    AddEthosContextProperties(await _educationalInstitutionUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _educationalInstitutionUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { instUnit.Id }));
                }
                return instUnit;
                
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Read (GET) an Educational-Institution-Unit using a GUID
        /// </summary>
        /// <param name="id">GUID to desired educationalInstitutionUnits</param>
        /// <returns>An EducationalInstitutionUnits object <see cref="Dtos.EducationalInstitutionUnits"/> in HEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/educational-institution-units/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionUnitsByGuidV7", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EducationalInstitutionUnits2>> GetEducationalInstitutionUnitsByGuid2Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
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
                var instUnit = await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(id);

                if (instUnit != null)
                {
                    AddEthosContextProperties(await _educationalInstitutionUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _educationalInstitutionUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { instUnit.Id }));
                }
                return instUnit;
                
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Read (GET) an Educational-Institution-Unit using a GUID
        /// </summary>
        /// <param name="id">GUID to desired educationalInstitutionUnits</param>
        /// <returns>An EducationalInstitutionUnits object <see cref="Dtos.EducationalInstitutionUnits"/> in HEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/educational-institution-units/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionUnitsByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EducationalInstitutionUnits>> GetEducationalInstitutionUnitsByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
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
                var instUnit = await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(id);

                if (instUnit != null)
                {
                    AddEthosContextProperties(await _educationalInstitutionUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _educationalInstitutionUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { instUnit.Id }));
                }
                return instUnit;
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
        /// Create (POST) a new Educational-Institution-Unit
        /// </summary>
        /// <param name="educationalInstitutionUnits">DTO of the new educationalInstitutionUnits</param>
        /// <returns>A educationalInstitutionUnits object <see cref="Dtos.EducationalInstitutionUnits"/> in HEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/educational-institution-units", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEducationalInstitutionUnitsV12")]
        [HeaderVersionRoute("/educational-institution-units", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEducationalInstitutionUnitsV7")]
        [HeaderVersionRoute("/educational-institution-units", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEducationalInstitutionUnitsV6")]
        public async Task<ActionResult<Dtos.EducationalInstitutionUnits>> PostEducationalInstitutionUnitsAsync([FromBody] Dtos.EducationalInstitutionUnits educationalInstitutionUnits)
        {
            //Post is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
    
        }

        /// <summary>
        /// Update (PUT) an existing Educational-Institution-Unit
        /// </summary>
        /// <param name="id">GUID of the EducationalInstitutionUnits to update</param>
        /// <param name="educationalInstitutionUnits">DTO of the updated EducationalInstitutionUnits</param>
        /// <returns>A EducationalInstitutionUnits object <see cref="Dtos.EducationalInstitutionUnits"/> in HEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/educational-institution-units/{id}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEducationalInstitutionUnitsV12")]
        [HeaderVersionRoute("/educational-institution-units/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEducationalInstitutionUnitsV7")]
        [HeaderVersionRoute("/educational-institution-units/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEducationalInstitutionUnitsV6")]
        public async Task<ActionResult<Dtos.EducationalInstitutionUnits>> PutEducationalInstitutionUnitsAsync([FromRoute] string id, [FromBody] Dtos.EducationalInstitutionUnits educationalInstitutionUnits)
        {
            //Put is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
    
        }

        /// <summary>
        /// Delete (DELETE) a Educational-Institution-Unit
        /// </summary>
        /// <param name="id">GUID to desired EducationalInstitutionUnits</param>
        [HttpDelete]
        [Route("/educational-institution-units/{id}", Name = "DeleteEducationalInstitutionUnitsByGuid", Order = -10)]
        public async Task<IActionResult> DeleteEducationalInstitutionUnitsByGuidAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
    
        }
    }
}
