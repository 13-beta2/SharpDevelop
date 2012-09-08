﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

using CSharpBinding.FormattingStrategy;
using CSharpBinding.Refactoring;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Refactoring;

namespace CSharpBinding
{
	/// <summary>
	/// Description of CSharpLanguageBinding.
	/// </summary>
	public class CSharpLanguageBinding : DefaultLanguageBinding
	{
		public override IFormattingStrategy FormattingStrategy {
			get { return new CSharpFormattingStrategy(); }
		}
		
//		public override LanguageProperties Properties {
//			get { return LanguageProperties.CSharp; }
//		}
//
		public override IBracketSearcher BracketSearcher {
			get { return new CSharpBracketSearcher(); }
		}
		
		ITextEditor editor;
		CSharpSemanticHighlighter semanticHighlighter;
		IssueManager inspectionManager;
		IList<IContextActionProvider> contextActionProviders;
		
		public override void Attach(ITextEditor editor)
		{
			base.Attach(editor);
			this.editor = editor;
			inspectionManager = new IssueManager(editor);
			//codeManipulation = new CodeManipulation(editor);
			
			if (!editor.ContextActionProviders.IsReadOnly) {
				contextActionProviders = AddInTree.BuildItems<IContextActionProvider>("/SharpDevelop/ViewContent/TextEditor/C#/ContextActions", null);
				editor.ContextActionProviders.AddRange(contextActionProviders);
			}
		}
		
		public override void Detach()
		{
			//codeManipulation.Dispose();
			IHighlighter highlighter = editor.GetService(typeof(IHighlighter)) as IHighlighter;
			if (inspectionManager != null) {
				inspectionManager.Dispose();
				inspectionManager = null;
			}
			if (contextActionProviders != null) {
				editor.ContextActionProviders.RemoveWhere(contextActionProviders.Contains);
			}
			this.editor = null;
			base.Detach();
		}
	}
}
