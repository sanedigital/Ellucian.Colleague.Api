// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Areas.Planning.Models.Tests;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Planning.Repositories;
using Ellucian.Colleague.Data.Student;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Client;
using Ellucian.Dmi.Client.DMIF;
using Ellucian.Logging;
using Ellucian.Web.Adapters;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.Security.Claims;

namespace Ellucian.Colleague.Api.Areas.Planning.Controllers
{
    /// <summary>
    /// Test utilities controller for planning module.
    /// </summary>
    [LocalRequest]
    [Area("Planning")]
    public class LocalTestController : Controller
    {
        private static string TxFactoryKey = "TxFactory";
        private StringLogger logger = new StringLogger();
        private ApiSettings apiSettingsMock;
        private ColleagueSettings colleagueSettingsMock;
        private DmiSettings _dmiSettings;
        private ICurrentUserFactory _currentUserFactory;
        private IRoleRepository _roleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #region Constructor(s)
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dmiSettings"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="httpContextAccessor"></param>
        public LocalTestController(DmiSettings dmiSettings, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IHttpContextAccessor httpContextAccessor)
        {
            _dmiSettings = dmiSettings;
            _currentUserFactory = currentUserFactory;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Constructor(s)

        #region Scan Rules Methods
        /// <summary>
        /// Scan of rules to determine which will not be executed in .net
        /// </summary>
        /// <returns></returns>
        [ActionName("ScanRules")]
        public async Task<ActionResult> ScanRulesAsync()
        {
            Setup();
            var results = await QueryRulesAsync(GetTxFactory());
            results.Log = logger.ToString();
            return View(results);
        }
        #endregion Scan Rules Methods

        #region Evaluation Methods
        /// <summary>
        /// Gets the evaluation test page
        /// </summary>
        /// <returns></returns>
        public ActionResult Evaluation()
        {
            return View();
        }

        /// <summary>
        /// Submits an evaluation request with a program to evaluated for the logged in person
        /// NOTE: This method is currently not working due to a platform level issue with CurrentUser
        ///       Platform team has a task CSSPO-8655 to correct this (Q3 2023 Sprint 1) 
        ///  
        /// </summary>
        /// <param name="testEvaluation"><see cref="TestEvaluation"/> model</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Evaluation(TestEvaluation testEvaluation)
        {
            //ensure user has logged in before calling evaluate method
            var cookieValue = LocalUserUtilities.GetCookie(Request);
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new ColleagueWebApiException("Log in first");
            }

            TempData["program"] = testEvaluation.Program.ToUpper();
            return RedirectToAction("EvaluationResult");
        }

