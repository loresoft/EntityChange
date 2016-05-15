using System;

namespace EntityChange
{

    /// <summary>
    /// The type of change operation
    /// </summary>
    public enum ChangeOperation
    {
        /// <summary>
        /// The change is an addition to a dictionary or collection.
        /// </summary>
        Add,
        /// <summary>
        /// The change is a removal from a dictionary or collection.
        /// </summary>
        Remove,
        /// <summary>
        /// The change is a property value replacement.
        /// </summary>
        Replace
    }
}