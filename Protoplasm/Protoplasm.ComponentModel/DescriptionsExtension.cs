using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protoplasm.ComponentModel
{
	public static class DescriptionsExtension
	{

		public static MethodDescription[] MethodDescriptions(StaticModifier staticModifier = StaticModifier.All, AccessModifier accessModifier = AccessModifier.All)
		{
			var	ret = new List<MethodDescription>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					ret.AddRange(type.MethodDescriptions(staticModifier, accessModifier));
				}
			}

			return ret.ToArray();
		}

		public static MethodDescription[] MethodDescriptions(this Type type, StaticModifier staticModifier = StaticModifier.All, AccessModifier accessModifier = AccessModifier.All)
		{
			var bindingFlags = GetBindingFlags(staticModifier, accessModifier);

			return type.GetMethods(bindingFlags)
			           .Where(x => CheckAccessModifier(x, accessModifier))
			           .Select(x => x.Description())
			           .Where(x => x.IsIdentified)
			           .ToArray();
		}

		static BindingFlags GetBindingFlags(StaticModifier staticModifier, AccessModifier accessModifier)
		{
			BindingFlags p1;
			BindingFlags p2;
			switch (staticModifier)
			{
				case StaticModifier.Static:
					p1 = BindingFlags.Static;
					break;
				case StaticModifier.Instance:
					p1 = BindingFlags.Instance;
					break;
				case StaticModifier.All:
					p1 = BindingFlags.Static | BindingFlags.Instance;
					break;
				default:
					throw new ArgumentOutOfRangeException("staticModifier");
			}

			if (accessModifier.Contains(AccessModifier.Public))
				p1 |= BindingFlags.Public;

			if (AccessModifier.NotPublic.ContainsAny(accessModifier))
				p1 |= BindingFlags.NonPublic;

			return p1;
		}

		private static bool CheckAccessModifier(MethodInfo method, AccessModifier accessModifier)
		{
			return (accessModifier.Contains(AccessModifier.Public) && method.IsPublic)
			       || (accessModifier.Contains(AccessModifier.Private) && method.IsPrivate)
			       || (accessModifier.Contains(AccessModifier.Protected) && (method.IsFamily || method.IsFamilyOrAssembly))
			       || (accessModifier.Contains(AccessModifier.Internal) && (method.IsAssembly || method.IsFamilyOrAssembly))
			       || (accessModifier.Contains(AccessModifier.ProtectedInternal) && method.IsFamilyOrAssembly);
		}

		public static MethodDescription Description(this MethodInfo method)
		{
			return new MethodDescription(method);
		}

		public static bool ContainsAny(this AccessModifier values, AccessModifier value)
		{
			var accessModifier = (values & value);
			var @is = accessModifier != 0;
			return @is;
		}

		public static bool Contains(this AccessModifier values, AccessModifier value)
		{
			var accessModifier = (values & value);
			var @is = accessModifier == value;
			return @is;
		}
	}
}