        /// <summary>
        /// Gets the evaluation result page for the student and program.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> EvaluationResult()
        {
            var result = new TestEvaluationResult();

            Setup();
            var txFactory = GetTxFactory();
            var cacheProvider = new MemoryCacheProvider();
            var ruleAdapterRegistry = new RuleAdapterRegistry();
            apiSettingsMock = new ApiSettings("null");
            colleagueSettingsMock = new ColleagueSettings();
            ruleAdapterRegistry.Register<Course>("COURSES", new CourseRuleAdapter());
            ruleAdapterRegistry.Register<AcademicCredit>("STUDENT.ACAD.CRED", new AcademicCreditRuleAdapter());
            var ruleRepo = new RuleRepository(cacheProvider, txFactory, logger, ruleAdapterRegistry, new RuleConfiguration());
            var gradeRepo = new GradeRepository(cacheProvider, txFactory, logger);
            var requirementRepo = new RequirementRepository(cacheProvider, txFactory, logger, gradeRepo, ruleRepo);
            var programRequirementsRepo = new ProgramRequirementsRepository(cacheProvider, txFactory, logger, requirementRepo, gradeRepo, ruleRepo);
            var studentRepo = new StudentRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var studentProgramRepo = new StudentProgramRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var courseRepo = new CourseRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var termRepo = new TermRepository(cacheProvider, txFactory, logger);
            var academicCreditRepo = new AcademicCreditRepository(cacheProvider, txFactory, logger, courseRepo, gradeRepo, termRepo, null, apiSettingsMock);
            var degreePlanRepo = new DegreePlanRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var studentDegreePlanRepo = new StudentDegreePlanRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var programRepo = new ProgramRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var catalogRepo = new CatalogRepository(cacheProvider, txFactory, logger);
            var planningConfigRepo = new PlanningConfigurationRepository(cacheProvider, txFactory, logger);
            var applicantRepo = new ApplicantRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var planningStudentRepo = new PlanningStudentRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var referenceDataRepo = new ReferenceDataRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var baseConfigRepo = new ConfigurationRepository(cacheProvider, txFactory, apiSettingsMock, logger, colleagueSettingsMock, _httpContextAccessor);

            //dumper = new Dumper();
            var programEvaluationService = new ProgramEvaluationService(
                new AdapterRegistry(new HashSet<ITypeAdapter>(), null, logger), studentDegreePlanRepo, programRequirementsRepo, studentRepo,
                planningStudentRepo, applicantRepo, studentProgramRepo, requirementRepo, academicCreditRepo, degreePlanRepo, courseRepo,
                termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo, referenceDataRepo,
                _currentUserFactory, _roleRepository, logger, baseConfigRepo);

            result.PersonId = GetPersonId();
            result.ProgramId = (string)TempData["program"];

            var evaluation = (await programEvaluationService.EvaluateAsync(result.PersonId, new List<string>() { result.ProgramId }, null)).First();
            result.Evaluation = evaluation.ToString();
            result.Log = logger.ToString();

            return View(result);
        }
        #endregion Evaluation Methods

        #region Private Methods
        private string GetPersonId()
        {
            return HttpContext.Items[TestController.PersonIdKey] as string;
        }

        private IColleagueTransactionFactory GetTxFactory()
        {
            return HttpContext.Items[TxFactoryKey] as IColleagueTransactionFactory;
        }

        private void Setup()
        {
            logger.Clear();
            var cookieValue = LocalUserUtilities.GetCookie(Request);
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new ColleagueWebApiException("Log in first");
            }
            var baseUrl = cookieValue.Split('*')[0];
            var token = cookieValue.Split('*')[1];
            var principal = JwtHelper.CreatePrincipal(token);
            var sessionClaim = (principal as ClaimsPrincipal).Identities.First().Claims.FirstOrDefault(c => c.Type == "sid");
            var personId = (principal as ClaimsPrincipal).Identities.First().Claims.FirstOrDefault(c => c.Type == "pid").Value;
            var securityToken = sessionClaim.Value.Split('*')[0];
            var controlId = sessionClaim.Value.Split('*')[1];
            var session = new StandardDmiSession() { SecurityToken = securityToken, SenderControlId = controlId };

            var txFactory = new TestTransactionFactory(session, logger, _dmiSettings, _httpContextAccessor);
            HttpContext.Items[TxFactoryKey] = txFactory;
            HttpContext.Items[TestController.PersonIdKey] = personId;
        }

        //private ColleagueApiClient CreateClient()
        //{
        //    var cookieValue = LocalUserUtilities.GetCookie(Request);
        //    if (string.IsNullOrEmpty(cookieValue))
        //    {
        //        throw new ColleagueWebApiException("Log in first");
        //    }
        //    var baseUrl = cookieValue.Split('*')[0];
        //    var token = cookieValue.Split('*')[1];
        //    var principal = JwtHelper.CreatePrincipal(token);
        //    var personId = (principal as ClaimsPrincipal).Identities.First().Claims.First(claim => claim.Type == ClaimConstants.PersonId).Value;
        //    HttpContext.Items[PersonIdKey] = personId;
        //    var client = new ColleagueApiClient(baseUrl, _logger);
        //    client.Credentials = token;
        //    return client;
        //}


