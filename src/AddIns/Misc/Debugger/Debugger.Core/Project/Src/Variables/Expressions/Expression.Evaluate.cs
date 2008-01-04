// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbeck�" email="dsrbecky@gmail.com"/>
//     <version>$Revision: 2285 $</version>
// </file>

using System;
using System.Collections.Generic;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PrettyPrinter;
using Ast = ICSharpCode.NRefactory.Ast;
using Debugger.Wrappers.CorSym;

namespace Debugger
{
	public partial class Expression: DebuggerObject
	{
		public Value Evaluate(Process process)
		{
			if (process != null &&
			    process.SelectedThread != null &&
			    process.SelectedThread.SelectedStackFrame != null)
			{
				return Evaluate(process.SelectedThread.SelectedStackFrame);
			} else {
				return null;
			}
		}
		
		public Value Evaluate(StackFrame stackFrame)
		{
			EvaluateAstVisitor astVisitor = new EvaluateAstVisitor(stackFrame);
			return (Value)this.AbstractSynatxTree.AcceptVisitor(astVisitor, null);
		}
	}
}
