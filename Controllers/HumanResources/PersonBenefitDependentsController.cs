// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PersonBenefitDependents
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonBenefitDependentsController : BaseCompressedApiController
    {
        private readonly IPersonBenefitDependentsService _personBenefitDependentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonBenefitDependentsController class.
        /// </summary>
        /// <param name="personBenefitDependentsService">Service of type <see cref="IPersonBenefitDependentsService">IPersonBenefitDependentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonBenefitDependentsController(IPersonBenefitDependentsService personBenefitDependentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personBenefitDependentsService = personBenefitDependentsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all personBenefitDependents
        /// </summary>
        /// <returns>List of PersonBenefitDependents <see cref="Dtos.PersonBenefitDependents"/> objects representing matching personBenefitDependents</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/person-benefit-dependents", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonBenefitDependents", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonBenefitDependentsAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                AddDataPrivacyContextProperty((await _personBenefitDependentsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var pageOfItems = await _personBenefitDependentsService.GetPersonBenefitDependentsAsync(page.Offset, page.Limit, bypassCache);
                return new PagedActionResult<IEnumerable<Dtos.PersonBenefitDependents>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Read (GET) a personBenefitDependents using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personBenefitDependents</param>
        /// <returns>A personBenefitDependents object <see cref="Dtos.PersonBenefitDependents"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/person-benefit-dependents/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonBenefitDependentsByGuid")]
        public async Task<ActionResult<Dtos.PersonBenefitDependents>> GetPersonBenefitDependentsByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _personBenefitDependentsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _personBenefitDependentsService.GetPersonBenefitDependentsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Create (POST) a new personBenefitDependents
        /// </summary>
        /// <param name="personBenefitDependents">DTO of the new personBenefitDependents</param>
        /// <returns>A personBenefitDependents object <see cref="Dtos.PersonBenefitDependents"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/person-benefit-dependents", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonBenefitDependentsV11")]
        public async Task<ActionResult<Dtos.PersonBenefitDependents>> PostPersonBenefitDependentsAsync([FromBody] Dtos.PersonBenefitDependents personBenefitDependents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personBenefitDependents
        /// </summary>
        /// <param name="guid">GUID of the personBenefitDependents to update</param>
        /// <param name="personBenefitDependents">DTO of the updated personBenefitDependents</param>
        /// <returns>A personBenefitDependents object <see cref="Dtos.PersonBenefitDependents"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/person-benefit-dependents/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonBenefitDependentsV11")]
        public async Task<ActionResult<Dtos.PersonBenefitDependents>> PutPersonBenefitDependentsAsync([FromQuery] string guid, [FromBody] Dtos.PersonBenefitDependents personBenefitDependents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personBenefitDependents
        /// </summary>
        /// <param name="guid">GUID to desired personBenefitDependents</param>
        [HttpDelete]
        public async Task<IActionResult> DeletePersonBenefitDependentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
