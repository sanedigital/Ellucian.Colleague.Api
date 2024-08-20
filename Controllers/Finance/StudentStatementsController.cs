// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Configuration;
using System.Text.RegularExpressions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Finance;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Api;
using Microsoft.AspNetCore.Hosting;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// StudentStatementsController exposes the StudentStatement endpoint
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class StudentStatementsController : BaseCompressedApiController
    {
        private readonly IStudentStatementService statementService;
        private readonly ApiSettings apiSettings;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// StudentStatementsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="statementService">Interface to Student Statement Coordination Service</param>
        /// <param name="logger">Logger</param>
        /// <param name="apiSettings">ERP API Settings</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="webHostEnvironment"></param>
        public StudentStatementsController(IAdapterRegistry adapterRegistry, IStudentStatementService statementService, ILogger logger,
            ApiSettings apiSettings, IActionContextAccessor actionContextAccessor, IWebHostEnvironment webHostEnvironment) : base(actionContextAccessor, apiSettings)
        {
            this.statementService = statementService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.apiSettings = apiSettings;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Get a student's accounts receivable statement as a byte array representation of a PDF file for a timeframe.  
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY 
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="accountHolderId">ID of the student for whom the statement will be generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement will be generated. For example, for Spring 2022 term this would be 2022/SP</param>
        /// <param name="startDate">Date on which the supplied timeframe starts</param>
        /// <param name="endDate">Date on which the supplied timeframe ends</param>
        /// <returns>An IActionResult containing a byte array representing a PDF</returns>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/account-holders/{accountHolderId}/statement/{timeframeId}", 1, true, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetStudentStatement")]
        public async Task<IActionResult> GetStudentStatementAsync(string accountHolderId, string timeframeId, DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(accountHolderId)) return CreateHttpResponseException("Account Holder ID must be specified.");
            if (string.IsNullOrEmpty(timeframeId)) return CreateHttpResponseException("Timeframe ID must be specified.");

            try
            {
                //get Student Statement DTO
                var statementDto = await statementService.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                if (statementDto == null) throw new ApplicationException("Student Statement could not be generated.");

                var reportPath = "";
                //get the path of the .rdlc template
                if (statementDto.ActivityDisplay == Dtos.Finance.Configuration.ActivityDisplay.DisplayByTerm)
                {
                    reportPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports", "Finance", "StudentStatement.frx");
                }
                if (statementDto.ActivityDisplay == Dtos.Finance.Configuration.ActivityDisplay.DisplayByPeriod)
                {
                    reportPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports", "Finance", "StudentStatementPCF.frx");
                }

                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(apiSettings.ReportLogoPath))
                {
                    reportLogoPath = apiSettings.ReportLogoPath;
                    reportLogoPath = System.IO.Path.Combine(webHostEnvironment.WebRootPath, reportLogoPath);
                }

                    var resourceFilePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Resources", "Finance", "StatementResources.json");

                //generate the PDF based on the StudentStatement DTO
                var renderedBytes = statementService.GetStudentStatementReport(statementDto, reportPath, resourceFilePath, 
                    reportLogoPath);

                //create and return the HTTP response object

                var fileNameString = "StudentStatement" + " " + accountHolderId + " " + timeframeId;
                return File(renderedBytes, "application/pdf", fileNameString + ".pdf");

            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Student Statement query parameters are not valid. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to Student Statement is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of Student Statement resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Student Statement could not be generated. See log for details.");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentStatement resource. See log for details.");
            }
        }
    }
}
