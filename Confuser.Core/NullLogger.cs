using System;
using dnlib.DotNet;

namespace Confuser.Core {
	/// <summary>
	///     An <see cref="ILogger" /> implementation that doesn't actually do any logging.
	/// </summary>
	internal class NullLogger : ILogger {
		/// <summary>
		///     The singleton instance of <see cref="NullLogger" />.
		/// </summary>
		public static readonly NullLogger Instance = new NullLogger();

		/// <summary>
		///     Prevents a default instance of the <see cref="NullLogger" /> class from being created.
		/// </summary>
		NullLogger() { }

		/// <inheritdoc />
		public void Log(string msg) { }

		/// <inheritdoc />
		public void LogFormat(string format, params object[] args) { }

		/// <inheritdoc />
		public void Error(string msg) { }

		/// <inheritdoc />
		public void ErrorFormat(string format, params object[] args) { }

		/// <inheritdoc />
		public void ErrorException(string msg, Exception ex) { }

		/// <inheritdoc />
		public void Progress(int overall, int progress) { }

		/// <inheritdoc />
		public void EndProgress() { }

		/// <inheritdoc />
		public void Finish(bool successful) { }

		/// <inheritdoc />
		public void BeginModule(ModuleDef module) { }

		/// <inheritdoc />
		public void EndModule(ModuleDef module) { }
	}
}