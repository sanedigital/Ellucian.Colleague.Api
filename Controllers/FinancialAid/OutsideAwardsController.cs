// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net.Http;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Security;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// OutsideAwardsController 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class OutsideAwardsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IOutsideAwardsService outsideAwardsService;
        private readonly ILogger logger;

        /// <summary>
        /// Instantiate a new OutsideAwardsController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="outsideAwardsService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public OutsideAwardsController(IAdapterRegistry adapterRegistry, IOutsideAwardsService outsideAwardsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.outsideAwardsService = outsideAwardsService;
            this.logger = logger;
        }

        /// <summary>
        /// Creates outside award record self-reported by students 
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only"
        /// </accessComments>
        /// <param name="outsideAward"></param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/outside-awards", 1, true, Name = "CreateOutsideAward")]
        public async Task<ActionResult<OutsideAward>> CreateOutsideAwardAsync([FromBody]OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                return CreateHttpResponseException("Create outsideAward object is required in the request body");
            }
            try
            {
                var newOutsideAward = await outsideAwardsService.CreateOutsideAwardAsync(outsideAward);
                return Created(Url.Link("GetOutsideAwards", new { studentId = newOutsideAward.StudentId, year = newOutsideAward.AwardYearCode }), newOutsideAward);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Input outside award is invalid. See log for details.");
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Input outside award is invalid. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Permission denied to create outside award resource. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Exception encountered while creating an outside award resource. See log for details.");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred creating OutsideAward resource. See log for details.");
            }
        }

        /// <summary>
        /// Gets all outside awards for the specified student id and award year code
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve the outside awards</param>
        /// <param name="year">award year code</param>
        /// <returns>List of OutsideAward DTOs</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/outside-awards/{year}", 1, true, Name = "GetOutsideAwards")]
        public async Task<ActionResult<IEnumerable<OutsideAward>>> GetOutsideAwardsAsync([FromRoute] string studentId, [FromRoute] string year)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year cannot be null or empty");
            }

            IEnumerable<OutsideAward> outsideAwards;
            try
            {
                outsideAwards = await outsideAwardsService.GetOutsideAwardsAsync(studentId, year);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Input studentId and/or year are invalid. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Permission denied to retrieve outside awards. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred retrieving outside awards.");
            }
            
            return Ok(outsideAwards);
        }

        /// <summary>
        /// Deletes outside award record with specified record id
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only"
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="id">record id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/students/{studentId}/outside-awards/{id}", Name = "DeleteOutsideAward")]
        public async Task<IActionResult> DeleteOutsideAwardAsync([FromRoute] string studentId, [FromRoute] string id)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId is required");
            }
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException("id is required");
            }
            try
            {
                await outsideAwardsService.DeleteOutsideAwardAsync(studentId, id);
                return NoContent();
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Invalid input arguments. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Permission denied to delete outside award resource. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to locate and delete outside award resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Exception encountered while deleting outside award resource. See log for details.");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred deleting outside award resource. See log for details.");
            }
        }

        /// <summary>
        /// Updates an Outside Award from student entered information.
        /// An Outside Award is defined as an award not given to the student thru the Financial Aid office.
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only"
        /// </accessComments>
        /// <param name="outsideAward">Outside Award Entity</param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/outside-awards", 1, true, Name = "UpdateOutsideAward")]
        public async Task<ActionResult<OutsideAward>> UpdateOutsideAwardAsync([FromBody]OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                return CreateHttpResponseException("Update outsideAward object is required in the request body");
            }
 
            try
            {
                var updatedOutsideAward = await outsideAwardsService.UpdateOutsideAwardAsync(outsideAward);
                return Ok(updatedOutsideAward);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Invalid input arguments. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Permission denied to update outside award resource. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to locate and update outside award resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Exception encountered while updating outside award resource. See log for details.");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred updating outside award resource. See log for details.");
            }
        }

    }
}
