using System;

namespace KAG.Unity.Common
{
	public sealed class StringCommandBehaviour : CommandBehaviour
	{
		protected override Type SourceType => typeof(string);

		public void Execute(string value) => 
			_dataBindingTarget.Set(value);
	}
}