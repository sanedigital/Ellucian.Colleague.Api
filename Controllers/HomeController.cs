// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Dmi.Client.Das;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Mvc.Controller;
using Ellucian.Web.Mvc.Session;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Home controller. Matches all requests for /.
    /// </summary>
    public class HomeController : BaseCompressedController
    {
        private ISettingsRepository settingsRepository;
        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the HomeController class.
        /// </summary>
        /// <param name="settingsRepository">ISettingsRepository instance</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="sessionCookieManager"></param>
        /// <param name="antiforgery"></param>
        public HomeController(ISettingsRepository settingsRepository, ILogger logger, SessionCookieManager sessionCookieManager, IAntiforgery antiforgery)
            : base(logger, sessionCookieManager, antiforgery)
        {
            if (settingsRepository == null)
            {
                throw new ArgumentNullException("settingsRepository");
            }
            this.settingsRepository = settingsRepository;
            this.logger = logger;
        }

        /// <summary>
        /// /index page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var domainSettings = settingsRepository.Get();
            ApiStatusModel apiStatus = new ApiStatusModel();
            apiStatus.UnSuccessfulLoginCounter = DasSession.DasUnsuccessfulLoginCounter;
            apiStatus.UseDasDataReader = domainSettings.ColleagueSettings.GeneralSettings.UseDasDatareader;
            return View(apiStatus);
        }

        /// <summary>
        /// This is the default route for anything coming in that doesn't match other patterns
        /// </summary>
        /// <returns></returns>
        public IActionResult NotSupportedRoute()
        {
            var apiException = new Ellucian.Web.Http.Exceptions.IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError);
            var serialized = JsonConvert.SerializeObject(apiException);
            return new ContentResult() { Content = serialized, ContentType = "application/json", StatusCode = (int)apiException.HttpStatusCode };

        }

    }
}
