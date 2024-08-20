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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to PurchaseClassifications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class PurchaseClassificationsController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PurchaseClassificationsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PurchaseClassificationsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all purchase-classifications
        /// </summary>
        /// <returns>All <see cref="Dtos.PurchaseClassifications">PurchaseClassifications</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/purchase-classifications", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPurchaseClassifications", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.PurchaseClassifications>>> GetPurchaseClassificationsAsync()
        {
            return new List<Dtos.PurchaseClassifications>();
        }

        /// <summary>
        /// Retrieve (GET) an existing purchase-classifications
        /// </summary>
        /// <param name="guid">GUID of the purchase-classifications to get</param>
        /// <returns>A purchaseClassifications object <see cref="Dtos.PurchaseClassifications"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/purchase-classifications/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPurchaseClassificationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PurchaseClassifications>> GetPurchaseClassificationsByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No purchase-classifications was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new purchaseClassifications
        /// </summary>
        /// <param name="purchaseClassifications">DTO of the new purchaseClassifications</param>
        /// <returns>A purchaseClassifications object <see cref="Dtos.PurchaseClassifications"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/purchase-classifications", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPurchaseClassificationsV10")]
        public async Task<ActionResult<Dtos.PurchaseClassifications>> PostPurchaseClassificationsAsync([FromBody] Dtos.PurchaseClassifications purchaseClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing purchaseClassifications
        /// </summary>
        /// <param name="guid">GUID of the purchaseClassifications to update</param>
        /// <param name="purchaseClassifications">DTO of the updated purchaseClassifications</param>
        /// <returns>A purchaseClassifications object <see cref="Dtos.PurchaseClassifications"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/purchase-classifications/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPurchaseClassificationsV10")]
        public async Task<ActionResult<Dtos.PurchaseClassifications>> PutPurchaseClassificationsAsync([FromRoute] string guid, [FromBody] Dtos.PurchaseClassifications purchaseClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a purchaseClassifications
        /// </summary>
        /// <param name="guid">GUID to desired purchaseClassifications</param>
        [HttpDelete]
        [Route("/purchase-classifications/{guid}", Name = "DefaultDeletePurchaseClassifications")]
        public async Task<IActionResult> DeletePurchaseClassificationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
