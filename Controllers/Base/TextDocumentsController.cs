// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class TextDocumentsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IDocumentService _documentService;

        /// <summary>
        /// Initializes a new instance of the AddressesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="documentService">Service of type <see cref="IDocumentService">IDocumentService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TextDocumentsController(IAdapterRegistry adapterRegistry, IDocumentService documentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _documentService = documentService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a text document
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <returns>A text document</returns>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "documentId" })]
        [HttpGet]
        [HeaderVersionRoute("/text-documents/{documentId}", 1, true, Name = "GetTextDocumentAsync")]
        public async Task<ActionResult<TextDocument>> GetAsync([FromRoute]string documentId, [FromQuery]string primaryEntity, [FromQuery] string primaryId, [FromQuery] string personId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                string message = "Text document ID cannot be null.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(primaryEntity))
            {
                string message = "Primary entity cannot be null.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(primaryId))
            {
                string message = "Primary record ID cannot be null.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _documentService.GetTextDocumentAsync(documentId, primaryEntity, primaryId, personId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

    }
}
