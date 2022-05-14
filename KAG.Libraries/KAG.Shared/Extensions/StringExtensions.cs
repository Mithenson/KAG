using System;

namespace KAG.Shared.Extensions
{
	public static class StringExtensions
	{
		public static string GenericityToString(this Type type)
		{
			if (!type.IsGenericType)
				throw new InvalidOperationException($"The `{nameof(type)}={type}` isn't generic.");
			
			var genericArguments = type.GetGenericArguments();
			var genericity = $"<{genericArguments[0].Name}";

			for (var i = 1; i < genericArguments.Length; i++)
				genericity += $", {genericArguments[i].Name}";

			genericity += ">";
			return genericity;
		}

		public static string FormatCamelCase(this string value)
		{
			for (var i = 1; i < value.Length; i++)
			{
				var character = value[i];
				if (!char.IsUpper(character))
					continue;

				value = value.Remove(i, 1).Insert(i, $" {char.ToLower(character)}");
			}

			return value;
		}
	}
}