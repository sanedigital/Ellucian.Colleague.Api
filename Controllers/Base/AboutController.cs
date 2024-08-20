// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Reflection;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AboutController : BaseCompressedApiController
    {
        /// <summary>
        /// Provides the About-related endpoints
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AboutController(IActionContextAccessor contextAccessor, ApiSettings apiSettings) : base(contextAccessor, apiSettings) { }

        /// <summary>
        /// Retrieves version information for the Colleague Web API.
        /// </summary>
        /// <returns>Version information.</returns>
        [HttpGet]
        [HeaderVersionRoute("/about", 1, true, Name = "GetAbout")]
        public async Task<ActionResult<IEnumerable<About>>> GetAboutAsync()
        {
            var apiVersionCollection = new List<About>();
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            About versionInfo = new About("Colleague Web Api", assemblyVersion.ToString(3));
            apiVersionCollection.Add(versionInfo);

            return apiVersionCollection;
        }
    }
}
