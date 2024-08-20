// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Links Controller is used to get links for the Financial Aid Homepage
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FALinkController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IFALinkRepository FaLinkRepository;

        /// <summary>
        /// FA Link Controller constructor
        /// </summary>
        /// <param name="faLinkRepository">FA Link repository</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FALinkController(ILogger logger, IFALinkRepository faLinkRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            FaLinkRepository = faLinkRepository;

        }

        /// <summary>
        /// Send the input FA Link document to Colleague to be processed by Trimdata's CTX/subroutines.
        /// </summary>
        /// <param name="inputDocument">FA Link input Document</param>
        /// <returns>Output FA Link document</returns>
        [HttpPost]
        [HeaderVersionRoute("/fa-link", 1, true, Name = "FALink")]
        public async Task<ActionResult<FALinkDocument>> PostAsync([FromBody]FALinkDocument inputDocument)
        {
            try
            {
                string outputDocString = await FaLinkRepository.PostFALinkDocumentAsync(inputDocument.Document.ToString());
                JObject outputDoc = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(outputDocString);
                FALinkDocument returnDto = new FALinkDocument();
                returnDto.Document = outputDoc;
                return returnDto;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred processing FA Link document.");
            }
        }
    }
}
