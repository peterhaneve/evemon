using System;

namespace EVEMon.Common.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Throws an ArgumentNullException if the object is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void ThrowIfNull<T>(this T obj, string paramName, string message = null)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName, message ?? "Value cannot be null.");
        }
    }
}
