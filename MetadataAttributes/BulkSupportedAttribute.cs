// Copyright 2023 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Api.MetadataAttributes
{
    /// <summary>
    /// Allows for bulk support indication
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BulkSupportedAttribute : Attribute
    {
        private bool _isSupported;
        /// <summary>
        /// Returns true if this action is bulk supported, otherwise false.
        /// </summary>
        public bool IsSupported
        {
            get { return _isSupported; }
        }

        /// <summary>
        /// The constructor. 
        /// </summary>
        /// <param name="isSupported">Default parameter of true, so you only need to specify if false (or don't even include the attribute).</param>
        public BulkSupportedAttribute(bool isSupported = true)
        {
            _isSupported = isSupported;
        }
    }
}
