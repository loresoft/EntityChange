using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EntityChange
{
    /// <summary>
    /// An <see langword="interface"/> for formatting changes
    /// </summary>
    public interface IChangeFormatter
    {
        /// <summary>
        /// Create a readable change report.
        /// </summary>
        /// <param name="changes">The changes to format.</param>
        /// <returns>A string representing the <paramref name="changes"/>.</returns>
        string Format(ReadOnlyCollection<ChangeRecord> changes);
    }
}