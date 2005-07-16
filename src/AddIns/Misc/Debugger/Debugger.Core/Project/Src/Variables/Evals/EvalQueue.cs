// <file>
//     <owner name="David Srbeck�" email="dsrbecky@post.cz"/>
// </file>

using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;

using DebuggerInterop.Core;
using DebuggerInterop.MetaData;

namespace DebuggerLibrary
{
	class EvalQueue
	{
		ArrayList waitingEvals = new ArrayList();
		
		public event EventHandler AllEvalsComplete;
		
		public void AddEval(Eval eval)
		{
			waitingEvals.Add(eval);
		}
		
		public void PerformAllEvals()
		{
			while (waitingEvals.Count > 0) {
				PerformNextEval();
			}
		}
		
		public void PerformNextEval()
		{
			if (waitingEvals.Count == 0) {
				throw new DebuggerException("No eval in queue to perform.");
			}
			Eval eval = (Eval)waitingEvals[0];
			waitingEvals.Remove(eval);
			eval.PerformEval();
			
			if (waitingEvals.Count == 0) {
				if (AllEvalsComplete != null) {
					AllEvalsComplete(null, EventArgs.Empty);
				}
			}
		}
	}
}
