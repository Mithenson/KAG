using System.Text;
using Newtonsoft.Json;

namespace KAG.Shared.Prototype
{
	public sealed class Prototype
	{
		public Identity Identity => _identity;
		
		[JsonProperty("Identity")]
		private Identity _identity;
		
		[JsonProperty("Components")]
		private Component[] _components;

		public Prototype(Identity identity, params Component[] components)
		{
			_identity = identity;
			_components = components;
		}

		public Entity CreateEntity(ComponentTypeRepository componentTypeRepository)
		{
			var entity = new Entity(componentTypeRepository);

			var identity = new IdentityComponent()  {Value = _identity }; 
			entity.BYPASS_AddComponent(identity);

			foreach (var component in _components)
				entity.BYPASS_AddComponent(component);

			return entity;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendLine($"{nameof(_identity)}={_identity}");
			
			builder.AppendLine("Components=[");
			foreach (var component in _components)
				builder.AppendLine($"	{component}");

			builder.AppendLine("]");
			return builder.ToString();
		}
	}
}