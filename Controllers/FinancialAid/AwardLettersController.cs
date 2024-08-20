// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;
using Ellucian.Colleague.Api;
using Microsoft.AspNetCore.Hosting;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// AwardLettersController exposes actions to interact with AwardLetter resources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardLettersController : BaseCompressedApiController
    {
        private readonly IAwardLetterService awardLetterService;

        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly ApiSettings apiSettings;
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// AwardLettersController constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="awardLetterService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="webHostEnvironment"></param>
        public AwardLettersController(IAdapterRegistry adapterRegistry, IAwardLetterService awardLetterService,
            ILogger logger, ApiSettings apiSettings, IActionContextAccessor actionContextAccessor, IWebHostEnvironment webHostEnvironment) : base(actionContextAccessor, apiSettings)
        {
            this.awardLetterService = awardLetterService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.apiSettings = apiSettings;
            this.webHostEnvironment = webHostEnvironment;
        }

        #region Obsolete methods

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.9, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-letters", 1, false, Name = "GetAwardLetters", Order = 100)]
        public ActionResult<IEnumerable<AwardLetter>> GetAwardLetters(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(awardLetterService.GetAwardLetters(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Award letter objects might contain no awards if none found
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.10, use version 3 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-letters", 2, false, Name = "GetAwardLetters2", Order = 100)]
        public ActionResult<IEnumerable<AwardLetter>> GetAwardLetters2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(awardLetterService.GetAwardLetters2(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter DTO object.</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.9, use version 2 of this API")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 1, false, Name = "GetAwardLetter", Order = 100)]
        public ActionResult<AwardLetter> GetAwardLetter(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return Ok(awardLetterService.GetAwardLetters(studentId, awardYear));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year. Award letter is returned even if no awards are
        /// associated with the letter
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter DTO object.</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.10, use version 3 of this API")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 2, false, Name = "GetAwardLetter2", Order = 100)]
        public ActionResult<AwardLetter> GetAwardLetter2(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return Ok(awardLetterService.GetAwardLetters2(studentId, awardYear));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.  
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get award letter data</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.9, use version 2 of this API")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 1, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetAwardLetterReport", Order = 90)]
        public IActionResult GetAwardLetterReport(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = awardLetterService.GetAwardLetters(studentId, awardYear);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                //get Path of the .rdlc template
                var reportPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/FinancialAid/AwardLetter.rdlc");

                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(apiSettings.ReportLogoPath))
                {
                    reportLogoPath = apiSettings.ReportLogoPath;
                    reportLogoPath = System.IO.Path.Combine(webHostEnvironment.WebRootPath, reportLogoPath);
                }

                //generate the pdf based on the award letter DTO
                var renderedBytes = awardLetterService.GetAwardLetters(awardLetterDto, reportPath, reportLogoPath);

                var fileName = Regex.Replace(
                     ("AwardLetter" + " " + studentId + " " + awardYear + " " + awardLetterDto.Date.ToShortDateString()),
                     "[^a-zA-Z0-9_]",
                     "_")
                     + ".pdf";
                return File(renderedBytes, "application/pdf", fileName);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.
        /// An award letter object that is used to create the report might come back with no awards if
        /// none were found for the year.
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get award letter data</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.10, use GetAwardLetterReport3Async")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 2, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetAwardLetterReport2", Order = -5)]
        public IActionResult GetAwardLetterReport2(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = awardLetterService.GetAwardLetters2(studentId, awardYear);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                //get Path of the .rdlc template
                var reportPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/FinancialAid/AwardLetter.rdlc");

                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(apiSettings.ReportLogoPath))
                {
                    reportLogoPath = apiSettings.ReportLogoPath;
                   
                    reportLogoPath = System.IO.Path.Combine(webHostEnvironment.WebRootPath, reportLogoPath);
                }

                //generate the pdf based on the award letter DTO
                var renderedBytes = awardLetterService.GetAwardLetters(awardLetterDto, reportPath, reportLogoPath);

                var fileName = Regex.Replace(
                    ("AwardLetter" + " " + studentId + " " + awardYear + " " + awardLetterDto.Date.ToShortDateString()),
                    "[^a-zA-Z0-9_]",
                    "_")
                    + ".pdf";

                return File(renderedBytes, "application/pdf", fileName);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Update a student's award letter. This update permits changes to the award letter's AcceptedDate
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Student's Colleague PERSON id. Must match awardLetter's studentId</param>
        /// <param name="awardYear">AwardYear of award letter to update. Must match awardLetter's awardYear</param>
        /// <param name="awardLetter">AwardLetter DTO containing data which which to update the database</param>
        /// <returns>An updated AwardLetter DTO</returns>
        [HttpPut]
        [Obsolete("Obsolete as of Api version 1.10, use version 2 of this API")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 1, false, Name = "UpdateAwardLetter")]
        public IActionResult UpdateAwardLetter([FromRoute] string studentId, [FromRoute] string awardYear, [FromBody] AwardLetter awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }
            if (awardLetter == null)
            {
                return CreateHttpResponseException("awardLetter cannot be null");
            }
            if (awardYear != awardLetter.AwardYearCode)
            {
                var message = string.Format("AwardYear {0} in URI does not match AwardYear {1} of awardLetter in request body", awardYear, awardLetter.AwardYearCode);
                logger.LogError(message);
                return CreateHttpResponseException(message);
            }

            try
            {
                return Ok(awardLetterService.UpdateAwardLetter(awardLetter));
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("awardLetter in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to update AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, oce.Message);
                return CreateHttpResponseException("AwardLetter Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }

        }


        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Award letter objects might contain no awards if none found
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetters4Async")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-letters", 3, false, Name = "GetAwardLetters3Async")]
        public async Task<ActionResult<IEnumerable<AwardLetter2>>> GetAwardLetters3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(await awardLetterService.GetAwardLetters3Async(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year. Award letter is returned even if no awards are
        /// associated with the letter
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter DTO object.</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetter4Async")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 3, false, Name = "GetAwardLetter3Async", Order = -5)]
        public async Task<ActionResult<AwardLetter2>> GetAwardLetter3Async(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return Ok(await awardLetterService.GetAwardLetter3Async(studentId, awardYear));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve the report</param>        
        /// <param name="awardLetterId">id of the award letter history record</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetterReport4Async")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardLetterId}", 3, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetAwardLetterReport3Async")]
        public async Task<IActionResult> GetAwardLetterReport3Async(string studentId, string awardLetterId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardLetterId))
            {
                return CreateHttpResponseException("awardLetterId cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = await awardLetterService.GetAwardLetterByIdAsync(studentId, awardLetterId);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                //get Path of the .rdlc template
                var reportPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/FinancialAid/AwardLetter2.rdlc");

                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(apiSettings.ReportLogoPath))
                {
                    reportLogoPath = apiSettings.ReportLogoPath;
                   
                    reportLogoPath = System.IO.Path.Combine(webHostEnvironment.WebRootPath, reportLogoPath);
                }

                //generate the pdf based on the award letter DTO
                var renderedBytes = await awardLetterService.GetAwardLetterReport3Async(awardLetterDto, reportPath, reportLogoPath);
                
                var fileName = Regex.Replace(
                ("AwardLetter" + " " + studentId + " " + awardLetterDto.AwardLetterYear + " " + awardLetterDto.CreatedDate.Value.ToShortDateString()),
                "[^a-zA-Z0-9_]",
                "_")
                + ".pdf";
                return File(renderedBytes, "application/pdf", fileName);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Update a student's award letter. This update permits changes to the award letter's AcceptedDate
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Student's Colleague PERSON id. Must match awardLetter's studentId</param>
        /// <param name="awardYear">AwardYear of award letter to update. Must match awardLetter's awardYear</param>
        /// <param name="awardLetter">AwardLetter DTO containing data which which to update the database</param>
        /// <returns>An updated AwardLetter DTO</returns>
        [HttpPut]
        [Obsolete("Obsolete as of Api version 1.22, use UpdateAwardLetter3Async")]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 2, false, Name = "UpdateAwardLetter2")]
        public async Task<ActionResult<AwardLetter2>> UpdateAwardLetter2Async([FromRoute] string studentId, [FromRoute] string awardYear, [FromBody] AwardLetter2 awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }
            if (awardLetter == null)
            {
                return CreateHttpResponseException("awardLetter cannot be null");
            }
            if (awardYear != awardLetter.AwardLetterYear)
            {
                var message = string.Format("AwardYear {0} in URI does not match AwardYear {1} of awardLetter in request body", awardYear, awardLetter.AwardLetterYear);
                logger.LogError(message);
                return CreateHttpResponseException(message);
            }

            try
            {
                return Ok(await awardLetterService.UpdateAwardLetter2Async(awardLetter));
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("awardLetter in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to update AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, oce.Message);
                return CreateHttpResponseException("AwardLetter Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }

        }

        #endregion

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Award letter objects might contain no awards if none found
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-letters", 4, true, Name = "GetAwardLetters4Async")]
        public async Task<ActionResult<IEnumerable<AwardLetter3>>> GetAwardLetters4Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(await awardLetterService.GetAwardLetters4Async(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year. Award letter is returned even if no awards are
        /// associated with the letter
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter3 DTO object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 4, true, Name = "GetAwardLetter4Async", Order = -5)]
        public async Task<ActionResult<AwardLetter3>> GetAwardLetter4Async(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return await awardLetterService.GetAwardLetter4Async(studentId, awardYear);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve the report</param>        
        /// <param name="awardLetterId">id of the award letter history record</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardLetterId}", 4, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetAwardLetterReport4Async")]
        public async Task<IActionResult> GetAwardLetterReport4Async(string studentId, string awardLetterId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardLetterId))
            {
                return CreateHttpResponseException("awardLetterId cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = await awardLetterService.GetAwardLetterById2Async(studentId, awardLetterId);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                var reportPath = "";
                //get Path of the .frx template
                if (awardLetterDto.AwardLetterHistoryType == "OLTR")
                {
                    reportPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports", "FinancialAid", "OfferLetter.frx");
                    //reportPath = System.IO.Path.Join(webHostEnvironment.ContentRootPath, "Reports/FinancialAid/OfferLetter.frx");
                }
                else
                {
                    reportPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports", "FinancialAid", "AwardLetter.frx");
                    //reportPath = System.IO.Path.Join(webHostEnvironment.ContentRootPath, "Reports/FinancialAid/AwardLetter.frx");
                }
                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(apiSettings.ReportLogoPath))
                {
                    reportLogoPath = apiSettings.ReportLogoPath;

                    //reportLogoPath = System.IO.Path.Combine(webHostEnvironment.WebRootPath, reportLogoPath);
                    reportLogoPath = System.IO.Path.Combine(webHostEnvironment.WebRootPath, reportLogoPath);
                }

                //generate the pdf based on the award letter DTO
                var renderedBytes = await awardLetterService.GetAwardLetterReport4Async(awardLetterDto, reportPath, reportLogoPath);

                var fileName = Regex.Replace("AwardLetter" + " " + studentId + " " + awardLetterDto.AwardLetterYear + " " + awardLetterDto.CreatedDate.Value.ToShortDateString(),
                "[^a-zA-Z0-9_]",
                "_") + ".pdf";

                return File(renderedBytes, "application/pdf", fileName);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Update a student's award letter. This update permits changes to the award letter's AcceptedDate
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Student's Colleague PERSON id. Must match awardLetter's studentId</param>
        /// <param name="awardYear">AwardYear of award letter to update. Must match awardLetter's awardYear</param>
        /// <param name="awardLetter">AwardLetter3 DTO containing data which which to update the database</param>
        /// <returns>An updated AwardLetter3 DTO</returns>
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/award-letters/{awardYear}", 3, true, Name = "UpdateAwardLetter3")]
        public async Task<ActionResult<AwardLetter3>> UpdateAwardLetter3Async([FromRoute] string studentId, [FromRoute] string awardYear, [FromBody] AwardLetter3 awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear cannot be null or empty");
            }
            if (awardLetter == null)
            {
                return CreateHttpResponseException("awardLetter cannot be null");
            }
            if (awardYear != awardLetter.AwardLetterYear)
            {
                var message = string.Format("AwardYear {0} in URI does not match AwardYear {1} of awardLetter in request body", awardYear, awardLetter.AwardLetterYear);
                logger.LogError(message);
                return CreateHttpResponseException(message);
            }

            try
            {
                return await awardLetterService.UpdateAwardLetter3Async(awardLetter);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("awardLetter in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to AwardLetter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to update AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, oce.Message);
                return CreateHttpResponseException("AwardLetter Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }

        }

    }
}
