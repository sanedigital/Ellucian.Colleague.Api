// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonNameType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonNameTypesController : BaseCompressedApiController
    {
        private readonly IPersonNameTypeService _personNameTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonNameTypesController class.
        /// </summary>
        /// <param name="personNameTypeService">Service of type<see cref="IPersonNameTypeService"> IPersonNameTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>PersonNameTypes
        /// <param name="apiSettings"></param>
        public PersonNameTypesController(IPersonNameTypeService personNameTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
           _personNameTypeService = personNameTypeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all person name types.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.PersonNameTypeItem">PersonNameTypeItem</see> objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/person-name-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonNameTypesV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PersonNameTypeItem>>> GetPersonNameTypesAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                return Ok(await _personNameTypeService.GetPersonNameTypesAsync(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all person name types.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.PersonNameTypeItem">PersonNameTypeItem</see> objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/person-name-types", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonNameTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PersonNameTypeItem>>> GetPersonNameTypes2Async()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                return Ok(await _personNameTypeService.GetPersonNameTypes2Async(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an person name type by ID.
        /// </summary>
        /// <param name="id">Unique ID representing the person name type to get</param>
        /// <returns>An <see cref="Dtos.PersonNameTypeItem">PersonNameTypeItem</see> object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/person-name-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonNameTypeByIdV6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonNameTypeItem>> GetPersonNameTypeByIdAsync(string id)
        {
            try
            {
                return await _personNameTypeService.GetPersonNameTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an person name type by ID.
        /// </summary>
        /// <param name="id">Unique ID representing the person name type to get</param>
        /// <returns>An <see cref="Dtos.PersonNameTypeItem">PersonNameTypeItem</see> object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/person-name-types/{id}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonNameTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonNameTypeItem>> GetPersonNameTypeById2Async(string id)
        {
            try
            {
                return Ok(await _personNameTypeService.GetPersonNameTypeById2Async(id));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing person name type in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the person name type to delete</param>
        [HttpDelete]
        [Route("/person-name-types/{id}", Name = "DeletePersonNameTypes", Order = -10)]
        public IActionResult DeletePersonNameTypes(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Person Name Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="personNameTypeItem"><see cref="Dtos.PersonNameTypeItem">PersonNameTypeItem</see> to update</param>
        [HttpPut]
        [HeaderVersionRoute("/person-name-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonNameTypesV6")]
        [HeaderVersionRoute("/person-name-types/{id}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonNameTypesV12")]
        public ActionResult<Ellucian.Colleague.Dtos.PersonNameTypeItem> PutPersonNameTypes([FromBody] Ellucian.Colleague.Dtos.PersonNameTypeItem personNameTypeItem)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create a Person Name Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="personNameTypeItem"><see cref="Dtos.PersonNameTypeItem">PersonNameTypeItem</see> to create</param>
        [HttpPost]
        [HeaderVersionRoute("/person-name-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonNameTypesV6")]
        [HeaderVersionRoute("/person-name-types", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonNameTypesV12")]
        public ActionResult<Ellucian.Colleague.Dtos.PersonNameTypeItem> PostPersonNameTypes([FromBody] Ellucian.Colleague.Dtos.PersonNameTypeItem personNameTypeItem)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
