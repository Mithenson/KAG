using System;

namespace KAG.Shared.Extensions
{
	public static class StringExtensions
	{
		private static readonly char[] _formatCamelCaseNextCharToUpper = new char[]
		{
			' ',
			',', 
			'('
		};
		
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
			for (var i = 1; i < value.Length - 1; i++)
			{
				var character = value[i];
				if (Array.IndexOf(_formatCamelCaseNextCharToUpper, character) != -1)
				{
					var replacementIndex = i + 1;
					var nextCharacter = value[replacementIndex];

					if (!char.IsLetter(nextCharacter))
						continue;
					
					value = value.Remove(replacementIndex, 1).Insert(replacementIndex, $"{char.ToUpper(nextCharacter)}");
					i += 2;
					
					continue;
				}

				if (character.IsUpperCaseLetter()
				    && (value[i + 1].IsUpperCaseLetter() && value[i - 1].IsLowerCaseLetter()
				        || value[i - 1].IsUpperCaseLetter() && value[i + 1].IsLowerCaseLetter()
				        || value[i - 1].IsLowerCaseLetter() && value[i + 1].IsLowerCaseLetter()))
				{
					value = value.Remove(i, 1).Insert(i, $" {character}");
					i++;
				}
			}

			return value;
		}

		public static string NicifyName(this string value)
		{
			value = value.Replace("_", string.Empty);

			var firstCharacter = value[0];
			value = value.Remove(0, 1).Insert(0, char.ToUpper(firstCharacter).ToString());

			return value.FormatCamelCase();
		}

		public static bool IsUpperCaseLetter(this char value) =>
			char.IsLetter(value) && char.IsUpper(value);
		public static bool IsLowerCaseLetter(this char value) =>
			char.IsLetter(value) && char.IsLower(value);
	}
}