// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbeck�" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;

namespace Debugger
{
	public class ExceptionEventArgs: DebuggerEventArgs
	{
		bool @continue;
		Exception exception;
		
		public bool Continue {
			get {
				return @continue;
			}
			set {
				@continue = value;
			}
		}
		
		public Exception Exception {
			get {
				return exception;
			}
		}
		
		public ExceptionEventArgs(NDebugger debugger, Exception exception):base(debugger)
		{
			this.exception = exception;
		}
	}
}
