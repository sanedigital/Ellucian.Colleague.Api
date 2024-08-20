// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to PurchasingArrangements
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class PurchasingArrangementsController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PurchasingArrangementsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PurchasingArrangementsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all purchasing-arrangements
        /// </summary>
        /// <returns>All <see cref="Dtos.PurchasingArrangement">PurchasingArrangements</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/purchasing-arrangements", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPurchasingArrangements", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.PurchasingArrangement>>> GetPurchasingArrangementsAsync()
        {
            return new List<Dtos.PurchasingArrangement>();
        }

        /// <summary>
        /// Retrieve (GET) an existing purchasing-arrangements
        /// </summary>
        /// <param name="guid">GUID of the purchasing-arrangements to get</param>
        /// <returns>A purchasingArrangements object <see cref="Dtos.PurchasingArrangement"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/purchasing-arrangements/{guid}", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPurchasingArrangementsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PurchasingArrangement>> GetPurchasingArrangementsByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No purchasing-arrangements was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new purchasingArrangements
        /// </summary>
        /// <param name="purchasingArrangements">DTO of the new purchasingArrangements</param>
        /// <returns>A purchasingArrangements object <see cref="Dtos.PurchasingArrangement"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/purchasing-arrangements", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPurchasingArrangementsV13")]
        public async Task<ActionResult<Dtos.PurchasingArrangement>> PostPurchasingArrangementsAsync([FromBody] Dtos.PurchasingArrangement purchasingArrangements)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing purchasingArrangement
        /// </summary>
        /// <param name="guid">GUID of the purchasingArrangement to update</param>
        /// <param name="purchasingArrangement">DTO of the updated purchasingArrangement</param>
        /// <returns>A purchasingArrangements object <see cref="Dtos.PurchasingArrangement"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/purchasing-arrangements/{guid}", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPurchasingArrangementsV13")]
        public async Task<ActionResult<Dtos.PurchasingArrangement>> PutPurchasingArrangementsAsync([FromRoute] string guid, [FromBody] Dtos.PurchasingArrangement purchasingArrangement)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a purchasingArrangement
        /// </summary>
        /// <param name="guid">GUID to desired purchasingArrangement</param>
        [HttpDelete]
        [Route("/purchasing-arrangements/{guid}", Name = "DefaultDeletePurchasingArrangements")]
        public async Task<IActionResult> DeletePurchasingArrangementAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
