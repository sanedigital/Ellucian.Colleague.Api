// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to organizational positions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]

    public class OrganizationalPositionController : BaseCompressedApiController
    {
        private IOrganizationalPositionService _organizationalPositionService;
        private ILogger _logger;
        /// <summary>
        /// Organizational Position Controller constructor
        /// </summary>
        public OrganizationalPositionController(IOrganizationalPositionService organizationalPositionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _organizationalPositionService = organizationalPositionService;
            this._logger = logger;
        }

        /// <summary>
        /// For the given id, get the Organizational Position
        /// </summary>
        /// <param name="id">Organizational Position id</param>
        /// <returns>Organizational Position DTO</returns>
        /// <accessComments>
        /// User must have the VIEW.ORGANIZATIONAL.RELATIONSHIPS or UPDATE.ORGANIZATIONAL.RELATIONSHIPS permission.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/organizational-positions/{id}", 1, true, Name = "GetOrganizationalPosition")]
        public async Task<ActionResult<Dtos.Base.OrganizationalPosition>> GetOrganizationalPositionAsync(string id)
        {
            try
            {
                var organizationalPosition = await _organizationalPositionService.GetOrganizationalPositionByIdAsync(id);
                return organizationalPosition;
            }
            catch (PermissionsException pe)
            {
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to get Organizational Position: " + id);
                return BadRequest("Unable to get Organizational Position: " + id);
            }

        }
        /// <summary>
        /// For a given list of IDs or search string, returns organizational positions
        /// </summary>
        /// <param name="criteria">Organizational position query criteria</param>
        /// <returns>Matching organizational positions</returns>
        /// <accessComments>
        /// User must have the VIEW.ORGANIZATIONAL.RELATIONSHIPS or UPDATE.ORGANIZATIONAL.RELATIONSHIPS permission.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/organizational-positions", 1, true, Name = "QueryOrganizationalPositions")]
        public async Task<ActionResult<IEnumerable<Dtos.Base.OrganizationalPosition>>> QueryOrganizationalPositionsAsync(OrganizationalPositionQueryCriteria criteria)
        {
            try
            {
                var organizationalPositions = await _organizationalPositionService.QueryOrganizationalPositionsAsync(criteria);
                return Ok(organizationalPositions);
            }
            catch (PermissionsException pe)
            {
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to get Organizational Positions");
                return CreateHttpResponseException("Unable to get Organizational Position", HttpStatusCode.BadRequest);
            }
        }
    }
}
