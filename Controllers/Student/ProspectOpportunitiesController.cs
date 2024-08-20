// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to ProspectOpportunities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ProspectOpportunitiesController : BaseCompressedApiController
    {
        private readonly IProspectOpportunitiesService _prospectOpportunitiesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ProspectOpportunitiesController class.
        /// </summary>
        /// <param name="prospectOpportunitiesService">Service of type <see cref="IProspectOpportunitiesService">IProspectOpportunitiesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProspectOpportunitiesController(IProspectOpportunitiesService prospectOpportunitiesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _prospectOpportunitiesService = prospectOpportunitiesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all prospectOpportunities
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">JSON formatted selection criteria.</param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters.</param>
        /// <returns>List of ProspectOpportunities <see cref="Dtos.ProspectOpportunities"/> objects representing matching prospectOpportunities</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewProspectOpportunity, StudentPermissionCodes.UpdateProspectOpportunity })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.ProspectOpportunities))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/prospect-opportunities", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetProspectOpportunities", IsEedmSupported = true)]
        public async Task<IActionResult> GetProspectOpportunitiesAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _prospectOpportunitiesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var criteriaObject = GetFilterObject<Dtos.ProspectOpportunities>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.ProspectOpportunities>>(new List<Dtos.ProspectOpportunities>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(page.Offset, page.Limit, criteriaObject, personFilterValue, bypassCache);

                AddEthosContextProperties(
                  await _prospectOpportunitiesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _prospectOpportunitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.ProspectOpportunities>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a prospectOpportunities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired prospectOpportunities</param>
        /// <returns>A prospectOpportunities object <see cref="Dtos.ProspectOpportunities"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewProspectOpportunity, StudentPermissionCodes.UpdateProspectOpportunity })]
        [HeaderVersionRoute("/prospect-opportunities/{guid}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetProspectOpportunitiesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ProspectOpportunities>> GetProspectOpportunitiesByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _prospectOpportunitiesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _prospectOpportunitiesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _prospectOpportunitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _prospectOpportunitiesService.GetProspectOpportunitiesByGuidAsync(guid, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new prospectOpportunities
        /// </summary>
        /// <param name="prospectOpportunities">DTO of the new prospectOpportunities</param>
        /// <returns>A prospectOpportunities object <see cref="Dtos.ProspectOpportunities"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/prospect-opportunities", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostProspectOpportunitiesVema")]
        public async Task<ActionResult<Dtos.ProspectOpportunities>> PostProspectOpportunitiesAsync([FromBody] Dtos.ProspectOpportunities prospectOpportunities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing prospectOpportunities
        /// </summary>
        /// <param name="guid">GUID of the prospectOpportunities to update</param>
        /// <param name="prospectOpportunities">DTO of the updated prospectOpportunities</param>
        /// <returns>A prospectOpportunities object <see cref="Dtos.ProspectOpportunities"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/prospect-opportunities/{guid}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutProspectOpportunitiesVema")]
        public async Task<ActionResult<Dtos.ProspectOpportunities>> PutProspectOpportunitiesAsync([FromRoute] string guid, [FromBody] Dtos.ProspectOpportunities prospectOpportunities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #region submissions

        /// <summary>
        /// Create (POST) a new prospectOpportunities
        /// </summary>
        /// <param name="prospectOpportunities">DTO of the new prospectOpportunities</param>
        /// <returns>A prospectOpportunities object <see cref="Dtos.ProspectOpportunities"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationProspectOpportunitiesSubmissionsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/prospect-opportunities", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPostProspectOpportunitiesSubmissionsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.ProspectOpportunities>> PostProspectOpportunitiesSubmissionsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.ProspectOpportunitiesSubmissions prospectOpportunities)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (prospectOpportunities == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null prospectOpportunities argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(prospectOpportunities.Id) || !prospectOpportunities.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                var dpList = await _prospectOpportunitiesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);
                await _prospectOpportunitiesService.ImportExtendedEthosData(await ExtractExtendedData(await _prospectOpportunitiesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var prospectOpportunitiesReturn = await _prospectOpportunitiesService.CreateProspectOpportunitiesSubmissionsAsync(prospectOpportunities, bypassCache);

                AddEthosContextProperties(dpList,
                    await _prospectOpportunitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { prospectOpportunitiesReturn.Id }));

                return prospectOpportunitiesReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }

        }

        /// <summary>
        /// Update (PUT) an existing prospectOpportunities
        /// </summary>
        /// <param name="guid">GUID of the prospectOpportunities to update</param>
        /// <param name="prospectOpportunities">DTO of the updated prospectOpportunities</param>
        /// <returns>A prospectOpportunities object <see cref="Dtos.ProspectOpportunities"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationProspectOpportunitiesSubmissionsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/prospect-opportunities/{guid}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPutProspectOpportunitiesSubmissionsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.ProspectOpportunities>> PutProspectOpportunitiesSubmissionsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.ProspectOpportunitiesSubmissions prospectOpportunities)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (prospectOpportunities == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null prospectOpportunities argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(prospectOpportunities.Id))
            {
                prospectOpportunities.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, prospectOpportunities.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                var dpList = await _prospectOpportunitiesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                await _prospectOpportunitiesService.ImportExtendedEthosData(await ExtractExtendedData(await _prospectOpportunitiesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var prospectOpportunitiesReturn = await _prospectOpportunitiesService.UpdateProspectOpportunitiesSubmissionsAsync(
                  await PerformPartialPayloadMerge(prospectOpportunities, async () => await _prospectOpportunitiesService.GetProspectOpportunitiesSubmissionsByGuidAsync(guid, true),
                  dpList, _logger), bypassCache);

                AddEthosContextProperties(dpList,
                    await _prospectOpportunitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return prospectOpportunitiesReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }

        }

        #endregion

        /// <summary>
        /// Delete (DELETE) a prospectOpportunities
        /// </summary>
        /// <param name="guid">GUID to desired prospectOpportunities</param>
        [HttpDelete]
        [Route("/prospect-opportunities/{guid}", Name = "DefaultDeleteProspectOpportunities", Order = -10)]
        public async Task<IActionResult> DeleteProspectOpportunitiesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
