// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Net;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonalPronounType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalPronounTypesController : BaseCompressedApiController
    {
        private readonly IPersonalPronounTypeService _personalPronounTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonalPronounTypesController class.
        /// </summary>
        /// <param name="personalPronounTypeService">Service of type <see cref="IPersonalPronounTypeService">IPersonalPronounTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonalPronounTypesController(IPersonalPronounTypeService personalPronounTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personalPronounTypeService = personalPronounTypeService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves personal pronoun types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.PersonalPronounType">PersonalPronounType</see> objects></returns>
        /// <note>This request supports anonymous access. The PERSONAL.PRONOUNS (CORE.VALCODES) valcode in Colleague must have public access enabled for this API to function anonymously. See :ref:`anonymousapis` for additional information.</note>
        [HttpGet]
        [HeaderVersionRoute("/personal-pronoun-types", 1, true, Name = "GetPersonalPronounTypes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>>> GetAsync()
        {
            try
            {
                bool ignoreCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        ignoreCache = true;
                    }
                }
                return Ok(await _personalPronounTypeService.GetBasePersonalPronounTypesAsync(ignoreCache));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving personal pronoun types";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Return all personalPronouns
        /// </summary>
        /// <returns>List of PersonalPronouns <see cref="Dtos.PersonalPronouns"/> objects representing matching personalPronouns</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/personal-pronouns", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonalPronouns", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PersonalPronouns>>> GetPersonalPronounsAsync()
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
                var personalPronouns = await _personalPronounTypeService.GetPersonalPronounsAsync(bypassCache);

                if (personalPronouns != null && personalPronouns.Any())
                {
                    AddEthosContextProperties(await _personalPronounTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _personalPronounTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personalPronouns.Select(a => a.Id).ToList()));
                }
                return Ok(personalPronouns);
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
        /// Read (GET) a personalPronouns using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personalPronouns</param>
        /// <returns>A personalPronouns object <see cref="Dtos.PersonalPronouns"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/personal-pronouns/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonalPronounsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonalPronouns>> GetPersonalPronounsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _personalPronounTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personalPronounTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personalPronounTypeService.GetPersonalPronounsByGuidAsync(guid);
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
        /// Create (POST) a new personalPronouns
        /// </summary>
        /// <param name="personalPronouns">DTO of the new personalPronouns</param>
        /// <returns>A personalPronouns object <see cref="Dtos.PersonalPronouns"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/personal-pronouns", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonalPronounsV1.0.0")]
        public async Task<ActionResult<Dtos.PersonalPronouns>> PostPersonalPronounsAsync([FromBody] Dtos.PersonalPronouns personalPronouns)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personalPronouns
        /// </summary>
        /// <param name="guid">GUID of the personalPronouns to update</param>
        /// <param name="personalPronouns">DTO of the updated personalPronouns</param>
        /// <returns>A personalPronouns object <see cref="Dtos.PersonalPronouns"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/personal-pronouns/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonalPronounsV1.0.0")]
        public async Task<ActionResult<Dtos.PersonalPronouns>> PutPersonalPronounsAsync([FromRoute] string guid, [FromBody] Dtos.PersonalPronouns personalPronouns)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personalPronouns
        /// </summary>
        /// <param name="guid">GUID to desired personalPronouns</param>
        [HttpDelete]
        [Route("/personal-pronouns/{guid}", Name = "DefaultDeletePersonalPronouns", Order = -10)]
        public async Task<IActionResult> DeletePersonalPronounsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