        private async Task<TestScanRules> QueryRulesAsync(IColleagueTransactionFactory txFactory)
        {
            var results = new TestScanRules();
            //results.Name = env;
            var cacheProvider = new MemoryCacheProvider();
            var ruleIds = new List<string>();
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.REQMT.BLOCKS", "ACRB.ACAD.CRED.RULES NE '' SAVING ACRB.ACAD.CRED.RULES"));
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.REQMT.BLOCKS", "ACRB.MAX.COURSES.RULES NE '' SAVING ACRB.MAX.COURSES.RULES"));
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.REQMT.BLOCKS", "ACRB.MAX.CRED.RULES NE '' SAVING ACRB.MAX.CRED.RULES"));
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.PROGRAM.REQMTS", "ACPR.ACAD.CRED.RULES NE '' SAVING ACPR.ACAD.CRED.RULES"));
            ruleIds = ruleIds.Distinct().Where(r => !r.Contains("ý")).ToList();
            var ruleAdapterRegistry = new RuleAdapterRegistry();
            ruleAdapterRegistry.Register<AcademicCredit>("STUDENT.ACAD.CRED", new AcademicCreditRuleAdapter());
            ruleAdapterRegistry.Register<Course>("COURSES", new CourseRuleAdapter());
            var ruleRepository = new RuleRepository(cacheProvider, txFactory, logger, ruleAdapterRegistry, new RuleConfiguration());
            var rules = await ruleRepository.GetManyAsync(ruleIds);
            results.Count = rules.Count();
            foreach (var rule in rules)
            {
                if (rule.GetType() == typeof(Rule<Course>))
                {
                    var crule = (Rule<Course>)rule;
                    if (crule.HasExpression)
                    {
                        results.Supported++;
                    }
                    else
                    {
                        results.NotSupportedNames.Add(crule);
                    }
                }
                else if (rule.GetType() == typeof(Rule<AcademicCredit>))
                {
                    var stcRule = (Rule<AcademicCredit>)rule;
                    if (stcRule.HasExpression)
                    {
                        results.Supported++;
                    }
                    else
                    {
                        results.NotSupportedNames.Add(stcRule);
                    }
                }
            }


            var rawRecords = txFactory.GetDataReader().BulkReadRecord<Rules>("RULES", results.NotSupportedNames.Select(rr => rr.Id).ToArray());
            foreach (var record in rawRecords)
            {
                logger.LogInformation(RuleRepository.Dump(record));
                logger.LogInformation("Not supported because: " + results.NotSupportedNames.First(rr => rr.Id == record.Recordkey).NotSupportedMessage);
            }
            logger.LogInformation(" ");
            return results;
        }
        #endregion Private Methods

        #region Transaction Factory Class
        class TestTransactionFactory : IColleagueTransactionFactory
        {
            private StandardDmiSession session;
            private ILogger logger;
            private DmiSettings settings;
            private readonly IHttpContextAccessor httpContextAccessor;

            public TestTransactionFactory(StandardDmiSession session, ILogger logger, DmiSettings settings, IHttpContextAccessor httpContextAccessor)
            {
                this.session = session;
                this.logger = logger;
                this.settings = settings;
                this.httpContextAccessor = httpContextAccessor;
            }

            public IColleagueDataReader GetColleagueDataReader()
            {
                return new ColleagueDataReader(session, settings, httpContextAccessor);
            }

            public IColleagueDataReader GetDataReader()
            {
                return GetDataReader(false);
            }

            public IColleagueDataReader GetDataReader(bool anonymous)
            {
                if (anonymous)
                {
                    return new AnonymousColleagueDataReader(settings, httpContextAccessor);
                }
                else
                {
                    return new ColleagueDataReader(session, settings, httpContextAccessor);
                }
            }

            public IColleagueTransactionInvoker GetTransactionInvoker()
            {
                return new ColleagueTransactionInvoker(session.SecurityToken, session.SenderControlId, logger, settings, httpContextAccessor);
            }

            public DMIFileTransferClient GetDMIFClient()
            {
                return new DMIFileTransferClient(new DmiConnectionFactory(logger, settings, httpContextAccessor), logger);
            }
        }
        #endregion Transaction Factory Class
    }
}
