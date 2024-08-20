// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ellucian.Colleague.Api.Areas.Planning.Models.Tests
{
    /// <summary>
    /// Test evaluation result model.
    /// </summary>
    public class TestEvaluationResult
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the program id.
        /// </summary>
        public string ProgramId { get; set; }

        /// <summary>
        /// Gets or sets the evaluation result text.
        /// </summary>
        public string Evaluation { get; set; }

        /// <summary>
        /// Gets or sets the evaluation log.
        /// </summary>
        public string Log { get; set; }
    }
}
