// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// 
    /// </summary>

    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class LoadPeriodsController : BaseCompressedApiController
    {
        private readonly ILoadPeriodService _loadPeriodService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the LoadPeriodsController class.
        /// </summary>
        /// <param name="loadPeriodService">Service of type <see cref="ILoadPeriodService">ILoadPeriodService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public LoadPeriodsController(ILoadPeriodService loadPeriodService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _loadPeriodService = loadPeriodService;
            this._logger = logger;
        }

        /// <summary>
        /// Query Load Periods
        /// </summary>
        /// <param name="loadPeriodQueryCriteria">Load Period Query Criteria</param>
        /// <returns>Load periods</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/load-periods", 1, true, Name = "QueryLoadPeriodsAsync")]
        public async Task<ActionResult<IEnumerable<LoadPeriod>>> QueryLoadPeriodsAsync(LoadPeriodQueryCriteria loadPeriodQueryCriteria)
        {
            if(loadPeriodQueryCriteria == null)
            {
                //Uncaught exceptions will return a 500 error = bad, this returns 400 which is default
                return CreateHttpResponseException("Load period query criteria required to query load period");
            }

            //Controllers only talk in DTOs so do not have to specify in var name
            try
            {
                var loadPeriods = await _loadPeriodService.GetLoadPeriodsByIdsAsync(loadPeriodQueryCriteria.Ids);
                return Ok(loadPeriods);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to retrieve load periods");
                return CreateHttpResponseException("Failed to retrieve load periods");
            }
            
        }
    }
}
