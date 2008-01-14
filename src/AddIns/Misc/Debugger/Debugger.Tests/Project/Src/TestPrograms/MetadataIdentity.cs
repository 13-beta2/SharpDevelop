// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbeck�" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;

namespace Debugger.Tests.TestPrograms
{
	public class MetadataIdentity
	{
		public static void Main()
		{
			new MetadataIdentity().Func();
		}
		
		public void Func()
		{
			System.Diagnostics.Debugger.Break();
			System.Diagnostics.Debugger.Break();
		}
	}
}

#if TESTS
namespace Debugger.Tests {
	using NUnit.Framework;
	
	public partial class DebuggerTests
	{
		[NUnit.Framework.Test]
		public void MetadataIdentity()
		{
			StartTest("MetadataIdentity");
			WaitForPause();
			
			DebugType type = process.SelectedStackFrame.ThisValue.Type;
			MethodInfo mainMethod = process.SelectedStackFrame.MethodInfo;
			process.Continue();
			WaitForPause();
			
			Assert.AreEqual(type, process.SelectedStackFrame.ThisValue.Type);
			Assert.AreEqual(mainMethod, process.SelectedStackFrame.MethodInfo);
			process.Continue();
			process.WaitForExit();
			CheckXmlOutput();
		}
	}
}
#endif
