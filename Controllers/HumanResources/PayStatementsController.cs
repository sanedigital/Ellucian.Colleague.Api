// Copyright 2017-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Pay Statements Controller routes requests for interacting with pay statements
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayStatementsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPayStatementService payStatementService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="payStatementService">pay statement service</param>
        /// <param name="apiSettings">api settings</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="webHostEnvironment"></param>
        public PayStatementsController(ILogger logger, IPayStatementService payStatementService, ApiSettings apiSettings,
            IActionContextAccessor actionContextAccessor, IWebHostEnvironment webHostEnvironment) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.payStatementService = payStatementService;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets a list of summaries of all pay statements for the current user, or, if you have the proper permissions, for any
        /// employee.
        /// A summary object corresponds to a pay statement record. You can use the id of the PayStatement in a request for a
        /// Pay Statement Report.
        /// Use the filter start and end dates to filter out 
        /// A successful request will return a status code 200 response and a list of pay statement summary object.    
        /// </summary>
        /// <param name="employeeId">optional: the employee identifier</param>
        /// <param name="hasOnlineConsent">optional: whether the employee has consented to viewing statements online</param>
        /// <param name="payCycleId">optional: the pay cycle in which the employee was paid that generated this statement</param>
        /// <param name="payDate">optional: the date the employee was paid</param>
        /// <param name="startDate">optional: start date to filter pay statements by</param>
        /// <param name="endDate">optional: end date to filter pay statements by</param>
        /// <returns>An array of PayStatementSummary objects for the requested parameters</returns>
        /// <note>Pay Statements are cached for 24 hours.</note>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService), primaryGuidParameters: new[] {"employeeId"})]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/pay-statements", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayStatementSummariesAsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/pay-statements", 1, false, Name = "GetPayStatementSummariesAsync")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a list of summaries of all pay statements for the current user, or, if you have the proper permissions, for any employee.",
            HttpMethodDescription = "Gets a list of summaries of all pay statements for the current user, or, if you have the proper permissions, for any employee.")]
        public async Task<ActionResult<IEnumerable<PayStatementSummary>>> GetPayStatementSummariesAsync(
            [FromQuery(Name = "employeeId")] string employeeId = null,
            [FromQuery(Name = "hasOnlineConsent")] bool? hasOnlineConsent = null,
            [FromQuery(Name = "payDate")] DateTime? payDate = null,
            [FromQuery(Name = "payCycleId")] string payCycleId = null,
            [FromQuery(Name = "startDate")] DateTime? startDate = null,
            [FromQuery(Name = "endDate")] DateTime? endDate = null)
        {
            try
            {
                logger.LogDebug("************Start- Process to get pay statement summary - Start************");
                var paystatementsummary = await payStatementService.GetPayStatementSummariesAsync(
                    string.IsNullOrEmpty(employeeId) ? null : new List<string>() { employeeId },
                    hasOnlineConsent,
                    payDate,
                    payCycleId,
                    startDate,
                    endDate
                );
                logger.LogDebug("************End- Process to pay statement summary - End************");

                return Ok(paystatementsummary);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Invalid arguments at some level of the request", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Current user does not have permission to view requested data", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query multiple Pay Statement PDFs for the given ids
        /// </summary>
        /// <param name="ids">a list of Pay Statement ids to generate into a single PDF</param>
        /// <returns>a single PDF containing the pay statements specified in the list of ids</returns>
        [HttpPost]
        [HeaderVersionRoute("/pay-statements", 1, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "QueryMultiplePayStatementPdfs")]
        public async Task<IActionResult> QueryPayStatementPdfs([FromBody] IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return CreateHttpResponseException("ids are required in request body");
            }

            try
            {
                logger.LogDebug("************Start- Process to generate multiple PDF statement - Start************");
                var pathToReportTemplate = System.IO.Path.Join(webHostEnvironment.ContentRootPath, "Reports/HumanResources/PayStatement.frx");
                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(_apiSettings.ReportLogoPath))
                {
                    reportLogoPath = _apiSettings.ReportLogoPath;
                    reportLogoPath = System.IO.Path.Join(webHostEnvironment.WebRootPath, reportLogoPath);
                }

                var reportBytes = await payStatementService.GetPayStatementPdf(ids, pathToReportTemplate, reportLogoPath);

                logger.LogDebug("************End - Process to generate multiple PDF statement - End************");
                return File(reportBytes, "application/pdf", "Multiple");

            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Invalid arguments at some level of the request", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Current user does not have permission to view requested data", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get Pay Statement PDF for the given id. The requested statement must 
        /// be owned by the authenticated user, or the user must have the proper permissions.
        /// </summary>
        /// <param name="id">The id of the requested Pay Statement</param>
        /// <returns>An HttpResponseMessage with the Content property containing a byte[] of the PDF</returns>
        /// <note>PayStatementSummary is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/pay-statements/{id}", 1, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetPayStatementPdf")]
        public async Task<IActionResult> GetPayStatementPdf(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.LogDebug("************The id of the requested Pay Statement is null or empty************");
                return CreateHttpResponseException("id is required in request");
            }
            try
            {
                logger.LogDebug("************Start - Paystatement PDF process - Start************");
                var pathToReportTemplate = System.IO.Path.Join(webHostEnvironment.ContentRootPath, "Reports/HumanResources/PayStatement.frx");
                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(_apiSettings.ReportLogoPath))
                {
                    reportLogoPath = _apiSettings.ReportLogoPath;
                    reportLogoPath = System.IO.Path.Join(webHostEnvironment.WebRootPath, reportLogoPath);
                }

                var reportTuple = await payStatementService.GetPayStatementPdf(id, pathToReportTemplate, reportLogoPath);
                logger.LogDebug("************End -Paystatement PDF process ends successfully - End ************");
                return File(reportTuple.Item2, "application/pdf", reportTuple.Item1);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Invalid arguments at some level of the request", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Current user does not have permission to view requested data", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        #region Experience end-points
        /// <summary>
        /// This ethos end-point will return the pay statement information for a given id.
        /// The requested pay statement must be owned by the authenticated user.
        /// </summary>
        /// <param name="id">The id of the requested pay statement.</param>      
        /// <returns>The requested PayStatementInformation DTO</returns>

        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/pay-statement-info/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayStatementInformationAsyncV1.0.0", IsEthosEnabled = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "This ethos end-point will return the pay statement information for a given id.",
           HttpMethodDescription = "This ethos end-point will return the pay statement information for a given id.")]
        //ProCode API for Experience
        public async Task<ActionResult<PayStatementInformation>> GetPayStatementInformationAsync(string id)
        {
            logger.LogDebug("********* Start - Process to get pay statement information - Start *********");
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    logger.LogDebug("id cannot be null");
                    throw new ArgumentNullException("id", "id cannot be null");
                }

                var payStatementInformation = await payStatementService.GetPayStatementInformationAsync(id);
                logger.LogDebug("********* End - Process to get pay statement information - End *********");
                return payStatementInformation;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ApplicationException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>        
        /// Get all PayStatementInformation
        /// </summary>
        /// <returns>PayStatementInformation objects</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/pay-statement-info", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllPayStatementInformationAsyncV1.0.0", IsEedmSupported = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "This ethos end-point will return all the pay statement information.",
          HttpMethodDescription = "This ethos end-point will return all the pay statement information.")]
        public async Task<ActionResult<IEnumerable<PayStatementInformation>>> GetAllPayStatementInformationAsync()
        {
            // GetAll is not supported for Colleague/Experience but ethos requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}
