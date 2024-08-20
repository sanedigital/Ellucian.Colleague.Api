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
using Ellucian.Colleague.Dtos.Student;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to InstructionalDeliveryMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructionalDeliveryMethodsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructionalDeliveryMethodsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstructionalDeliveryMethodsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all instructional-delivery-methods
        /// </summary>
        /// <returns>All <see cref="InstructionalDeliveryMethod">InstructionalDeliveryMethods</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/instructional-delivery-methods", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructionalDeliveryMethods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<InstructionalDeliveryMethod>>> GetInstructionalDeliveryMethodsAsync()
        {
            return new List<InstructionalDeliveryMethod>();
        }

        /// <summary>
        /// Retrieve (GET) an existing instructional-delivery-methods
        /// </summary>
        /// <param name="guid">GUID of the instructional-delivery-methods to get</param>
        /// <returns>A InstructionalDeliveryMethods object <see cref="InstructionalDeliveryMethod"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/instructional-delivery-methods/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructionalDeliveryMethodsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<InstructionalDeliveryMethod>> GetInstructionalDeliveryMethodByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No instructional-delivery-methods was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new InstructionalDeliveryMethods
        /// </summary>
        /// <param name="InstructionalDeliveryMethod">DTO of the new InstructionalDeliveryMethods</param>
        /// <returns>A InstructionalDeliveryMethod object <see cref="InstructionalDeliveryMethod"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/instructional-delivery-methods", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructionalDeliveryMethodsV11")]
        public async Task<ActionResult<InstructionalDeliveryMethod>> PostInstructionalDeliveryMethodAsync([FromBody] InstructionalDeliveryMethod InstructionalDeliveryMethod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing InstructionalDeliveryMethods
        /// </summary>
        /// <param name="guid">GUID of the InstructionalDeliveryMethods to update</param>
        /// <param name="InstructionalDeliveryMethod">DTO of the updated InstructionalDeliveryMethod</param>
        /// <returns>A InstructionalDeliveryMethod object <see cref="InstructionalDeliveryMethod"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/instructional-delivery-methods/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructionalDeliveryMethodsV11")]
        public async Task<ActionResult<InstructionalDeliveryMethod>> PutInstructionalDeliveryMethodAsync([FromRoute] string guid, [FromBody] InstructionalDeliveryMethod InstructionalDeliveryMethod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a InstructionalDeliveryMethods
        /// </summary>
        /// <param name="guid">GUID to desired InstructionalDeliveryMethod</param>
        [HttpDelete]
        [Route("/instructional-delivery-methods/{guid}", Name = "DefaultDeleteInstructionalDeliveryMethods", Order = -10)]
        public async Task<IActionResult> DeleteInstructionalDeliveryMethodAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
