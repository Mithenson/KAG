using System;
using System.Data;
using Newtonsoft.Json;

namespace KAG.Shared.Prototype
{
	public sealed class IdentityConverter : JsonConverter<Identity>
	{
		public override void WriteJson(JsonWriter writer, Identity value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		public override Identity ReadJson(JsonReader reader, Type objectType, Identity existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (!(reader.Value is string input))
				throw new InvalidConstraintException(
					$"The `{nameof(reader)}={reader.Path}` expects {nameof(Identity)} to be written as {nameof(String)} "
					+ $"but it received `{nameof(reader.Value)}={reader.Value}`.");

			if (!Enum.TryParse(input, out Identity identity))
				throw new InvalidOperationException($"The `{nameof(reader)}={reader.Path}` could not parse `{nameof(input)}={input}` to an {nameof(Identity)}.");

			return identity;
		}
	}
}