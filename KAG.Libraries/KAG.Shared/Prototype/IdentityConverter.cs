using System;
using System.Data;
using Newtonsoft.Json;

namespace KAG.Shared.Prototype
{
	public sealed class IdentityConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => 
			objectType == typeof(Identity);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
			writer.WriteValue(value.ToString());

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) 
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