// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to MiscellaneousText data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class MiscellaneousTextController : BaseCompressedApiController
    {
        private readonly IMiscellaneousTextService _miscellaneousTextService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MiscellaneousTextController class.
        /// </summary>
        /// <param name="miscellaneousTextService">Service of type <see cref="IMiscellaneousTextService">IMiscellaneousTextService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MiscellaneousTextController(IMiscellaneousTextService miscellaneousTextService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _miscellaneousTextService = miscellaneousTextService;
            this._logger = logger;
        }

        /// <summary>
        /// Gets the Miscellaneous Text records
        /// </summary>
        /// <returns>IEnumerable of <see cref="MiscellaneousText">Miscellaneous Text Collection</see></returns>
        /// <note>IEnumerable--MiscellaneousText-- is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/miscellaneous-text", 1, true, Name = "GetAllMiscellaneousText")]
        public async Task<ActionResult<IEnumerable<MiscellaneousText>>> GetAsync()
        {
            try
            {
                return Ok(await _miscellaneousTextService.GetAllMiscellaneousTextAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while retrieving Miscellaneous Text records: ", ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
