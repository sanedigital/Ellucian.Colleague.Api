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
    /// Test sections for courses model.
    /// </summary>
    public class TestSectionsForCourses
    {
        /// <summary>
        /// Gets or sets the course id.
        /// </summary>
        [DisplayName("Course IDs, comma or space delimited")]
        [Required]
        public string CourseIds { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the cache should be used.
        /// </summary>
        [DisplayName("Use section cache")]
        public bool FromCache { get; set; }
    }
}
