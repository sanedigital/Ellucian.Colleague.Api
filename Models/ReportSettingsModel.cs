// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// View model that contains API report settings
    /// </summary>
    public class ReportSettingsModel
    {
        /// <summary>
        /// The path to the report logo image file
        /// </summary>
        public string ReportLogoPath { get; set; }

        /// <summary>
        /// The path to the report watermark image file
        /// </summary>
        public string UnofficialWatermarkPath { get; set; }

        /// <summary>
        /// Constructor for a ReportSettingsModel
        /// </summary>
        public ReportSettingsModel()
        {
            ReportLogoPath = string.Empty;
            UnofficialWatermarkPath = string.Empty;
        }
    }
}
