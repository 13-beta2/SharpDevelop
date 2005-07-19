// <file>
//     <owner name="David Srbeck�" email="dsrbecky@post.cz"/>
// </file>

using System;

namespace DebuggerLibrary 
{	
	public delegate void MessageEventHandler (object sender, MessageEventArgs e);
	
	[Serializable]
	public class MessageEventArgs : DebuggerEventArgs
	{
		string message;
		
		public string Message {
			get {
				return message;
			}
		}
		
		public MessageEventArgs(NDebugger debugger, string message): base(debugger)
		{
			this.message = message;
		}
	}
}
