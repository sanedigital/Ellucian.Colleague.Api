// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// A view model that contains all the general API settings that can be set through the API Settings UI.
    /// </summary>
    public class ApiSettingsModel
    {
        /// <summary>
        /// The name of the current settings profile being used (development, production, etc)
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// The ID to this API Settings Model
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The current version of the API
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PhotoSettingsModel"/>.
        /// </summary>
        public PhotoSettingsModel PhotoSettings { get; set; }

        /// <summary>
        /// Instance of a report settings view model
        /// </summary>
        public ReportSettingsModel ReportSettings { get; set; }

        /// <summary>
        /// Instance of the Web API cache settings.
        /// </summary>
        public CacheSettingsModel CacheSettings { get; set; }

        /// <summary>
        /// Constructor for an API Settings Model
        /// </summary>
        public ApiSettingsModel()
        {
            PhotoSettings = new PhotoSettingsModel();
            ReportSettings = new ReportSettingsModel();
            CacheSettings = new CacheSettingsModel();
        }
    }
}
