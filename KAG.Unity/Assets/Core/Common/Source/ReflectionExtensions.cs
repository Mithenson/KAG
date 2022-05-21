using System;
using System.Reflection;

namespace KAG.Unity.Common
{
	public static class ReflectionExtensions
	{
		public static bool IsParameterlessMethod(this MethodInfo method) =>
			method.GetParameters().Length == 0;
		public static bool HasASingleParameterOfType(this MethodInfo method, Type type)
		{
			var parameters = method.GetParameters();
			if (parameters.Length != 1)
				return false;

			return parameters[0].ParameterType == type;
		}

		public static string GetDetailedName(this MethodInfo method)
		{
			var parameters = method.GetParameters();
			if (parameters.Length == 0)
				return $"{method.Name}()";
			
			var detailedName = $"{method.Name}({parameters[0].Name}";
			for (var i = 1; i < parameters.Length; i++)
				detailedName += $", {parameters[i].Name}";

			return $"{detailedName})";
		}
	}
}