// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Reflection;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class VersionController : BaseCompressedApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VersionController(IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {

        }

        /// <summary>
        /// Retrieves version information for the Colleague Web API.
        /// </summary>
        /// <returns>Version information.</returns>
        /// <note>This request supports anonymous access. No Colleague entity data is exposed via this anonymous request. See :ref:`anonymousapis` for additional information.</note>
        [HttpGet]
        [HeaderVersionRoute("/version", 1, true, Name = "GetVersion")]
        public ApiVersion Get()
        {
            ApiVersion versionInfo = new ApiVersion();
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            versionInfo.ProductVersion = assemblyVersion.ToString(3);

            return versionInfo;
        }
    }
}
