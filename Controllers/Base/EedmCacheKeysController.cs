// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Coordination.Base.Services;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Clear API cache keys for Ethos integration
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EedmCacheKeysController : BaseCompressedApiController
    {
        private readonly IEedmCacheKeysService _eedmCacheKeysService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EedmCacheKeysController class.
        /// </summary>
        /// <param name="eedmCacheKeysService">Service of type <see cref="IEedmCacheKeysService">IEedmCacheKeysService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EedmCacheKeysController(IEedmCacheKeysService eedmCacheKeysService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _eedmCacheKeysService = eedmCacheKeysService;
            this._logger = logger;
        }

        /// <summary>
        /// POST - Clear EEDM cache keys
        /// </summary>
        [HttpPost]
        [HeaderVersionRoute("/eedm-cache-keys", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "ClearEedmCacheKeys", IsEedmSupported = true)]
        public void ClearEedmCacheKeys()
        {
            _eedmCacheKeysService.ClearEedmCacheKeys();
        }
    }
}
