// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Results;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Attachment data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AttachmentsController : BaseCompressedApiController
    {
        private const string encrContentKeyHeader = "X-Encr-Content-Key";
        private const string encrIVHeader = "X-Encr-IV";
        private const string encrTypeHeader = "X-Encr-Type";
        private const string encrKeyIdHeader = "X-Encr-Key-Id";

        private readonly IAttachmentService _attachmentService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AttachmentsController class.
        /// </summary>
        /// <param name="attachmentService">Service of type <see cref="IAttachmentService">IAttachmentService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings">API settings</param>
        /// <param name="actionContextAccessor"></param>
        public AttachmentsController(IAttachmentService attachmentService, ILogger logger, ApiSettings apiSettings, IActionContextAccessor actionContextAccessor) : base(actionContextAccessor, apiSettings)
        {
            _attachmentService = attachmentService;
            this._logger = logger;
        }

        /// <summary>
        /// Get attachments
        /// </summary>
        /// <param name="owner">Owner (optional) to get attachments for</param>
        /// <param name="collectionId">Collection Id (optional) to get attachments for</param>
        /// <param name="tagOne">TagOne value to get attachments for</param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/attachments", 1, true, Name = "GetAttachments")]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachmentsAsync(
            [FromQuery(Name = "owner")] string owner = null,
            [FromQuery(Name = "collectionid")] string collectionId = null,
            [FromQuery(Name = "tagone")] string tagOne = null)
        {
            try
            {
                // get the attachments using filters
                return Ok(await _attachmentService.GetAttachmentsAsync(owner, collectionId, tagOne));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Get the attachment's content
        /// </summary>
        /// <param name="id">Id of the attachment whose content is requested</param>
        /// <returns>A IActionResult with the attachment contents</returns>
        [HttpGet]
        [HeaderVersionRoute("/attachments/{id}/content", 1, true, Name = "GetAttachmentsContent")]
        public async Task<IActionResult> GetAttachmentContentAsync(string id)
        {
            try
            {
                // get the attachment and content
                var attachmentTuple = await _attachmentService.GetAttachmentContentAsync(id);

                // return the encryption metadata, if present, in response headers
                var responseHeaders = new Dictionary<string, string>();
                if (attachmentTuple.Item3 != null)
                {
                    responseHeaders.Add(encrContentKeyHeader, Convert.ToBase64String(attachmentTuple.Item3.EncrContentKey));
                    responseHeaders.Add(encrIVHeader, Convert.ToBase64String(attachmentTuple.Item3.EncrIV));
                    responseHeaders.Add(encrTypeHeader, attachmentTuple.Item3.EncrType);
                    responseHeaders.Add(encrKeyIdHeader, attachmentTuple.Item3.EncrKeyId);
                }

                // return the content
                return new FileContentActionResult(attachmentTuple.Item2, attachmentTuple.Item1.Name,
                    attachmentTuple.Item1.ContentType, responseHeaders);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                return CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Query attachments
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/attachments", 1, true, Name = "QueryAttachmentsByPost")]
        public async Task<ActionResult<IEnumerable<Attachment>>> QueryAttachmentsByPostAsync([FromBody] AttachmentSearchCriteria criteria)
        {
            try
            {
                // get attachments by query criteria
                return Ok(await _attachmentService.QueryAttachmentsAsync(criteria));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Create the new attachment
        /// </summary>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/attachments", 1, true, Name = "PostAttachment")]
        public async Task<ActionResult<Attachment>> PostAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(Request.GetMultipartBoundary()))
                {
                    // attachment metadata and content
                    return await PostAttachmentAndStreamAsync();
                }
                else
                {
                    if (Request.Body == System.IO.Stream.Null)
                    {
                        throw new ArgumentNullException("Content must not be null.");
                    }
                    // attachment metadata only
                    var attachmentDto = JsonConvert.DeserializeObject<Attachment>(new StreamReader(Request.Body).ReadToEnd());

                    return await _attachmentService.PostAttachmentAsync(attachmentDto, GetAttachmentEncryptionMetadata());
                }
            }
            catch (IOException ioe)
            {
                _logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException(ioe.Message);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Update the attachment
        /// </summary>
        /// <param name="id">ID of the attachment to update</param>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to update</param>
        /// <returns>The updated <see cref="Attachment">Attachment</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/attachments/{id}", 1, true, Name = "PutAttachment")]
        public async Task<ActionResult<Attachment>> PutAsync([FromRoute] string id, [FromBody] Attachment attachment)
        {
            try
            {
                return await _attachmentService.PutAttachmentAsync(id, attachment);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                return CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Delete the attachment
        /// </summary>
        /// <param name="id">Id of the attachment to delete</param>
        [HttpDelete]
        [HeaderVersionRoute("/attachments/{id}", 1, true, Name = "DeleteAttachment")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                // delete the attachment
                await _attachmentService.DeleteAttachmentAsync(id);
                return NoContent();
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                return CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        // Get the encryption metadata from the request, if present
        private AttachmentEncryption GetAttachmentEncryptionMetadata()
        {
            AttachmentEncryption attachmentEncryption = null;

            // get the key ID
            string encrKeyId;
            StringValues headerValues;
            if (Request.Headers.TryGetValue(encrKeyIdHeader, out headerValues))
            {
                encrKeyId = headerValues.FirstOrDefault();

                // get the rest of the encryption metadata
                string encrIV = null;
                string encrContentKey = null;
                string encrType = null;
                if (Request.Headers.TryGetValue(encrIVHeader, out headerValues))
                    encrIV = headerValues.FirstOrDefault();
                if (Request.Headers.TryGetValue(encrContentKeyHeader, out headerValues))
                    encrContentKey = headerValues.FirstOrDefault();
                if (Request.Headers.TryGetValue(encrTypeHeader, out headerValues))
                    encrType = headerValues.FirstOrDefault();

                attachmentEncryption = new AttachmentEncryption(encrKeyId, encrType, Convert.FromBase64String(encrContentKey),
                        Convert.FromBase64String(encrIV));
            }

            return attachmentEncryption;
        }

        // Get the attachment metadata, stream, and temp file locations from the request
        private async Task<Attachment> PostAttachmentAndStreamAsync()
        {
            Attachment attachment = null;
            Stream attachmentContentStream = null;
            var attachmentTempFilePaths = new List<string>();

            try
            {
                // verify the request length does not exceed the max size
                var requestSizeHeader = Request.Headers.FirstOrDefault(h => h.Key == "Content-Length");
                if (requestSizeHeader.Value.Any())
                {
                    long requestSize;
                    if (long.TryParse(requestSizeHeader.Value.First(), out requestSize))
                    {
                        if (requestSize > _apiSettings.AttachRequestMaxSize)
                        {
                            _logger.LogError(string.Format("Request size of {0} exceeded max attachment request size of {1}", requestSize,
                                _apiSettings.AttachRequestMaxSize));
                            throw new ArgumentException("Max attachment request size exceeded");
                        }
                    }
                }

                var boundary = MultipartRequestHelper.GetBoundary(
                      MediaTypeHeaderValue.Parse(Request.ContentType),
                       70);
                var reader = new MultipartReader(boundary, Request.Body);
                var section = await reader.ReadNextSectionAsync();
                MultipartSection fileSection = null;
                while (section != null)
                {
                    // determine from the content disposition header what we are working with
                    if (section.GetContentDispositionHeader().DispositionType.Value.Equals("attachment"))
                    {
                        // get the attachment metadata
                        attachment = JsonConvert.DeserializeObject<Attachment>(new StreamReader(section.Body).ReadToEnd());
                    }

                    if (section.GetContentDispositionHeader().DispositionType.Value.Equals("datafile"))
                    {
                        fileSection = section;
                        var filename = section.GetContentDispositionHeader().FileName.ToString();
                        var attachmentTempPath = Path.Combine(_attachmentService.GetAttachTempFilePath(), "temp_attachment_" + Guid.NewGuid());
                        attachmentTempFilePaths.Add(attachmentTempPath);
                        if (!Directory.Exists(Path.GetDirectoryName(attachmentTempPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(attachmentTempPath));
                        }
                        using (FileStream fs = new FileStream(attachmentTempPath, FileMode.OpenOrCreate))
                        {
                            fileSection.Body.CopyTo(fs);
                            fs.Close();
                        }
                    }
                    section = await reader.ReadNextSectionAsync();
                }

                // validate what we have
                if (attachmentTempFilePaths.Count() == 0)
                    throw new ArgumentException("No attachment content found in the request");
                if (attachmentTempFilePaths.Count() > 1)
                    throw new ArgumentException("Multiple attachment content in a single request is not allowed");
                if (attachment == null)
                    throw new ArgumentException("Attachment metadata not found in request");

                // open the file to pass the stream
                var filePath = attachmentTempFilePaths.First();
                attachmentContentStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, useAsync: true);

                // set the attachment size
                attachment.Size = new FileInfo(filePath).Length;

                return await _attachmentService.PostAttachmentAndContentAsync(attachment, GetAttachmentEncryptionMetadata(), attachmentContentStream);
            }
            finally
            {
                if (attachmentContentStream != null)
                {
                    try
                    {
                        attachmentContentStream.Close();
                    }
                    catch (Exception e)
                    {
                        string info = "Could not close stream when creating attachment content";
                        if (attachment != null && !string.IsNullOrEmpty(attachment.Id) && !string.IsNullOrEmpty(attachment.Name))
                            info = string.Format("{0}, id = {1} ({2})", info, attachment.Id, attachment.Name);
                        _logger.LogInformation(e, info);
                    }
                }

                if (attachmentTempFilePaths != null && attachmentTempFilePaths.Any())
                {
                    // delete temp files.  Multiple may have been created if a multi attachment upload was attempted
                    foreach (var tempFile in attachmentTempFilePaths)
                    {
                        if (System.IO.File.Exists(tempFile))
                        {
                            try
                            {
                                System.IO.File.Delete(tempFile);
                            }
                            catch (Exception e)
                            {
                                _logger.LogInformation(e, string.Format("Could not delete temp file {0}", tempFile));
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="lengthLimit"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static bool HasMixedContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: mixed; name="key";
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("mixed");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }
}
