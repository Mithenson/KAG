using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KAG.Shared.Json
{
	public class CustomContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			
			if (member.MemberType == MemberTypes.Property)
				property.Ignored = true;
			
			return property;
		}
	}
}