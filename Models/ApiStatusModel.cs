// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Api Status Model
    /// </summary>
    public class ApiStatusModel
    {
        /// <summary>
        /// Count of Unsuccessful logins
        /// </summary>
        public int UnSuccessfulLoginCounter { get; set; }
        /// <summary>
        /// Gets or Sets the boolean value for DAS Datareader usuage
        /// </summary>
        public bool UseDasDataReader { get; set; }
    }
}
