// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Api.MetadataAttributes
{
    /// <summary>
    /// The attribute can indicate if an action/route is Eedm supported.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]

    public class EedmSupportedAttribute : Attribute
    {
        private bool _isSupported;
        /// <summary>
        /// Returns true if this action is Eedm supported, otherwise false.
        /// </summary>
        public bool IsSupported
        {
            get { return _isSupported; }
        }

        /// <summary>
        /// The constructor. 
        /// </summary>
        /// <param name="isSupported">Default parameter of true, so you only need to specify if false (or don't even include the attribute).</param>
        public EedmSupportedAttribute(bool isSupported = true)
        {
            _isSupported = isSupported;
        }

    }
}
