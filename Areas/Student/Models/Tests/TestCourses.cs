// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Ellucian.Colleague.Api.Areas.Student.Models.Tests
{
    /// <summary>
    /// Test courses model
    /// </summary>
    public class TestCourses
    {
        /// <summary>
        /// Gets or sets the course ids.
        /// </summary>
        [DisplayName("Course IDs, comma or space delimited")]
        [Required]
        public string CourseIds { get; set; }

    }
}
