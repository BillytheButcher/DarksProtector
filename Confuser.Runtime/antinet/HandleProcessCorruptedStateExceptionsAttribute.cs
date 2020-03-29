using System;

namespace Confuser.Runtime {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	internal class HandleProcessCorruptedStateExceptionsAttribute : Attribute {

	}
}