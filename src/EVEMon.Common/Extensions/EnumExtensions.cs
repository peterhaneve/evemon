using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using EVEMon.Common.Attributes;

namespace EVEMon.Common.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Checks whether the given member has an access mask.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasForcedOnStartup(this Enum item) => GetAttribute<ForcedOnStartupAttribute>(item) != null;

        /// <summary>
        /// Gets the description bound to the given enumeration member.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum item) => GetAttribute<DescriptionAttribute>(item).Description;

        /// <summary>
        /// Checks whether the given member has a header.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasHeader(this Enum item) => GetAttribute<HeaderAttribute>(item) != null;

        /// <summary>
        /// Gets the header bound to the given enumeration member.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetHeader(this Enum item) => GetAttribute<HeaderAttribute>(item).Header;

        /// <summary>
        /// Checks whether the given member has a specific parent.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasParent(this Enum item, Enum other) => GetAttribute<ParentAttribute>(item)?.Parents?.Any(e => other.Equals(e)) ?? false;

        /// <summary>
        /// Gets the period bound to the given enumeration member.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static UpdateAttribute GetUpdatePeriod(this Enum item) => GetAttribute<UpdateAttribute>(item);

        /// <summary>
        /// Gets the default value of the given enumeration member.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static int GetDefaultValue(this Enum item) => (int)GetAttribute<DefaultValueAttribute>(item).Value;

        /// <summary>
        /// Gets the attribute associated to the given enumeration item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">item</exception>
        private static TAttribute GetAttribute<TAttribute>(this Enum item)
            where TAttribute : Attribute
        {
            item.ThrowIfNull(nameof(item));

            MemberInfo[] members = item.GetType().GetMember(item.ToString());
            if (members.Length <= 0)
                return null;

            object[] attrs = members[0].GetCustomAttributes(typeof(TAttribute), false);
            if (attrs.Length > 0)
                return (TAttribute)attrs[0];

            return null;
        }

        /// <summary>
        /// Gets the values for this enum.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TEnum> GetValues<TEnum>() => Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

        /// <summary>
        /// Gets the descriptions for this enum.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<object> GetDescriptions<TEnum>()
            => Enum.GetValues(typeof(TEnum)).Cast<Enum>().Select(item => item.GetDescription());

        /// <summary>
        /// Gets the values that are powers of two for this flag enum, excluding the one for zero.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TEnum> GetBitValues<TEnum>()
        {
            foreach (object value in Enum.GetValues(typeof(TEnum)))
            {
                // Check it matches a power of two 
                bool found = false;
                for (int i = 0; i < 32; i++)
                {
                    if ((int)value != 1 << i)
                        continue;
                    found = !found;

                    // If two bits matched, found is false again and value is not a power of two
                    if (!found)
                        break;
                }

                // Is it a power of two ?
                if (found)
                    yield return (TEnum)value;
            }
        }

        /// <summary>
        /// Gets the enum value from description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static Enum GetValueFromDescription<TEnum>(string description)
            => Enum.GetValues(typeof(TEnum)).Cast<Enum>().FirstOrDefault(item => item.GetDescription() == description);

    }
}
