// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Web.Http.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class SecuritySystemLoginAccessException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public SecuritySystemLoginAccessException() : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public SecuritySystemLoginAccessException(string message) : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public SecuritySystemLoginAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
