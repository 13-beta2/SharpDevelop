// <file>
//     <owner name="David Srbeck�" email="dsrbecky@post.cz"/>
// </file>

using System;

namespace DebuggerLibrary 
{	
	public delegate void DebuggingIsResumingEventHandler (object sender, DebuggingIsResumingEventArgs e);
	
	[Serializable]
	public class DebuggingIsResumingEventArgs : System.EventArgs 
	{
		bool abort;
		
		public bool Abort {
			get {
				return abort;
			}
			set {
				abort = value;
			}
		}
		
		public DebuggingIsResumingEventArgs()
		{
			this.abort = false;
		}
	}
}
