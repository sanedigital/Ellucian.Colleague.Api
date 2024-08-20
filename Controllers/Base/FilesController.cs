// Copyright 2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Results;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to report files
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class FilesController : BaseCompressedApiController
    {
        private readonly IReportFileService reportFileService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes the FilesController
        /// </summary>
        /// <param name="reportFileService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FilesController(IReportFileService reportFileService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.reportFileService = reportFileService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a report file from the server
        /// </summary>
        /// <param name="location">Location key</param>
        /// <param name="fileName">File name</param>
        /// <accessComments>User can only retrieve files from locations that are accessible to their security class.</accessComments>
        /// <returns>The file contents. Content-Type will always be application/octet-stream</returns>
        [HttpGet]
        [HeaderVersionRoute("/files/{location}/{fileName}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFiles", IsEedmSupported = true)]
        [Metadata(ApiVersionStatus = "B", HttpMethodSummary = "Retrieves a report file from the server.",
           HttpMethodDescription = "Retrieves a report file from the server. User must have security class directory access to the location requested.")]
        public async Task<IActionResult> GetFilesAsync([FromRoute] string location, [FromRoute] string fileName)
        {
            try
            {
                var downloadPath = await reportFileService.GetReportFileAsync(location, fileName);
                var contentType = "application/octet-stream";
                var headers = new System.Collections.Generic.Dictionary<string, string>();

                var fs = new FileStream(downloadPath, FileMode.Open, FileAccess.Read, FileShare.None, 4096,
                         FileOptions.Asynchronous | FileOptions.DeleteOnClose);

                var fileContentResult = new FileStreamResult(fs, contentType)
                {
                    FileDownloadName = fileName
                };
                return fileContentResult;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.NotFound);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException(statusCode: HttpStatusCode.InternalServerError);
            }
        }
    }
}
