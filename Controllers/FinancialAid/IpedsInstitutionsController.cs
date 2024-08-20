// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes IpedsInstitution data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class IpedsInstitutionsController : BaseCompressedApiController
    {
        private readonly IIpedsInstitutionRepository ipedsInstitutionRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Instantiates a new instance of the IpedsInstitutionsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="ipedsInstitutionRepository">IpedsInstitutionRepository</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public IpedsInstitutionsController(IAdapterRegistry adapterRegistry, IIpedsInstitutionRepository ipedsInstitutionRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.ipedsInstitutionRepository = ipedsInstitutionRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Query by post method used to get IpedsInstitution objects for the given OPE (Office of Postsecondary Education) Ids
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources.
        /// </accessComments>
        /// <param name="opeIds">List of OPE Ids</param>
        /// <returns>The requested IpedsInstitution DTOs</returns>
        /// <note>IpedsInstitutions are cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/ipeds-institutions", 1, true, Name = "QueryByPostIpedsInstitutionsByOpeId")]
        public async Task<ActionResult<IEnumerable<IpedsInstitution>>> QueryByPostIpedsInstitutionsByOpeIdAsync([FromBody]IEnumerable<string> opeIds)
        {
            if (opeIds == null || !opeIds.Any())
            {
                var message = "At least one item in list of opeIds must be provided";
                logger.LogInformation(message);
                return new List<IpedsInstitution>();
            }

            var ipedsInstitutionEntityList = await ipedsInstitutionRepository.GetIpedsInstitutionsAsync(opeIds);

            var ipedsInstitutionDtoAdapter = adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.IpedsInstitution, IpedsInstitution>();

            var ipedsInstitutionDtoList = new List<IpedsInstitution>();
            foreach (var ipedsInstitutionEntity in ipedsInstitutionEntityList)
            {
                ipedsInstitutionDtoList.Add(ipedsInstitutionDtoAdapter.MapToType(ipedsInstitutionEntity));
            }

            return ipedsInstitutionDtoList;

        }


    }
}
