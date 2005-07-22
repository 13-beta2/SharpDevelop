﻿// <file>
//     <copyright see="prj:///doc/copyright.txt">2002-2005 AlphaSierraPapa</copyright>
//     <license see="prj:///doc/license.txt">GNU General Public License</license>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Windows.Forms;
using System.Drawing;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpDevelop.Bookmarks;

namespace ICSharpCode.Core
{
	public class BreakpointBookmark: SDBookmark
	{
		object tag;

		public object Tag {
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}

		public BreakpointBookmark(string fileName, ICSharpCode.TextEditor.Document.IDocument document, int lineNumber) : base(fileName, document, lineNumber)
		{

		}
		
		public override void Draw(ICSharpCode.TextEditor.IconBarMargin margin, Graphics g, Point p)
		{
			margin.DrawBreakpoint(g, p.Y, IsEnabled);
		}
	}
}
