// Copyright 2016-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Helpers;
using Ellucian.Colleague.Api.Middleware;
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Controller;
using Ellucian.Web.Mvc.Session;
using Ellucian.Web.Resource;
using Microsoft.AspNetCore.Antiforgery;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Claims;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller to modify resource files and save the history of modifications done to the res files.
    /// </summary>
    /// <seealso cref="BaseCompressedController" />
    public class ResourceFileEditorController : BaseCompressedController
    {
        private IResourceRepository resourceRepository;
        private ApiSettings apiSettings;
        private ILogger logger;
        private readonly IAntiforgery antiforgery;
        private readonly AppConfigUtility appConfigUtility;
        private readonly AuditLoggingAdapter _auditLoggingAdapter;
        private readonly ISettingsRepository _settingsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileEditorController" /> class.
        /// </summary>
        /// <param name="resourceRepository">The resource repository.</param>
        /// <param name="apiSettings">API Settings</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sessionCookieManager"></param>
        /// <param name="antiforgery"></param>
        /// <param name="appConfigUtility"></param>
        /// <param name="auditLoggingAdapter"></param>
        /// <param name="settingsRepository"></param>
        /// <exception cref="ArgumentNullException">resourceRepository</exception>
        public ResourceFileEditorController(IResourceRepository resourceRepository, ApiSettings apiSettings,
            ILogger logger, SessionCookieManager sessionCookieManager, IAntiforgery antiforgery, AppConfigUtility appConfigUtility,
            AuditLoggingAdapter auditLoggingAdapter, ISettingsRepository settingsRepository)
            : base(logger, sessionCookieManager, antiforgery)
        {
            this.resourceRepository = resourceRepository ?? throw new ArgumentNullException(nameof(resourceRepository));
            this.apiSettings = apiSettings;
            this.logger = logger;
            this.antiforgery = antiforgery;
            this.appConfigUtility = appConfigUtility;
            _auditLoggingAdapter = auditLoggingAdapter;
            _settingsRepository = settingsRepository;
        }

        /// <summary>
        /// Returns the Resource File Editor if a user is logged in 
        /// </summary>
        /// <returns>Resource File Editor</returns>
        public ActionResult ResourceFileEditor()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before accessing the Resource File Editor";
                string returnUrl = Url.Action("ResourceFileEditor", "ResourceFileEditor");
                return RedirectToAction("Login", "Admin", new { returnUrl = returnUrl, error = error });
            }
            return View();
        }

        /// <summary>
        /// Gets the list of resource files found in the working directory
        /// </summary>
        /// <returns></returns>
        public ActionResult GetResourceFiles(string cultureCode)
        {
            string uiCultureLang = cultureCode.Split("-").FirstOrDefault();
            List<string> listResFilesFound = resourceRepository.GetResourceFilePaths(true);
            List<KeyValuePair<string, string>> resFileNamePaths = listResFilesFound.Select(x => new KeyValuePair<string, string>(System.IO.Path.GetFileName(x), x)).OrderBy(resx => resx.Key).ToList();

            // Get default list which is en
            var resFileResults = resFileNamePaths.Where(f => f.Key.Split(".").Length < 3).ToList();

            // Replace file pairs that match the ui culture setting
            if (!cultureCode.Equals("en"))
                resFileResults = resFileNamePaths.Where(f => f.Key.Split(".")[1].Equals(cultureCode)).ToList();

            return Json(resFileResults);

        }

        /// <summary>
        /// Get the current ui culutre from the settings
        /// </summary>
        /// <returns>JSON containing the current ui culutre code</returns>
        public JsonResult GetCurrentUICulture() => Json(_settingsRepository.Get().DefaultUiCulture);


        /// <summary>
        /// Get list of supported UI cultures
        /// </summary>
        /// <returns></returns>
        public JsonResult GetSupportedUICultures()
        {
            List<KeyValuePair<string, string>> supportedCultureList = new();

            foreach (var code in Bootstrapper.SupportedUICultures)
            {
                var culture = new CultureInfo(code);
                supportedCultureList.Add(new KeyValuePair<string, string>(culture.NativeName, culture.Name));
            }

            return Json(supportedCultureList);
        }

        /// <summary>
        /// Gets the resource items of the resource file in the provided file path
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Gets the resource items in the file as a JSonResult object </returns>
        public ActionResult GetResourceItemsByFile(string filePath)
        {

            try
            {
                ResourceFile currentFile = resourceRepository.GetResourceFile(filePath);
                List<ResourceFileEntry> listResourceItems = currentFile.ResourceFileEntries;

                return Json(currentFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                string errorMessage = "Error retrieving values: " + ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage += " (" + ex.InnerException.Message + ")";
                }
                return Json(errorMessage);
            }
        }

        /// <summary>
        /// Saves the updated resource file to the path of the file
        /// </summary>
        /// <param name="model">The resource file with the updated values</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SaveResourceFile(string model)
        {
            try
            {
                ResourceFileModel file = Newtonsoft.Json.JsonConvert.DeserializeObject<ResourceFileModel>(model);
                ResourceFile updatedResourceFile = new ResourceFile(file.ResourceFileName);
                ResourceFile oldResourceFile = resourceRepository.GetResourceFile(file.ResourceFilePath);

                //Map the fileEntryModels to FileEntry object
                updatedResourceFile.ResourceFileEntries = file.ResourceFileEntries.Select
                    (x => new ResourceFileEntry() { Key = x.Key, Value = x.Value, OriginalValue = x.OriginalValue }).ToList();

                resourceRepository.UpdateResourceFile(file.ResourceFilePath, updatedResourceFile);

                try
                {
                    var userPrincipal = LocalUserUtilities.GetCurrentUser(Request) as ClaimsPrincipal;
                    var userId = userPrincipal?.FindFirstValue("pid") ?? "LocalAdmin";
                    var auditLogProps = new AuditLogProperties(userId);
                    updatedResourceFile.AuditLogConfigurationChanges(oldResourceFile, auditLogProps, _auditLoggingAdapter);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to audit log changes for web api resource file editor.");
                }

                PerformBackupConfig();
                return Json("Success");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                string errorMessage = "Error when saving values: " + ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage += " (" + ex.InnerException.Message + ")";
                }
                return Json(errorMessage);
            }
        }

        /// <summary>
        /// Backup all API configs
        /// </summary>
        public async void PerformBackupConfig()
        {
            // SaaS backup
            if (appConfigUtility.ConfigServiceClientSettings != null && appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment)
            {
                string username = "unknown";
                try
                {
                    username = HttpContext.User.Identity.Name;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, "Error at user identity name");
                }

                try
                {
                    // send a copy of this latest config data to the storage service.
                    var configObject = appConfigUtility.GetApiConfigurationObject();
                    appConfigUtility.StorageServiceClient.PostConfigurationAsync(
                        configObject.Namespace, configObject.ConfigData, username,
                        configObject.ConfigVersion, configObject.ProductId, configObject.ProductVersion).GetAwaiter().GetResult();

                    // after submitting a new snapshot, set the lastrestoredchecksum to this new snapshot's checksum.
                    // This must be done to avoid a looping situation where instances keep performing merges
                    // in lock step with each other due to lastrestoredchecksum file containing an older checksum, when 
                    // there are changes that are repeated (e.g. logging toggled on/off).
                    var currentChecksum = Utilities.GetMd5ChecksumString(configObject.ConfigData);
                    Utilities.SetLastRestoredChecksum(currentChecksum);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Configuration changes have been saved, but the backup to config storage service failed. See API log for more details.");
                }

            }
            else
            {
                if (!apiSettings.EnableConfigBackup)
                {
                    return;
                }
                try
                {
                    var cookieValue = LocalUserUtilities.GetCookie(Request);
                    if (string.IsNullOrEmpty(cookieValue))
                    {
                        throw new ColleagueWebApiException("Log in first");
                    }
                    var baseUrl = cookieValue.Split('*')[0];
                    var token = cookieValue.Split('*')[1];
                    var client = new ColleagueApiClient(baseUrl, logger);
                    client.Credentials = token;
                    await client.PostBackupApiConfigDataAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Configuration changes have been saved, but the backup action failed. See API log for more details.");
                    throw;
                }
            }
        }
    }
}
