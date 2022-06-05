using System;
using UnityEngine;

namespace KAG.Unity.Common
{
	public sealed class BoolCommandBehaviour : CommandBehaviour
	{
		protected override Type SourceType => typeof(bool);

		public void Execute(bool value) => 
			_dataBindingTarget.Set(value);
	}
}