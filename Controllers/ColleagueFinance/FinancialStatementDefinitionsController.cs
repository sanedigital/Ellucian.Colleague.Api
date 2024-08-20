// Copyright 2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for FinancialStatementsConfiguration
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class FinancialStatementDefinitionsController : BaseCompressedApiController
    {
        private readonly IFinancialStatementDefinitionsService _financialStatementDefinitionsService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// This constructor initializes the FinancialStatementsConfigurationController object
        /// </summary>
        /// <param name="financialStatementDefinitionsService">FinancialStatementDefinitionsService object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialStatementDefinitionsController(IFinancialStatementDefinitionsService financialStatementDefinitionsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _financialStatementDefinitionsService = financialStatementDefinitionsService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the financial statement definition for the given preference type.
        /// </summary>
        /// <param name="preferenceType">The financial statement definition key.</param>
        /// <returns>The financial statement definition for the specified key.</returns>
        /// /// <accessComments>
        /// Requires permission USE.FINANCIAL.STATEMENTS.
        /// </accessComments>
        [HttpGet]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get financial statement definition.", HttpMethodDescription = "Get financial statement definition.")]
        [HeaderVersionRoute("/financial-statement-definitions/{preferenceType}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFinancialStatementDefinitionsAsync", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<Dtos.ColleagueFinance.FinancialStatementDefinition>> GetFinancialStatementDefinitionsAsync(string preferenceType)
        {
            if (string.IsNullOrEmpty(preferenceType))
            {
                logger.LogError("Error retrieving financial statement definition due to invalid arguments.");
                return CreateHttpResponseException("Could not retrieve financial statement definition.");
            }

            try
            {
                var financialStatementDefinition = await _financialStatementDefinitionsService.GetFinancialStatementDefinitionsAsync(preferenceType);
                if (financialStatementDefinition == null)
                {
                    return CreateHttpResponseException("No financial statement definition exists of the given type.", System.Net.HttpStatusCode.NotFound);
                }
                return Ok(financialStatementDefinition);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, "GetFinancialStatementDefinitionsAsync ColleagueSessionExpiredException");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e, "GetFinancialStatementDefinitionsAsync PermissionException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving financial statement definition of type " + preferenceType + ".");
                return CreateHttpResponseException("Could not retrieve financial statement definition.");
            }
        }

        /// <summary>
        /// Updates the financial statement definition with the given parameters.
        /// </summary>
        /// <param name="finStmtDefinition">The financial statement definition to be updated</param>
        /// <returns>The updated financial statement definition.</returns>
        /// /// <accessComments>
        /// Requires permission USE.FINANCIAL.STATEMENTS.
        /// </accessComments>
        [HttpPut]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update financial statement definition.", HttpMethodDescription = "Update financial statement definition.")]
        [HeaderVersionRoute("/financial-statement-definitions/{preferenceType}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "UpdateFinancialStatementDefinitionsAsync", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<Dtos.ColleagueFinance.FinancialStatementDefinition>> UpdateFinancialStatementDefinitionsAsync([FromBody] FinancialStatementDefinition finStmtDefinition)
        {
            if (finStmtDefinition == null)
            {
                return CreateHttpResponseException("Could not update financial statement definition.");
            }
            try
            {
                var updatedFinancialStatementDefinition = await _financialStatementDefinitionsService.UpdateFinancialStatementDefinitionsAsync(finStmtDefinition.Id, finStmtDefinition.PreferenceType, finStmtDefinition.Preferences);
                return updatedFinancialStatementDefinition;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, "UpdateFinancialStatementDefinitionsAsync ColleagueSessionExpiredException");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e, "UpdateFinancialStatementDefinitionsAsync PermissionException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating financial statement definition of type " + finStmtDefinition.PreferenceType + ".");
                return CreateHttpResponseException("Could not update financial statement definition.");
            }
        }

        /// <summary>
        /// Delete a financial statement definition.
        /// </summary>
        /// <param name="preferenceType">The financial statement definition type.</param>
        /// <returns>nothing</returns>
        /// /// <accessComments>
        /// Requires permission USE.FINANCIAL.STATEMENTS.
        /// </accessComments>
        [HttpDelete]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Delete financial statement definition.", HttpMethodDescription = "Delete financial statement definition.")]
        [HeaderVersionRoute("/financial-statement-definitions/{preferenceType}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DeleteFinancialStatementDefinitionsAsync", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<IActionResult> DeleteFinancialStatementDefinitionsAsync(string preferenceType)
        {
            if (string.IsNullOrEmpty(preferenceType))
            {
                logger.LogError("Error deleting financial statement definition due to invalid arguments.");
                return CreateHttpResponseException("Could not delete financial statement definition.");
            }
            try
            {
                await _financialStatementDefinitionsService.DeleteFinancialStatementDefinitionsAsync(preferenceType);
                return NoContent();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, "DeleteFinancialStatementDefinitionsAsync ColleagueSessionExpiredException");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e, "DeleteFinancialStatementDefinitionsAsync PermissionException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting financial statement definition of type " + preferenceType + ".");
                return CreateHttpResponseException("Could not delete financial statement definition.");
            }
        }
    }
}
