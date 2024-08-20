// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Areas.Student.Models.Tests;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;

namespace Ellucian.Colleague.Api.Areas.Student.Controllers
{
    /// <summary>
    /// Provides the test utilities for the student area.
    /// </summary>
    [LocalRequest]
    [Area("Student")]
    public class TestController : Controller
    {
        /// <summary>
        /// Person id string.
        /// </summary>
        public static string PersonIdKey = "PersonId";

        private ILogger _logger;

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="logger"></param>
        public TestController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var tests = new ApiTests();
            tests.Tests.Add("subjects");
            return View(tests);
        }

        /// <summary>
        /// Gets the subject test index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Subjects()
        {
            return View();
        }

        /// <summary>
        /// Submits a subject test.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostSubjects()
        {
            return Run(() =>
            {
                var results = CreateClient().GetSubjectsAsync().Result;
                return results.Count() + " Subjects returned";
            });
        }

        /// <summary>
        /// Gets the sections for courses test index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult SectionsForCourses()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to test getting sections for pages.
        /// </summary>
        /// <param name="sectionsForCourses"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SectionsForCourses(TestSectionsForCourses sectionsForCourses)
        {
            return Run(() =>
            {
                var results = CreateClient().GetSectionsByCourse3Async(GetDelimited(sectionsForCourses.CourseIds), sectionsForCourses.FromCache).Result;
                return results.Count() + " Course Sections returned";
            });
        }

        /// <summary>
        /// Gets the test credits index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Credits()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the credits tests.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostCredits()
        {
            return Run(() =>
            {
                var results = CreateClient().GetAcademicHistory4Async((string)HttpContext.Items[PersonIdKey]).Result;
                var cnt = results.AcademicTerms.Sum(terms => terms.AcademicCredits.Count);
                return cnt + " Academic Credits returned";
            });
        }

        /// <summary>
        /// Gets the test course search index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult CourseSearch()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the course search test.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CourseSearch(TestCourseSearch search)
        {
            return Run(() =>
            {
                var emptyList = new List<string>();
                var results = CreateClient().SearchCoursesAsync(emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList,
                    null, null, search.Keywords, null, "", emptyList, emptyList, 10, 1).Result;
                return results.TotalItems + " Courses returned";
            });
        }

        /// <summary>
        /// Gets the test course search parallel index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult CourseSearchParallel()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the course search parallel test.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CourseSearchParallel(TestCourseSearch search)
        {
            var sw = new Stopwatch();
            sw.Start();
            var emptyList = new List<string>();
            Thread lastThread = null;
            for (int i = 0; i < 5; i++)
            {
                Thread t1 = new Thread(ExecuteCourseSearch);
                t1.Start(search.Keywords);
                lastThread = t1;
            }
            lastThread.Join();
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        private void ExecuteCourseSearch(object keywords)
        {
            var emptyList = new List<string>();
            CreateClient().SearchCourses(emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList,
                null, null, (string)keywords, null, "", emptyList, emptyList, 10, 1);
        }

        /// <summary>
        /// Gets the test courses index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Courses()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the courses by ids test.
        /// </summary>
        /// <param name="courses"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Courses(TestCourses courses)
        {
            var criteria = new Dtos.Student.CourseQueryCriteria() { CourseIds = GetDelimited(courses.CourseIds) };
            return Run(() =>
            {
                var results = CreateClient().QueryCourses2Async(criteria).Result;
                return results.Count() + " courses returned";
            });
        }

        /// <summary>
        /// Gets the test section index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Sections()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the sections test.
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Sections(TestSections sections)
        {
            return Run(() =>
            {
                var results = CreateClient().GetSections5Async(GetDelimited(sections.SectionIds), sections.FromCache).Result;
                return results.Count() + " sections returned";
            });
        }

        /// <summary>
        /// Gets the test faculty index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Faculty()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the the faculty test.
        /// </summary>
        /// <param name="faculty"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Faculty(TestFaculty faculty)
        {
            var criteria = new Dtos.Student.FacultyQueryCriteria() { FacultyIds = GetDelimited(faculty.FacultyIds) };
            return Run(() =>
            {
                var results = CreateClient().QueryFacultyAsync(criteria).Result;
                return results.Count() + " faculty returned";
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
