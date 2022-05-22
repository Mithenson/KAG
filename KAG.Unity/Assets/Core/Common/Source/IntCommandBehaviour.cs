using System;

namespace KAG.Unity.Common
{
	public sealed class IntCommandBehaviour : CommandBehaviour
	{
		protected override Type SourceType => typeof(int);

		public void Execute(int value) => 
			_dataBindingTarget.Set(value);
	}
}