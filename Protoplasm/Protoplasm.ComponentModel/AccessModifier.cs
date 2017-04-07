using System;

namespace Protoplasm.ComponentModel
{
	[Flags]
	public enum AccessModifier
	{
		Public = 1, Private = 2, Protected = 4, Internal = 8, ProtectedInternal = 16,
		All = 31,
		NotPublic = 30
	}
}