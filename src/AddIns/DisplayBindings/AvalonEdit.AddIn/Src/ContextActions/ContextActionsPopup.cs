﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Windows.Controls.Primitives;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.AvalonEdit.AddIn.ContextActions
{
	/// <summary>
	/// Description of ContextActionsPopup.
	/// </summary>
	public class ContextActionsPopup : ContextActionsPopupBase
	{
		/// <summary>
		/// DOM Entity (IClass, IMember etc.) for which this popup is displayed.
		/// </summary>
		public IEntity Symbol { get; set; }
		
		public ContextActionsPopup()
		{
			// Close on lost focus
			this.StaysOpen = false;
			this.AllowsTransparency = true;
			this.ActionsControl = new ContextActionsHeaderedControl();
			// Close when any action excecuted
			this.ActionsControl.ActionExecuted += delegate { this.Close(); };
		}
		
		public ContextActionsHeaderedControl ActionsControl
		{
			get { return (ContextActionsHeaderedControl)this.Child; }
			set { this.Child = value; }
		}
		
		public ContextActionsViewModel Actions
		{
			get { return (ContextActionsViewModel)ActionsControl.DataContext; }
			set {
				ActionsControl.DataContext = value;
			}
		}
		
		public new void Focus()
		{
			this.ActionsControl.Focus();
		}
		
		public void OpenAtCaretAndFocus()
		{
			ITextEditor currentEditor = GetCurrentEditor();
			if (currentEditor == null) {
				OpenAtMousePosition();
				return;
			}
			OpenAtPosition(currentEditor, currentEditor.Caret.Line, currentEditor.Caret.Column, true);
			this.Focus();
		}

		ITextEditor GetCurrentEditor()
		{
			return SD.GetActiveViewContentService<ITextEditor>();
		}
		
		void OpenAtMousePosition()
		{
			this.Placement = PlacementMode.MousePoint;
			this.Open();
		}
	}
}
