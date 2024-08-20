// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Api.Controllers.ResidenceLife
{
    /// <summary>
    /// APIs related to meal plan assignments.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class MealPlanAssignmentsController : BaseCompressedApiController
    {
        private readonly IMealPlanAssignmentService mealPlanAssignmentService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for the MealPlanAssignment Controller
        /// </summary>
        /// <param name="mealPlanAssignmentService"></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MealPlanAssignmentsController(IMealPlanAssignmentService mealPlanAssignmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.mealPlanAssignmentService = mealPlanAssignmentService;
            this._logger = logger;
        }

        /// <summary>
        /// Get a meal plan assignment by the system id
        /// </summary>
        /// <param name="id">The system id of the meal plan assignment</param>
        /// <returns>The resulting meal plan assignment</returns>
        /// <accessComments>
        /// Requires permission VIEW.MEAL.PLAN.ASSIGNMENTS. 
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/meal-plan-assignments/{id}", 1, false, Name = "GetMealPlanAssignment")]
        public ActionResult<MealPlanAssignment> GetMealPlanAssignment(string id)
        {
            try
            {
                return Ok(mealPlanAssignmentService.GetMealPlanAssignment(id));
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
        /// Create a new meal plan assignment in the target system.
        /// </summary>
        /// <param name="mealPlanAssignment">The meal plan assignment to create</param>
        /// <returns>The resulting meal plan assignment</returns>
        [HttpPost]
        [HeaderVersionRoute("/meal-plan-assignments", 1, false, Name = "CreateMealPlanAssignment")]
        public ActionResult<MealPlanAssignment> PostMealPlanAssignment(MealPlanAssignment mealPlanAssignment)
        {
            try
            {
                return Ok(mealPlanAssignmentService.CreateMealPlanAssignment(mealPlanAssignment));
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
        /// Update an existing meal plan assignment in the target system.
        /// The existing meal plan assignment must be specified by Id and/or External Id.
        /// All attributes of the existing meal plan assignment will be overwritten with the attributes
        /// supplied to this API. An attribute that is supported, but is not supplied to
        /// the API, will be treated as a blank value and overwrite the corresponding
        /// attribute in the existing record with a blank.
        /// </summary>
        /// <param name="mealPlanAssignment">The new housing assignment data</param>
        /// <returns>The resulting housing assignment</returns>
        [HttpPut]
        [HeaderVersionRoute("/meal-plan-assignments", 1, false, Name = "UpdateMealPlanAssignment")]
        public ActionResult<MealPlanAssignment> PutMealPlanAssignment(MealPlanAssignment mealPlanAssignment)
        {
            try
            {
                return Ok(mealPlanAssignmentService.UpdateMealPlanAssignment(mealPlanAssignment));
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
