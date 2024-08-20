// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to course Section data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionCrosslistsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly ISectionCrosslistService _sectionCrosslistService;

        /// <summary>
        /// Initializes a new instance of the SectionsController class.
        /// </summary>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="sectionCrosslist">SectionCrosslist Service <see cref="ISectionCrosslistService">ISectionCrosslistService</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionCrosslistsController(ILogger logger, ISectionCrosslistService sectionCrosslist, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
            _sectionCrosslistService = sectionCrosslist;
        }


        /// <summary>
        /// Read (GET) all SectionCrosslists or all SectionCrosslists with section selected in filter
        /// </summary>
        /// <param name="section">GUID to desired Section to filter SectionCrosslists by</param>
        /// <param name="page">paging data from the url</param>
        /// <returns>A List SectionCrosslist object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter(new string[] { "section" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 500 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-crosslists", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetDataModelSectionCrosslists", IsEedmSupported = true)]
        public async Task<IActionResult> GetDataModelSectionCrosslistsAsync(Paging page, [FromQuery] string section = "")
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (section == null || section == "null")
            {
                return new PagedActionResult<IEnumerable<Dtos.SectionCrosslist>>(new List<Dtos.SectionCrosslist>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(500, 0);
                }

                var pageOfItems = await _sectionCrosslistService.GetDataModelSectionCrosslistsPageAsync(page.Offset, page.Limit, section);

                AddEthosContextProperties(await _sectionCrosslistService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.SectionCrosslist>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a SectionCrosslist using a GUID
        /// </summary>
        /// <param name="id">GUID to desired SectionCrosslist</param>
        /// <returns>A SectionCrosslist object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-crosslists/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetDataModelSectionCrosslistsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionCrosslist>> GetDataModelSectionCrosslistsByGuidAsync(string id)
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
                var crossslist = await _sectionCrosslistService.GetDataModelSectionCrosslistsByGuidAsync(id);

                if (crossslist != null)
                {

                    AddEthosContextProperties((await _sectionCrosslistService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList(),
                              await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { crossslist.Id }));
                }


                return crossslist;

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
            catch (KeyNotFoundException	e)
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
        /// Create (POST) a new SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist">DTO of the new SectionCrosslist</param>
        /// <returns>A section object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPost]
        [HeaderVersionRoute("/section-crosslists", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostDataModelSectionCrosslistsV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionCrosslist>> PostDataModelSectionCrosslistsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionCrosslist sectionCrosslist)
        {
            if (sectionCrosslist == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null sectioncrosslist argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(sectionCrosslist.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null sectioncrosslist id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            var validateResult = ValidateSectionCrosslist(sectionCrosslist);
            if (validateResult != null)
            {
                return Ok(validateResult);
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _sectionCrosslistService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCrosslistService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section crosslist
                var sectionCrosslistCreate = await _sectionCrosslistService.CreateDataModelSectionCrosslistsAsync(sectionCrosslist);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCrosslistService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionCrosslistCreate.Id }));

                return sectionCrosslistCreate;
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing SectionCrosslist
        /// </summary>
        /// <param name="id">GUID of the SectionCrosslist to update</param>
        /// <param name="sectionCrosslist">DTO of the updated SectionCrosslist</param>
        /// <returns>A section object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/section-crosslists/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutDataModelSectionCrosslistsV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionCrosslist>> PutDataModelSectionCrosslistsAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionCrosslist sectionCrosslist)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (sectionCrosslist == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null sectioncrosslist argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(sectionCrosslist.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                   IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }
            else if (id.ToLowerInvariant() != sectionCrosslist.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {             
                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCrosslistService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCrosslistService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                
                //get Data Privacy List
                var dpList = await _sectionCrosslistService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //do update with partial logic
                var sectionCrosslistReturn = await _sectionCrosslistService.UpdateDataModelSectionCrosslistsAsync(
                    await PerformPartialPayloadMerge(sectionCrosslist, async () => await _sectionCrosslistService.GetDataModelSectionCrosslistsByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return sectionCrosslistReturn; 
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a SectionCrosslist
        /// </summary>
        /// <param name="id">GUID to desired SectionCrosslist</param>
        /// <returns>A section object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpDelete]
        [Route("/section-crosslists/{id}", Name = "DefaultDeleteDataModelSectionCrosslists", Order = -10)]
        public async Task<IActionResult> DeleteDataModelSectionCrosslistsByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                await _sectionCrosslistService.DeleteDataModelSectionCrosslistsByGuidAsync(id);
                return NoContent();
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Validates the data in the SectionCrosslist object
        /// </summary>
        /// <param name="sectionCrosslist">SectoinCrosslist from the request</param>
        private IActionResult ValidateSectionCrosslist(SectionCrosslist sectionCrosslist)
        {
            if (sectionCrosslist.Sections == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null sections argument",
                    IntegrationApiUtility.GetDefaultApiError("The sections list is required.")));
            }

            if (sectionCrosslist.Sections.Count <= 1)
            {
                return CreateHttpResponseException(new IntegrationApiException("Missing sections argument",
                    IntegrationApiUtility.GetDefaultApiError("The sections list must have at least two sections.")));
            }

            if (sectionCrosslist.Sections.Count > sectionCrosslist.Sections.Select(s => s.Section.Id).ToList().Distinct().ToList().Count)
            {
                return CreateHttpResponseException(new IntegrationApiException("Repeating section ids",
                   IntegrationApiUtility.GetDefaultApiError("The sections list must contain unique sections, section ids cannot repeat.")));
            }

            if (!sectionCrosslist.Sections.Any(s => s.Type == SectionTypeForCrosslist.Primary))
            {
                return CreateHttpResponseException(new IntegrationApiException("Missing primary section argument",
                    IntegrationApiUtility.GetDefaultApiError("The sections list must have at least one section marked as primary.")));
            }

            if (sectionCrosslist.Sections.Where(s => s.Type == SectionTypeForCrosslist.Primary).ToList().Count > 1)
            {
                return CreateHttpResponseException(new IntegrationApiException("To many primary sections",
                    IntegrationApiUtility.GetDefaultApiError("The sections list may only have one section marked as primary.")));
            }

            return null;
        }
    }
}
