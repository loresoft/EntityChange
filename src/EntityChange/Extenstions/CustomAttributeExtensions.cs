using System;

#if NET40
namespace System.Reflection
{
    /// <summary>
    /// Contains static methods for retrieving custom attributes.
    /// </summary>
    public static class CustomAttributeExtensions
    {
        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a 
        /// specified member, and optionally inspects the ancestors of that member.
        /// </summary>
        /// <returns>A custom attribute that matches <paramref name="attributeType" />, or null if no such attribute is found.</returns>
        /// <param name="element">The member to inspect.</param>
        /// <param name="attributeType">The type of attribute to search for.</param>
        /// <param name="inherit">true to inspect the ancestors of <paramref name="element" />; otherwise, false. </param>
        public static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType, bool inherit)
        {
            return Attribute.GetCustomAttribute(element, attributeType, inherit);
        }

        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a 
        /// specified member, and optionally inspects the ancestors of that member.
        /// </summary>
        /// <returns>A custom attribute that matches <typeparamref name="T"/>, or null if no such attribute is found.</returns>
        /// <param name="element">The member to inspect.</param>
        /// <param name="inherit">true to inspect the ancestors of <paramref name="element" />; otherwise, false. </param>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        public static T GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute
        {
            return (T)element.GetCustomAttribute(typeof(T), inherit);
        }
    }
}
#endif