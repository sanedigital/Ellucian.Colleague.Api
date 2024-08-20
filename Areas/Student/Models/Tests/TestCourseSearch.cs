// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Ellucian.Colleague.Api.Areas.Student.Models.Tests
{
    /// <summary>
    /// Test course search model.
    /// </summary>
    public class TestCourseSearch
    {
        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        [Required]
        public string Keywords { get; set; }
    }
}
