// <file>
//     <owner name="David Srbeck�" email="dsrbecky@post.cz"/>
// </file>

namespace DebuggerLibrary
{
	public enum PausedReason:int
	{
		StepComplete,
		Breakpoint,
		Break,
		ControlCTrap,
		Exception,
		DebuggerError,
		CurrentThreadChanged
	}
}
