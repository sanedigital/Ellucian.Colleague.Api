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
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to organizational person positions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OrganizationalPersonPositionsController : BaseCompressedApiController
    {
        private readonly IOrganizationalPersonPositionService _organizationalPersonPositionService;
        private readonly ILogger _logger;

        /// <summary>
        /// OrganizationalPersonPositionsController constructor
        /// </summary>
        /// <param name="organizationalPersonPositionService">Service of type <see cref="IOrganizationalPersonPositionService">IOrganizationalPersonPositionService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public OrganizationalPersonPositionsController(IOrganizationalPersonPositionService organizationalPersonPositionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _organizationalPersonPositionService = organizationalPersonPositionService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the position for the given ID and the direct relationships to others for the position.
        /// </summary>
        /// <returns>OrganizationalPersonPosition for the given ID</returns>
        /// <accessComments>
        /// User must have the VIEW.ORGANIZATIONAL.RELATIONSHIPS or UPDATE.ORGANIZATIONAL.RELATIONSHIPS permission.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/organizational-person-positions/{id}", 1, true, Name = "GetOrganizationalPersonPosition")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>> GetOrganizationalPersonPositionAsync(string id)
        {
            try
            {
                return await _organizationalPersonPositionService.GetOrganizationalPersonPositionByIdAsync(id);
            }
            catch (PermissionsException pe)
            {
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the positions for the indicated persons and the direct relationships to others for each position.
        /// </summary>
        /// <returns>A list of OrganizationalPersonPosition objects</returns>
        /// <accessComments>
        /// User must have the VIEW.ORGANIZATIONAL.RELATIONSHIPS or UPDATE.ORGANIZATIONAL.RELATIONSHIPS permission.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/organizational-person-positions", 1, true, Name = "QueryOrganizationalPersonPositions")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>>> QueryOrganizationalPersonPositionAsync(OrganizationalPersonPositionQueryCriteria criteria)
        {
            try
            {
                return Ok(await _organizationalPersonPositionService.QueryOrganizationalPersonPositionAsync(criteria));
            }
            catch (PermissionsException pe)
            {
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }
    }
}
