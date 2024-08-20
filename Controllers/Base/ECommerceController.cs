// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to e-Commerce data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ECommerceController : BaseCompressedApiController
    {
        private readonly IECommerceService _ecommerceService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// ECommerceController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="ecommerceService">Service of type <see cref="IECommerceService">IECommerceService</see></param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ECommerceController(IAdapterRegistry adapterRegistry, IECommerceService ecommerceService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _ecommerceService = ecommerceService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Convenience Fees.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>All <see cref="ConvenienceFee">Convenience Fee codes and descriptions.</see></returns>
        /// <note>ConvenienceFee is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/ecommerce/convenience-fees", 1, true, Name = "GetConvenienceFees")]
        public ActionResult<IEnumerable<ConvenienceFee>> GetConvenienceFees()
        {
            try
            {
                return Ok(_ecommerceService.GetConvenienceFees());
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
        }
    }
}
