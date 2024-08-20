// Copyright 2014-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Logging;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Integration Configuration data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ConfigurationController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IConfigurationService configurationService;
        private readonly IProxyService proxyService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        private readonly AuditLoggingAdapter _auditLoggingAdapter;
        private readonly ICurrentUserFactory _currentUserFactory;

        /// <summary>
        /// Initializes a new instance of the ConfigurationController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="configurationService">Service of type <see cref="IConfigurationService">IConfigurationService</see></param>
        /// <param name="proxyService">Service of type <see cref="IProxyService">IProxyService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        /// <param name="auditLoggingAdapter"></param>
        /// <param name="currentUserFactory"></param>
        public ConfigurationController(IAdapterRegistry adapterRegistry, IConfigurationService configurationService, IProxyService proxyService, ILogger logger,
            IActionContextAccessor actionContextAccessor, ApiSettings apiSettings, AuditLoggingAdapter auditLoggingAdapter, ICurrentUserFactory currentUserFactory)
            : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.configurationService = configurationService;
            this.proxyService = proxyService;
            this.logger = logger;
            _auditLoggingAdapter = auditLoggingAdapter;
            _currentUserFactory = currentUserFactory;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves an integration configuration.
        /// </summary>
        /// <returns>An <see cref="IntegrationConfiguration">IntegrationConfiguration</see> information</returns>
        [HttpGet]
        [HeaderVersionRoute("/configuration/ems/{configId}", 1, true, Name = "GetIntegrationConfiguration")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.IntegrationConfiguration>> Get(string configId)
        {
            try
            {
                return await configurationService.GetIntegrationConfiguration(configId);
            }
            catch (PermissionsException peex)
            {
                logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the proxy configuration
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can access the proxy information.
        /// </accessComments>
        /// <returns>Proxy configuration information.</returns>
        /// <note>Proxy Configuration is cached for 1 hour.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/proxy", 1, true, Name = "GetProxyConfiguration")]
        public async Task<ActionResult<ProxyConfiguration>> GetProxyConfigurationAsync()
        {
            try
            {
                return await proxyService.GetProxyConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogInformation(e.ToString());
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the User Profile Configuration
        /// </summary>
        /// <returns><see cref="UserProfileConfiguration">User Profile Configuration</see></returns>
        /// <note>UserProfileConfiguration is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this API instead.")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/user-profile", 1, false, Name = "UserProfileConfiguration")]
        public async Task<ActionResult<UserProfileConfiguration>> GetUserProfileConfigurationAsync()
        {
            try
            {
                return await configurationService.GetUserProfileConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving User Profile Configuration: " + ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the User Profile Configuration
        /// </summary>
        /// <returns><see cref="UserProfileConfiguration2">User Profile Configuration</see></returns>
        /// <note>UserProfileConfiguration2 is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/user-profile", 2, true, Name = "UserProfileConfiguration2")]
        public async Task<ActionResult<UserProfileConfiguration2>> GetUserProfileConfiguration2Async()
        {
            try
            {
                return await configurationService.GetUserProfileConfiguration2Async();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving User Profile Configuration: " + ex.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Emergency Information Configuration
        /// </summary>
        /// <returns><see cref="EmergencyInformationConfiguration">Emergency Information Configuration</see></returns>
        /// <note>EmergencyInformationConfiguration is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this API instead.")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/emergency-information", 1, false, Name = "GetEmergencyInformationConfiguration")]
        public async Task<ActionResult<EmergencyInformationConfiguration>> GetEmergencyInformationConfigurationAsync()
        {
            try
            {
                return await configurationService.GetEmergencyInformationConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving Emergency Information Configuration: " + ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Emergency Information Configuration
        /// </summary>
        /// <returns><see cref="EmergencyInformationConfiguration2">Emergency Information Configuration</see></returns>
        /// <note>EmergencyInformationConfiguration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/emergency-information", 2, true, Name = "GetEmergencyInformationConfiguration2")]
        public async Task<ActionResult<EmergencyInformationConfiguration2>> GetEmergencyInformationConfiguration2Async()
        {
            try
            {
                return await configurationService.GetEmergencyInformationConfiguration2Async();
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving Emergency Information Configuration: " + ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Restriction Configuration
        /// </summary>
        /// <returns><see cref="RestrictionConfiguration">Restriction Configuration</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/configuration/restriction", 1, true, Name = "GetRestrictionConfiguration")]
        public async Task<ActionResult<RestrictionConfiguration>> GetRestrictionConfigurationAsync()
        {
            try
            {
                return await configurationService.GetRestrictionConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving Restriction Configuration: ", ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Privacy Configuration
        /// </summary>
        /// <returns><see cref="PrivacyConfiguration">Privacy Configuration</see></returns>
        /// <note>Privacy Configuration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/privacy", 1, true, Name = "GetPrivacyConfigurationAsync")]
        public async Task<ActionResult<PrivacyConfiguration>> GetPrivacyConfigurationAsync()
        {
            try
            {
                return await configurationService.GetPrivacyConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving privacy configuration";
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving privacy configuration: ", ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Organizational Relationship Configuration
        /// </summary>
        /// <returns><see cref="OrganizationalRelationshipConfiguration">OrganizationalRelationship Configuration</see></returns>
        /// <note>OrganizationalRelationshipConfiguration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/organizational-relationships", 1, true, Name = "GetOrganizationRelationshipConfigurationAsync")]
        public async Task<ActionResult<OrganizationalRelationshipConfiguration>> GetOrganizationalRelationshipConfigurationAsync()
        {
            try
            {
                return await configurationService.GetOrganizationalRelationshipConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving OrganizationalRelationship Configuration: ", ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #region backup configuration

        /// <summary>
        /// Writes a new configuration data record to Colleague 
        /// </summary>
        /// <param name="backupData">data to backup</param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/configuration", 1, false, RouteConstants.EllucianConfigurationFormat, Name = "PostBackupConfigData")]
        public async Task<ActionResult<BackupConfiguration>> PostConfigBackupDataAsync([FromBody] BackupConfiguration backupData)
        {
            try
            {
                var result = await configurationService.WriteBackupConfigurationAsync(backupData);
                return result;
            }
            catch (PermissionsException ex)
            {
                logger.LogError("Permission error occurred: ", ex.Message);
                return CreateHttpResponseException("Permission error occurred.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while adding Backup Configuration: ", ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Lookup a configuration data record from Colleague
        /// </summary>
        /// <param name="backupDataQuery">Criteria for looking up backup config data.</param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/configuration", 1, false, RouteConstants.EllucianConfigurationFormat, Name = "QueryBackupConfigData")]
        public async Task<ActionResult<IEnumerable<BackupConfiguration>>> QueryBackupConfigDataByPostAsync([FromBody] BackupConfigurationQueryCriteria backupDataQuery)
        {
            // must supply namespace + optional datetime filter
            /* e.g. POST /qapi/configuration/
             {
                "Namespace": "Ellucian/Colleague Web API/1.18.0.0/dvetk_wstst01_rt",
                "OnOrBeforeDateTimeUtc":"2017-10-22T02:57:22Z"
             }
             */
            if (backupDataQuery == null)
            {
                return CreateHttpResponseException(
                    "Missing required query criteria.",
                    HttpStatusCode.BadRequest);
            }
            try
            {
                var result = await configurationService.ReadBackupConfigurationAsync(backupDataQuery);
                return Ok(result);
            }
            catch (PermissionsException ex)
            {
                logger.LogError("Permission error occurred: ", ex.Message);
                return CreateHttpResponseException("Permission error occurred.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving Backup Configuration: ", ex.Message);
                HttpStatusCode errorCode = HttpStatusCode.BadRequest;
                if (ex.Message.Contains("No backup record found"))
                {
                    errorCode = HttpStatusCode.NotFound;
                }
                return CreateHttpResponseException(ex.Message, errorCode);
            }
        }

        /// <summary>
        /// Reads a configuration data record from Colleague
        /// </summary>
        /// <param name="id">ID or key of backup record</param>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/configuration/{id}", 1, false, RouteConstants.EllucianConfigurationFormat, Name = "GetBackupConfigData")]
        public async Task<ActionResult<BackupConfiguration>> GetConfigBackupDataAsync(string id)
        {
            // must supply either the id, e.g.
            // /configuration/0503be99-fd54-4152-939b-6743d00e0334
            if (string.IsNullOrWhiteSpace(id))
            {
                return CreateHttpResponseException(
                    "Config data record ID must be specified.",
                    HttpStatusCode.BadRequest);
            }
            try
            {
                var query = new BackupConfigurationQueryCriteria()
                {
                    ConfigurationIds = new List<string>() { id },
                    Namespace = null
                };
                var result = await configurationService.ReadBackupConfigurationAsync(query);
                return result.FirstOrDefault();
            }
            catch (PermissionsException ex)
            {
                logger.LogError("Permission error occurred: ", ex.Message);
                return CreateHttpResponseException("Permission error occurred.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving Backup Configuration: ", ex.Message);
                HttpStatusCode errorCode = HttpStatusCode.BadRequest;
                if (ex.Message.Contains("No backup record found"))
                {
                    errorCode = HttpStatusCode.NotFound;
                }
                return CreateHttpResponseException(ex.Message, errorCode);
            }
        }

        /// <summary>
        /// Causes this API instance to perform a backup of its configuration data.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/configuration/backup-api-config", 1, false, RouteConstants.EllucianConfigurationFormat, Name = "PostBackupApiConfigData")]
        public async Task<IActionResult> PostBackupApiConfigAsync()
        {
            try
            {
                await configurationService.BackupApiConfigurationAsync();
                return Ok();
            }
            catch (PermissionsException ex)
            {
                logger.LogError("Permission error occurred: ", ex.Message);
                return CreateHttpResponseException("Permission error occurred", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while backing up API configuration data: ", ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Causes this API instance to perform a restore of its configuration data using
        /// backup data retrieved from Colleague. Optionally merge the json files.
        /// Also cause app pool to recycle.
        /// Note: the json file merge operation currently is supported on a brand new instance only. It will
        /// not run on an instance whose resource files have already been modified.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/configuration/restore-api-config", 1, false, RouteConstants.EllucianConfigurationFormat, Name = "PostRestoreApiConfigData")]
        public async Task<IActionResult> PostRestoreApiConfigAsync(DateTimeOffset? onOrBeforeDateTime = null)
        {
            try
            {
                var apiBackupData = await configurationService.RestoreApiBackupConfigurationAsync();

                // even though the service method will update web.config to restore the appSettings values,
                // we'll still ensure the app pool gets recycled by touching web.config again here.
                return Ok();
            }
            catch (PermissionsException ex)
            {
                logger.LogError("Permission error occurred: ", ex.Message);
                return CreateHttpResponseException("Permission error occurred", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred restoring this API instance's previous configuration data: ", ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }


        #endregion

        /// <summary>
        /// Retrieves Colleague Self-Service configuration information
        /// </summary>
        /// <returns>A <see cref="SelfServiceConfiguration"/> object</returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Colleague Self-Service configuration information is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/self-service", 1, true, Name = "GetSelfServiceConfigurationAsync")]
        public async Task<ActionResult<SelfServiceConfiguration>> GetSelfServiceConfigurationAsync()
        {
            try
            {
                return await configurationService.GetSelfServiceConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving Self-Service Configuration.");
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Required Document Configuration
        /// </summary>
        /// <accessComments>Any authenticated user can get the Required Document Configuration</accessComments>
        /// <returns><see cref="RequiredDocumentConfiguration">Required Document Configuration</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/configuration/required-document", 1, true, Name = "GetRequiredDocumentConfigurationAsync")]
        public async Task<ActionResult<RequiredDocumentConfiguration>> GetRequiredDocumentConfigurationAsync()
        {
            try
            {
                return await configurationService.GetRequiredDocumentConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred while retrieving Required Document Configuration: " + ex.Message);
                return CreateHttpResponseException("Could not retrieve Required Document Configuration", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Session Configuration
        /// </summary>
        /// <accessComments>Session Configuration is available anonymously</accessComments>
        /// <returns><see cref="SessionConfiguration">Session Configuration</see></returns>
        /// <note>This request supports anonymous access. No Colleague entity data is exposed via this anonymous request. See :ref:`anonymousapis` for additional information.</note>
        [AllowAnonymous]
        [HttpGet]
        [HeaderVersionRoute("/configuration/session", 1, true, Name = "GetSessionConfigurationAsync")]
        public async Task<ActionResult<SessionConfiguration>> GetSessionConfigurationAsync()
        {
            try
            {
                return await configurationService.GetSessionConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving Session Configuration.");
                return CreateHttpResponseException("Could not retrieve Session Configuration", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Calls configuration service and returns the Audit Log Configuration object.
        /// </summary>
        /// <returns><see cref="Dtos.AuditLogConfiguration">Audit Log Configuration</see> object.</returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAuditLogConfiguration, BasePermissionCodes.UpdateAuditLogConfiguration })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/configuration/audit-logs/events", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAuditLogConfigData", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AuditLogConfiguration>>> GetAuditLogConfigurationAsync()
        {
            var bypassCache = false;
            if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                configurationService.ValidatePermissions(GetPermissionsMetaData());
                return Ok(await configurationService.GetAuditLogConfigurationAsync(bypassCache));
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving Audit Log Configuration.");
                return CreateHttpResponseException("Could not retrieve Audit Log Configuration", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Calls configuration repository and returns the Audit Log Configuration object.
        /// </summary>
        /// <returns><see cref="Dtos.AuditLogConfiguration">Audit Log Configuration</see> object.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, HttpPost, PermissionsFilter(new string[] { BasePermissionCodes.UpdateAuditLogConfiguration })]
        [HttpPut]
        [HeaderVersionRoute("/configuration/audit-logs/seteventenabled", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAuditLogConfigData", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AuditLogConfiguration>> UpdateAuditLogConfigurationAsync([FromBody] Dtos.AuditLogConfiguration auditLogConfiguration)
        {
            if (auditLogConfiguration == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("auditLogConfiguration",
                    IntegrationApiUtility.GetDefaultApiError("Request body must contain a valid Audit Log Configuration.")));
            }
            try
            {
                configurationService.ValidatePermissions(GetPermissionsMetaData());
                return await configurationService.UpdateAuditLogConfigurationAsync(auditLogConfiguration);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating Audit Log Configuration.");
                return CreateHttpResponseException("Could not update Audit Log Configuration", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update or create OAuth configuration settings that correspond to the OACF form in the UI.
        /// </summary>
        /// <param name="settings"></param>
        [HttpPut]
        [PermissionsFilter(new string[] { BasePermissionCodes.UpdateConfigurationSettings })]
        [Metadata(HttpMethodPermission = BasePermissionCodes.UpdateConfigurationSettings)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [HeaderVersionRoute("/oauth-configurations", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "UpdateOAuthConfigurationSettings", IsEedmSupported = true)]
        public async Task<IActionResult> UpdateOAuthConfigurationSettings(Dtos.OAuthConfigurationSettings settings)
        {
            try
            {
                configurationService.ValidatePermissions(GetPermissionsMetaData());

                if (settings == null)
                    return BadRequest($"{nameof(Dtos.OAuthConfigurationSettings)} cannot be null.");

                // update config settings
                var statusCode = await configurationService.UpdateOAuthConfigurationSettingsAsync(settings.ConfigId, settings.Description, settings.ServerUrl, settings.ClientId, settings.SecretKey);

                using (Serilog.Context.LogContext.PushProperty("category", "Configuration"))
                using (Serilog.Context.LogContext.PushProperty("subCategory", "Update"))
                using (Serilog.Context.LogContext.PushProperty("action", "update"))
                using (Serilog.Context.LogContext.PushProperty("user", _currentUserFactory.CurrentUser.UserId))
                using (Serilog.Context.LogContext.PushProperty("status", "success"))
                {
                    if (statusCode == StatusCodes.Status201Created)
                    {
                        _auditLoggingAdapter.Info($"OACF OAuth Configuration Settings Created: {JsonConvert.SerializeObject(settings)}");
                    }
                    if (statusCode == StatusCodes.Status204NoContent)
                    {
                        if (!string.IsNullOrWhiteSpace(settings.Description))
                            _auditLoggingAdapter.Info($"OACF OAuth Configuration Settings Configuration ID {settings.ConfigId}: Property, {nameof(settings.Description)}, set to '{settings.Description}'");
                        _auditLoggingAdapter.Info($"OACF OAuth Configuration Settings Configuration ID {settings.ConfigId}: Property, {nameof(settings.ServerUrl)}, set to {settings.ServerUrl}");
                        _auditLoggingAdapter.Info($"OACF OAuth Configuration Settings Configuration ID {settings.ConfigId}: Property, {nameof(settings.ClientId)}, set to '{settings.ClientId}'");
                        _auditLoggingAdapter.Info($"OACF OAuth Configuration Settings Configuration ID {settings.ConfigId}: Property, {nameof(settings.SecretKey)}, set to '{settings.SecretKey}'");
                    }
                }

                return new StatusCodeResult(statusCode);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating OAuth configuration settings.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.InternalServerError);
            }
        }




        ///////////////////////////////////////////////////////////////////////////////////
        ///                                                                             ///
        ///                               CF Team                                       ///                                                                             
        ///                         TAX INFORMATION VIEWS                               ///
        ///           TAX FORMS CONFIGURATION, CONSENTs, STATEMENTs, PDFs               ///
        ///                                                                             ///
        ///////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method gets Tax Form Configuration for the tax form passed in.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Tax Form Configuration for the type of tax form.</returns>
        [HttpGet]
        [HeaderVersionRoute("/configuration/tax-forms/{taxFormId}", 2, true, Name = "GetTaxFormConfiguration2")]
        public async Task<ActionResult<TaxFormConfiguration2>> GetTaxFormConfiguration2Async(string taxFormId)
        {
            try
            {
                var taxFormConfiguration = await this.configurationService.GetTaxFormConsentConfiguration2Async(taxFormId);
                return taxFormConfiguration;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        #region OBSOLETE METHODS

        /// <summary>
        /// This method gets Tax Form Configuration for the tax form passed in.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Tax Form Configuration for the type of tax form.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetTaxFormConfiguration2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/tax-forms/{taxFormId}", 1, false, Name = "GetTaxFormConfiguration")]
        public async Task<ActionResult<TaxFormConfiguration>> GetTaxFormConfigurationAsync(TaxForms taxFormId)
        {
            var taxFormConfiguration = await this.configurationService.GetTaxFormConsentConfigurationAsync(taxFormId);

            return taxFormConfiguration;
        }

        #endregion
    }
}
