using System;
using ChangeDetector.Reflection;

namespace ChangeDetector
{
    public interface ICompareOptions
    {
        Func<object, object, bool> Equality { get; set; }
        CollectionComparison CollectionComparison { get; set; }
    }

    /// <summary>
    /// Mapping information for a class member.
    /// </summary>
    public class MemberMapping : ICompareOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the member is ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ignored; otherwise, <c>false</c>.
        /// </value>
        public bool Ignored { get; set; }

        /// <summary>
        /// Gets or sets the member accessor.
        /// </summary>
        /// <value>
        /// The member accessor.
        /// </value>
        public IMemberAccessor MemberAccessor { get; set; }


        public Func<object, object, bool> Equality { get; set; }


        public CollectionComparison CollectionComparison { get; set; }
    }
}