// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Constraints;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for vendor commodity.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class VendorCommodityController : BaseCompressedApiController
    {
        private readonly IVendorCommodityService vendorCommodityService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the VendorCommodityController object.
        /// </summary>
        /// <param name="vendorCommodityService">vendor commodity service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VendorCommodityController(IVendorCommodityService vendorCommodityService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.vendorCommodityService = vendorCommodityService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns a vendor and commodity code association.
        /// </summary>
        /// <param name="vendorId">vendor id.</param>        
        /// <param name="commodityCode">Commodity code.</param>
        /// <returns>VendorCommodity Dto.</returns>
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.VENDOR, CREATE.UPDATE.VOUCHER, CREATE.UPDATE.REQUISITION and CREATE.UPDATE.PURCHASE.ORDER
        /// </accessComments>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "vendorId", "commodityCode")]
        [HeaderVersionRoute("/vendor-commodities", 1, true, Name = "GetVendorCommodityAsync")]
        public async Task<ActionResult<VendorCommodity>> GetVendorCommodityAsync(string vendorId, string commodityCode)
        {
            if (string.IsNullOrEmpty(vendorId) && string.IsNullOrEmpty(commodityCode))
            {
                string message = "vendorId and commodityCode must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var vendorCommoditiesDto = await vendorCommodityService.GetVendorCommodityAsync(vendorId, commodityCode);
                return vendorCommoditiesDto;
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the vendor commodities.");
                return CreateHttpResponseException("Insufficient permissions to get the vendor commodities.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to get VendorCommodities.");
                return CreateHttpResponseException("Unable to get vendor commodities.", HttpStatusCode.BadRequest);
            }
        }

    }
}
