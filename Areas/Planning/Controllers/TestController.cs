// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Areas.Planning.Models.Tests;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace Ellucian.Colleague.Api.Areas.Planning.Controllers
{
    /// <summary>
    /// Provides the test utilities for the planning area.
    /// </summary>
    [LocalRequest]
    [Area("Planning")]
    public class TestController : Controller
    {
        /// <summary>
        /// Person id string.
        /// </summary>
        public static string PersonIdKey = "PersonId";

        private readonly ILogger _logger;
        private readonly JwtHelper _jwtHelper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jwtHelper"></param>
        /// <param name="logger"></param>
        public TestController(JwtHelper jwtHelper, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(jwtHelper);
            _jwtHelper = jwtHelper;
            _logger = logger;
        }


        /// <summary>
        /// Gets the test index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var tests = new ApiTests();
            tests.Tests.Add("subjects");
            return View(tests);
        }

        /// <summary>
        /// Gets the test degree plan index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult DegreePlan()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the degree plan test.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostDegreePlan()
        {
            return Run(() =>
            {
                var client = CreateClient();
                var personId = (string)HttpContext.Items[PersonIdKey];
                try
                {
                    var student = client.GetPlanningStudentAsync(personId).Result;
                    if (student != null && student.DegreePlanId.HasValue)
                    {
                        var results = client.GetDegreePlan6Async(student.DegreePlanId.Value.ToString()).Result;
                        return "Degree Plan Id " + results.DegreePlan.Id;
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                return "Degree Plan does not exist for person " + personId;
            });
        }

        /// <summary>
        /// Gets the test advisees index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Advisees()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the advisees test.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostAdvisees()
        {
            return Run(() =>
            {
                var results = CreateClient().GetAdviseesAsync((string)HttpContext.Items[PersonIdKey], int.MaxValue, 1, true).Result;
                return results.Count() + " assigned advisees";
            });
        }

        /// <summary>
        /// Gets the test evaluation page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Evaluation()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the evaluation test.
        /// </summary>
        /// <param name="evaluation"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Evaluation(TestEvaluation evaluation)
        {
            if (evaluation == null || string.IsNullOrWhiteSpace(evaluation.Program))
            {
                throw new ArgumentNullException("evaluation", "a program is required for evaluation.");
            }

            return Run(() =>
            {
                var result = CreateClient().GetProgramEvaluation((string)HttpContext.Items[PersonIdKey], evaluation.Program.ToUpper());
                return result.RequirementResults.Count + " requirement results";
            });
        }

        /// <summary>
        /// Gets the done page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Done()
        {
            ViewBag.Result = TempData["result"];
            ViewBag.Elapsed = TempData["elapsed"];
            ViewBag.ExceptionMessage = TempData["exception"];
            ViewBag.Log = TempData["log"];
            return View();
        }

        private ColleagueApiClient CreateClient()
        {
            var cookieValue = LocalUserUtilities.GetCookie(Request);
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new ColleagueWebApiException("Log in first");
            }
            var baseUrl = cookieValue.Split('*')[0];
            var token = cookieValue.Split('*')[1];
            var principal = JwtHelper.CreatePrincipal(token);
            var personId = (principal as ClaimsPrincipal).Identities.First().Claims.First(claim => claim.Type == ClaimConstants.PersonId).Value;
            HttpContext.Items[PersonIdKey] = personId;
            var client = new ColleagueApiClient(baseUrl, _logger);
            client.Credentials = token;
            return client;
        }

        private static List<string> GetDelimited(string input)
        {
            if (input == null)
            {
                return new List<string>();
            }
            if (input.Contains(','))
            {
                return input.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            return input.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private ActionResult Run(Func<string> action)
        {
            var sw = new Stopwatch();
            sw.Start();
            string result = "";
            try
            {
                result = action.Invoke();
            }
            catch (Exception ex)
            {
                TempData["exception"] = ex.ToString();
            }
            sw.Stop();
            TempData["result"] = result;
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            TempData["log"] = string.Empty;

            return RedirectToAction("Done");
        }
    }
}
