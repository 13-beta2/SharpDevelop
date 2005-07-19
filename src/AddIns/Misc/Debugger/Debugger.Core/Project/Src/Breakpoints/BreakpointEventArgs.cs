// <file>
//     <owner name="David Srbeck�" email="dsrbecky@post.cz"/>
// </file>

using System;

namespace DebuggerLibrary 
{	
	[Serializable]
	public class BreakpointEventArgs : DebuggerEventArgs
	{
		Breakpoint breakpoint;
		
		public Breakpoint Breakpoint {
			get {
				return breakpoint;
			}
		}
		
		public BreakpointEventArgs(NDebugger debugger, Breakpoint breakpoint): base(debugger)
		{
			this.breakpoint = breakpoint;
		}
	}
}
