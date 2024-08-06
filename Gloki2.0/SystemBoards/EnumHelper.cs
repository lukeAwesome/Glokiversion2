
namespace Gloki2._0.SystemBoards
{
	using System;
	using System.ComponentModel;
	/// <summary>
	///     An enum helper.
	/// </summary>
	public static class EnumHelper
	{
		/// <summary>
		///     An Enum extension method that gets a description.
		/// </summary>
		///
		/// <param name="e">The e to act on.</param>
		///
		/// <returns>
		///     The description.
		/// </returns>
		public static string GetDescription(this Enum e)
		{
			var fieldInfo = e.GetType().GetField(name: e.ToString());
			var enumAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(
				attributeType: typeof(DescriptionAttribute),
				inherit: false);

			return enumAttributes.Length > 0 ? enumAttributes[0].Description : e.ToString();
		}

		/// <summary>
		///     Gets enum from description.
		/// </summary>
		///
		/// <typeparam name="TEnum">Type of the enum.</typeparam>
		/// <param name="description">The description.</param>
		///
		/// <returns>
		///     The enum from description.
		/// </returns>
		public static TEnum? GetEnumFromDescription<TEnum>(string description) where TEnum : struct, Enum
		{
			const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

			foreach (var field in typeof(TEnum).GetFields())
			{
				if (Attribute.GetCustomAttribute(
						element: field,
						attributeType: typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
				{
					if (string.Compare(strA: attribute.Description, strB: description, comparisonType: comparison) == 0)
					{
						return (TEnum)field.GetValue(obj: null);
					}
				}

				if (string.Compare(strA: field.Name, strB: description, comparisonType: comparison) == 0)
				{
					return (TEnum)field.GetValue(obj: null);
				}
			}

			return null;
		}
	}
}
