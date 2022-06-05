using System;

namespace KAG.Unity.Common
{
	public sealed class FloatCommandBehaviour : CommandBehaviour
	{
		protected override Type SourceType => typeof(float);

		public void Execute(float value) => 
			_dataBindingTarget.Set(value);
	}
}