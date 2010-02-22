﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Threading;

using ICSharpCode.Core;

namespace Debugger.AddIn.TreeModel
{
	public static partial class Utils
	{
		/// <param name="process">Process on which to track debuggee state</param>
		public static void DoEvents(Process process)
		{
			if (process == null) return;
			DebuggeeState oldState = process.DebuggeeState;
			WpfDoEvents();
			DebuggeeState newState = process.DebuggeeState;
			if (oldState != newState) {
				LoggingService.Info("Aborted because debuggee resumed");
				throw new AbortedBecauseDebuggeeResumedException();
			}
		}
		
		public static void WpfDoEvents()
		{
			DispatcherFrame frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
			Dispatcher.PushFrame(frame);
		}
	}
	
	public class AbortedBecauseDebuggeeResumedException: System.Exception
	{
		public AbortedBecauseDebuggeeResumedException(): base()
		{
			
		}
	}
	
	public class PrintTimes: PrintTime
	{
		public PrintTimes(string text): base(text + " - end")
		{
			LoggingService.InfoFormatted("{0} - start", text);
		}
	}
	
	public class PrintTime: IDisposable
	{
		string text;
		Stopwatch stopwatch = new Stopwatch();
		
		public PrintTime(string text)
		{
			this.text = text;
			stopwatch.Start();
		}
		
		public void Dispose()
		{
			stopwatch.Stop();
			LoggingService.InfoFormatted("{0} ({1} ms)", text, stopwatch.ElapsedMilliseconds);
		}
	}
}
