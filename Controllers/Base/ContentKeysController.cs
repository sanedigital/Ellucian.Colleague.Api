// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Content Key data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ContentKeysController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IContentKeyService _contentKeyService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="contentKeyService">Service of type <see cref="IContentKeyService">IContentKeyService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ContentKeysController(IAdapterRegistry adapterRegistry, IContentKeyService contentKeyService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _contentKeyService = contentKeyService;
            this._logger = logger;
        }

        /// <summary>
        /// Get a Content Key
        /// </summary>
        /// <param name="id">The encryption key ID to use to encrypt the content key</param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/content-keys/{id}", 1, true, Name = "GetContentKey")]
        public async Task<ActionResult<ContentKey>> GetContentKeyAsync(string id)
        {
            try
            {
                return await _contentKeyService.GetContentKeyAsync(id);
            }
            catch (KeyNotFoundException knfe)
            {
                return CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Post an encrypted content key to have it decrypted
        /// </summary>
        /// <param name="contentKeyRequest">The <see cref="ContentKeyRequest">Content Key Request</see></param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/content-keys", 1, true, Name = "PostContentKey")]
        public async Task<ActionResult<ContentKey>> PostContentKeyAsync(ContentKeyRequest contentKeyRequest)
        {
            try
            {
                return await _contentKeyService.PostContentKeyAsync(contentKeyRequest);
            }
            catch (KeyNotFoundException knfe)
            {
                return CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}
