// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System;
using System.Net;

using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.ResidenceLife;
using Ellucian.Colleague.Coordination.ResidenceLife.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.ResidenceLife
{
    /// <summary>
    /// APIs related to housing assignments.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class HousingAssignmentsController : BaseCompressedApiController
    {

        private readonly IHousingAssignmentService housingAssignmentService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for the HousingAssignment Controller
        /// </summary>
        /// <param name="housingAssignmentService"></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public HousingAssignmentsController(IHousingAssignmentService housingAssignmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.housingAssignmentService = housingAssignmentService;
            this._logger = logger;
        }

        /// <summary>
        /// Create a new housing assignment in the target system.
        /// </summary>
        /// <param name="housingAssignment">The housing assignment to create</param>
        /// <param name="boxNumberAlreadyFormatted">Set to true if the box number supplied in the AddressBoxNumber attribute is already formatted. Otherwise set to false or do not include the parameter. If true, Colleague will not apply its own formatting to the supplied value, such as adding the word box as a prefix.</param>
        /// <param name="removeBoxNumber">If true, Colleague will not automatically create a box number if configured to do so. There will be no box number for the created housing assignment. If passed as false or not included, this parameter has no effect. This cannot be set to true if a value is supplied in the AddressBoxNumber attribute. This cannot be set to true unless Colleague is configured to maintain addresses with housing assignments.</param>
        /// <returns>The resulting housing assignment</returns>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.HOUSING.ASSIGNMENTS. The user will be able to create housing assignments for all people.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/housing-assignments", 1, false, Name = "CreateHousingAssignment")]
        public async Task<ActionResult<HousingAssignment>> PostHousingAssignmentAsync(HousingAssignment housingAssignment, bool? boxNumberAlreadyFormatted = null, bool? removeBoxNumber = null)
        {
            try
            {
                return await housingAssignmentService.CreateHousingAssignmentAsync(housingAssignment, boxNumberAlreadyFormatted, removeBoxNumber);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(string.Empty, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update an existing housing assignment in the target system.
        /// The existing housing assignment must be specified by Id and/or External Id.
        /// All attributes of the existing housing assignment will be overwritten with the attributes
        /// supplied to this API. An attribute that is supported, but is not supplied to
        /// the API, will be treated as a blank value and overwrite the corresponding
        /// attribute in the existing record with a blank, with the exception of Box Number.
        /// A blank Box Number is ignored, and separate "Remove Box Number" flag is available.
        /// </summary>
        /// <param name="housingAssignment">The new housing assignment data</param>
        /// <param name="boxNumberAlreadyFormatted">Set to true if the box number supplied in the AddressBoxNumber attribute is already formatted. Otherwise set to false or do not include the parameter. If true, Colleague will not apply its own formatting to the supplied value, such as adding the word box as a prefix.</param>
        /// <param name="removeBoxNumber">Set to true to remove the existing box number on the housing assignment. Otherwise set to false or do not include the parameter. This cannot be set to true if a value is supplied in the AddressBoxNumber attribute. This cannot be set to true unless Colleague is configured to maintain addresses with housing assignments.</param>
        /// <returns>The resulting housing assignment</returns>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.HOUSING.ASSIGNMENTS. The user will be able to update housing assignments for all people.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/housing-assignments", 1, false, Name = "UpdateHousingAssignment")]
        public async Task<ActionResult<HousingAssignment>> PutHousingAssignmentAsync(HousingAssignment housingAssignment, bool? boxNumberAlreadyFormatted = null, bool? removeBoxNumber = null)
        {
            try
            {
                return await housingAssignmentService.UpdateHousingAssignmentAsync(housingAssignment, boxNumberAlreadyFormatted, removeBoxNumber);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(string.Empty, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a housing assignment by the system id
        /// </summary>
        /// <param name="id">The system id of the housing assignment</param>
        /// <returns>The resulting housing assignment</returns>
        /// <accessComments>
        /// Requires permission VIEW.HOUSING.ASSIGNMENTS. The user will be able to view housing assignments for all people.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/housing-assignments/{id}", 1, false, Name = "GetHousingAssignment")]
        public async Task<ActionResult<HousingAssignment>>  GetHousingAssignmentAsync(string id)
        {
            try
            {
                return await housingAssignmentService.GetHousingAssignmentAsync(id);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(string.Empty, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
