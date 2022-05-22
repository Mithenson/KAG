using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KAG.Shared.Extensions;
using Sirenix.OdinInspector;

namespace KAG.Unity.Common.DataBindings
{
	public static class DataBindingUtilities
	{
		#if UNITY_EDITOR
		
		public static class Editor
		{
			public static IList<ValueDropdownItem<DataBindingTargetBuilder>> GetAvailableDataBindingTargetBuilders(Type declaringType) =>
				declaringType.GetMethods(DataBindingConstants.MethodSearchFlags)
				   .Where(method => FilterCandidateTargetMethods(method, null))
				   .Select(method => 
					{
						var name = method.GetDetailedName().NicifyName();
						var binding = ConvertMethodToDataBindingTargetBuilder(method);
					
						return new ValueDropdownItem<DataBindingTargetBuilder>(name, binding);
					})
				   .Prepend(DataBindingConstants.DefaultDataBindingTargetBuilderDropdownItem)
				   .ToArray();
			
			public static IList<ValueDropdownItem<DataBindingTargetBuilder>> GetAvailableDataBindingTargetBuilders(Type sourceType, Type declaringType) =>
				declaringType.GetProperties(DataBindingConstants.PropertySearchFlags)
				   .Where(property => property.PropertyType == sourceType)
				   .Select(property =>
					{
						var name = property.Name.NicifyName();
						var binding = new DataBindingTargetBuilder(DataBindingTargetBuildMode.Property, property.Name);
					
						return new ValueDropdownItem<DataBindingTargetBuilder>(name, binding);
					})
				   .Concat(declaringType.GetMethods(DataBindingConstants.MethodSearchFlags)
					   .Where(method => FilterCandidateTargetMethods(method, sourceType))
					   .Select(method => 
						{
							var name = method.GetDetailedName().NicifyName();
							var binding = ConvertMethodToDataBindingTargetBuilder(method);
					
							return new ValueDropdownItem<DataBindingTargetBuilder>(name, binding);
						}))
				   .Prepend(DataBindingConstants.DefaultDataBindingTargetBuilderDropdownItem)
				   .ToArray();
			
			private static bool FilterCandidateTargetMethods(MethodInfo method, Type sourcePropertyType)
			{
				if (method.IsSpecialName)
					return false;

				if (sourcePropertyType == null)
					return method.IsParameterlessMethod();
			
				if (sourcePropertyType != typeof(bool))
					return method.HasASingleParameterOfType(sourcePropertyType);

				return method.IsParameterlessMethod() || method.HasASingleParameterOfType(sourcePropertyType);
			}
			private static DataBindingTargetBuilder ConvertMethodToDataBindingTargetBuilder(MethodInfo method)
			{
				if (method.IsParameterlessMethod())
					return new DataBindingTargetBuilder(DataBindingTargetBuildMode.ParameterlessMethod, method.Name);

				return new DataBindingTargetBuilder(DataBindingTargetBuildMode.Method, method.Name);
			}
		}
		
		#endif
	}
}