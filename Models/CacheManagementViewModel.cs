using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// View model for CacheManagement page
    /// </summary>
    public class CacheManagementViewModel
    {
        /// <summary>
        /// Cache keys list
        /// </summary>
        public IEnumerable<string> CacheKeys { get; set; }
        /// <summary>
        /// Application identification string
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Intended for the machine name of the host to assist in recognizing where it is executing.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Error message for the view model or action attempted.
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// We need to redirect to a confirmation URL because the app will restart.
        /// </summary>
        public string ConfirmationUrl { get; set; }
    }
}
