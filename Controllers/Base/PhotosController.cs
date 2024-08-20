// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides a API controller for fetching photos.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PhotosController : BaseCompressedApiController
    {
        private readonly IPhotoService photoService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        /// <summary>
        /// injection constructor
        /// </summary>
        /// <param name="photoService">IPhotoService instance.</param>
        /// <param name="apiSettings">ISettingsRepository instance.</param>
        /// <param name="logger">ILogger instance.</param>
        /// <param name="actionContextAccessor"></param>
        public PhotosController(IPhotoService photoService, ApiSettings apiSettings, ILogger logger, IActionContextAccessor actionContextAccessor) : base(actionContextAccessor, apiSettings)
        {
            this.photoService = photoService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the photo configuration.
        /// </summary>
        /// <returns>Bool based on presence of PhotoURL and PhotoType</returns>
        [HttpGet]
        [HeaderVersionRoute("/configuration/photo", 1, true, Name = "GetUserPhotoConfiguration")]
        public IActionResult GetUserPhotoConfiguration()
        {
            try
            {
                var settingsResult = _apiSettings.PhotoConfiguration;
                return Ok(JsonConvert.SerializeObject(settingsResult));
            }
            catch (Exception e)
            {
                logger.LogDebug(e, e.Message);
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Retrieves a person's photo using the provided id.
        /// </summary>
        /// <param name="id">Person's ID</param>
        /// <returns>The photo as a stream. The content-type will be based on the type of image being returned.</returns>
        /// <exception cref="FileNotFoundException">Thrown when a person's photo was not found.</exception>
        /// <accessComments>
        /// A person may view their own photo. Authenticated users with the CAN.VIEW.PERSON.PHOTOS permission can see other people's photos.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/photos/people/{id}", 1, true, Name = "PersonPhoto")]
        public async Task<ActionResult> GetPersonPhoto(string id)
        {
            try
            {
                var repoResult = await photoService.GetPersonPhotoAsync(id);
                var photoBytes = Array.Empty<byte>();
                using (var ms = new MemoryStream())
                {
                    repoResult.PhotoStream.CopyTo(ms);
                    photoBytes = ms.ToArray();
                }
                return File(photoBytes, repoResult.ContentType, repoResult.Filename ?? id);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                logger.LogError(pex.ToString());
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateNotFoundException("Person photo", id);
            }
        }
    }
}
