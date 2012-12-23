﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using ICSharpCode.AvalonEdit.AddIn.ContextActions;
using ICSharpCode.AvalonEdit.AddIn.Options;
using ICSharpCode.AvalonEdit.AddIn.Snippets;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Editor.AvalonEdit;
using ICSharpCode.SharpDevelop.Editor.Commands;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Parser;
using ICSharpCode.SharpDevelop.Refactoring;

namespace ICSharpCode.AvalonEdit.AddIn
{
	/// <summary>
	/// The text editor use inside the CodeEditor.
	/// There can be two CodeEditorView instances in a single CodeEditor if split-view
	/// is enabled.
	/// </summary>
	public class CodeEditorView : SharpDevelopTextEditor, IDisposable
	{
		public ITextEditor Adapter { get; set; }
		
		BracketHighlightRenderer bracketRenderer;
		//CaretReferencesRenderer caretReferencesRenderer;
		ContextActionsRenderer contextActionsRenderer;
		HiddenDefinition.HiddenDefinitionRenderer hiddenDefinitionRenderer;
		
		public CodeEditorView()
		{
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelpExecuted));
			
			this.bracketRenderer = new BracketHighlightRenderer(this.TextArea.TextView);
			//this.caretReferencesRenderer = new CaretReferencesRenderer(this);
			this.contextActionsRenderer = new ContextActionsRenderer(this);
			this.hiddenDefinitionRenderer = new HiddenDefinition.HiddenDefinitionRenderer(this);
			
			UpdateCustomizedHighlighting();
			
			this.MouseHover += TextEditorMouseHover;
			this.MouseHoverStopped += TextEditorMouseHoverStopped;
			this.MouseLeave += TextEditorMouseLeave;
			this.TextArea.TextView.MouseDown += TextViewMouseDown;
			this.TextArea.Caret.PositionChanged += HighlightBrackets;
			this.TextArea.TextView.VisualLinesChanged += CodeEditorView_VisualLinesChanged;
			
			SetupTabSnippetHandler();
		}

		void CodeEditorView_VisualLinesChanged(object sender, EventArgs e)
		{
			// hide tooltip
			if (this.toolTip != null)
				this.toolTip.IsOpen = false;
		}
		
		public virtual void Dispose()
		{
			contextActionsRenderer.Dispose();
			hiddenDefinitionRenderer.Dispose();
		}
		
		public IList<IContextActionProvider> ContextActionProviders {
			get { return contextActionsRenderer.Providers; }
		}
		
		protected override ICSharpCode.Core.FileName FileName {
			get { return this.Adapter.FileName; }
		}
		
		protected override void OnOptionChanged(PropertyChangedEventArgs e)
		{
			base.OnOptionChanged(e);
			switch (e.PropertyName) {
				case "HighlightBrackets":
					HighlightBrackets(null, e);
					break;
				case "EnableFolding":
					UpdateParseInformationForFolding();
					break;
				case "HighlightSymbol":
					//if (this.caretReferencesRenderer != null)
					//	this.caretReferencesRenderer.ClearHighlight();
					break;
			}
		}
		
		#region CaretPositionChanged - Bracket Highlighting
		/// <summary>
		/// Highlights matching brackets.
		/// </summary>
		void HighlightBrackets(object sender, EventArgs e)
		{
			/*
			 * Special case: ITextEditor.Language guarantees that it never returns null.
			 * In this case however it can be null, since this code may be called while the document is loaded.
			 * ITextEditor.Language gets set in CodeEditorAdapter.FileNameChanged, which is called after
			 * loading of the document has finished.
			 * */
			if (this.Adapter.Language != null) {
				if (CodeEditorOptions.Instance.HighlightBrackets || CodeEditorOptions.Instance.ShowHiddenDefinitions) {
					var bracketSearchResult = this.Adapter.Language.BracketSearcher.SearchBracket(this.Adapter.Document, this.TextArea.Caret.Offset);
					if (CodeEditorOptions.Instance.HighlightBrackets) {
						this.bracketRenderer.SetHighlight(bracketSearchResult);
					} else {
						this.bracketRenderer.SetHighlight(null);
					}
					if (CodeEditorOptions.Instance.ShowHiddenDefinitions) {
						this.hiddenDefinitionRenderer.BracketSearchResult = bracketSearchResult;
						this.hiddenDefinitionRenderer.Show();
					} else {
						this.hiddenDefinitionRenderer.ClosePopup();
					}
				} else {
					this.bracketRenderer.SetHighlight(null);
					this.hiddenDefinitionRenderer.ClosePopup();
				}
			}
		}
		#endregion
		
		#region Custom Tab command (code snippet expansion)
		void SetupTabSnippetHandler()
		{
			var editingKeyBindings = this.TextArea.DefaultInputHandler.Editing.InputBindings.OfType<KeyBinding>();
			var tabBinding = editingKeyBindings.Single(b => b.Key == Key.Tab && b.Modifiers == ModifierKeys.None);
			this.TextArea.DefaultInputHandler.Editing.InputBindings.Remove(tabBinding);
			var newTabBinding = new KeyBinding(new CustomTabCommand(this, tabBinding.Command), tabBinding.Key, tabBinding.Modifiers);
			this.TextArea.DefaultInputHandler.Editing.InputBindings.Add(newTabBinding);
		}
		
		sealed class CustomTabCommand : ICommand
		{
			CodeEditorView editor;
			ICommand baseCommand;
			
			public CustomTabCommand(CodeEditorView editor, ICommand baseCommand)
			{
				this.editor = editor;
				this.baseCommand = baseCommand;
			}
			
			public event EventHandler CanExecuteChanged {
				add {}
				remove {}
			}
			
			public bool CanExecute(object parameter)
			{
				return true;
			}
			
			public void Execute(object parameter)
			{
				if (editor.SelectionLength == 0) {
					int wordStart = DocumentUtilitites.FindPrevWordStart(editor.Adapter.Document, editor.CaretOffset);
					if (wordStart > 0) {
						string word = editor.Adapter.Document.GetText(wordStart, editor.CaretOffset - wordStart);
						CodeSnippet snippet = SnippetManager.Instance.FindSnippet(Path.GetExtension(editor.Adapter.FileName),
						                                                          word);
						if (snippet != null) {
							snippet.TrackUsage("CustomTabCommand");
							
							using (editor.Document.RunUpdate()) {
								editor.Adapter.Document.Remove(wordStart, editor.CaretOffset - wordStart);
								snippet.CreateAvalonEditSnippet(editor.Adapter).Insert(editor.TextArea);
							}
							return;
						}
					}
				}
				baseCommand.Execute(parameter);
			}
		}
		#endregion
		
		#region OnKeyDown
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled && e.Key == Key.Escape && e.KeyboardDevice.Modifiers == ModifierKeys.None) {
				if (this.SelectionLength > 0) {
					this.SelectionLength = 0;
					e.Handled = true;
				}
			}
		}
		#endregion
		
		#region Help
		void OnHelpExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ShowHelp();
		}
		
		public void ShowHelp()
		{
			// Resolve expression at cursor and show help
			var compilation = SD.ParserService.GetCompilationForFile(FileName);
			var result = SD.ParserService.Resolve(Adapter, TextArea.Caret.Location, compilation);
			TypeResolveResult trr = result as TypeResolveResult;
			if (trr != null && trr.Type.GetDefinition() != null) {
				HelpProvider.ShowHelp(trr.Type.GetDefinition());
			}
			MemberResolveResult mrr = result as MemberResolveResult;
			if (mrr != null) {
				HelpProvider.ShowHelp(mrr.Member);
			}
		}
		#endregion
		
		#region Tooltip
		ToolTip toolTip;
		Popup popupToolTip;
		
		void TextEditorMouseHover(object sender, MouseEventArgs e)
		{
			Debug.Assert(sender == this);
			ToolTipRequestEventArgs args = new ToolTipRequestEventArgs(this.Adapter);
			var pos = this.TextArea.TextView.GetPositionFloor(e.GetPosition(this.TextArea.TextView) + this.TextArea.TextView.ScrollOffset);
			args.InDocument = pos.HasValue;
			if (pos.HasValue) {
				args.LogicalPosition = pos.Value.Location;
			}
			
			if (args.InDocument) {
				int offset = args.Editor.Document.GetOffset(args.LogicalPosition);
				
				FoldingManager foldings = this.Adapter.GetService(typeof(FoldingManager)) as FoldingManager;
				if (foldings != null) {
					var foldingsAtOffset = foldings.GetFoldingsAt(offset);
					FoldingSection collapsedSection = foldingsAtOffset.FirstOrDefault(section => section.IsFolded);
					
					if (collapsedSection != null) {
						args.SetToolTip(GetTooltipTextForCollapsedSection(collapsedSection));
					}
				}
				
				TextMarkerService textMarkerService = this.Adapter.GetService(typeof(ITextMarkerService)) as TextMarkerService;
				if (textMarkerService != null) {
					var markersAtOffset = textMarkerService.GetMarkersAtOffset(offset);
					ITextMarker markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);
					
					if (markerWithToolTip != null) {
						args.SetToolTip(markerWithToolTip.ToolTip);
					}
				}
			}
			
			if (!args.Handled) {
				// if request wasn't handled by a marker, pass it to the ToolTipRequestService
				ToolTipRequestService.RequestToolTip(args);
			}
			
			if (!TryCloseExistingPopup(false)) {
				return;
			}
			
			if (args.ContentToShow != null) {
				popupToolTip = args.ContentToShow as Popup;
				
				if (popupToolTip != null) {
					var popupPosition = GetPopupPosition(e);
					popupToolTip.HorizontalOffset = popupPosition.X;
					popupToolTip.VerticalOffset = popupPosition.Y;
					popupToolTip.StaysOpen = true;  // We will close it ourselves
					
					e.Handled = true;
					popupToolTip.IsOpen = true;
				} else {
					if (toolTip == null) {
						toolTip = new ToolTip();
						toolTip.Closed += ToolTipClosed;
					}
					toolTip.PlacementTarget = this; // required for property inheritance
					
					if(args.ContentToShow is string) {
						toolTip.Content = new TextBlock
						{
							Text = args.ContentToShow as string,
							TextWrapping = TextWrapping.Wrap
						};
					}
					else
						toolTip.Content = args.ContentToShow;
					
					e.Handled = true;
					toolTip.IsOpen = true;
				}
			}
		}
		
		bool TryCloseExistingPopup(bool mouseClick)
		{
			if (popupToolTip != null) {
				if (popupToolTip.IsOpen && !mouseClick && popupToolTip is ITooltip && !((ITooltip)popupToolTip).CloseOnHoverEnd) {
					return false; // Popup does not want to be closed yet
				}
				popupToolTip.IsOpen = false;
				popupToolTip = null;
			}
			return true;
		}
		
		/// <summary> Returns Popup position based on mouse position, in device independent units </summary>
		Point GetPopupPosition(MouseEventArgs mouseArgs)
		{
			Point mousePos = mouseArgs.GetPosition(this);
			Point positionInPixels;
			// align Popup with line bottom
			TextViewPosition? logicalPos = GetPositionFromPoint(mousePos);
			if (logicalPos.HasValue) {
				var textView = this.TextArea.TextView;
				positionInPixels =
					textView.PointToScreen(
						textView.GetVisualPosition(logicalPos.Value, VisualYPosition.LineBottom) - textView.ScrollOffset);
				positionInPixels.X -= 4;
			} else {
				positionInPixels = PointToScreen(mousePos + new Vector(-4, 6));
			}
			// use device independent units, because Popup Left/Top are in independent units
			return positionInPixels.TransformFromDevice(this);
		}
		
		void TextEditorMouseHoverStopped(object sender, MouseEventArgs e)
		{
			if (toolTip != null) {
				toolTip.IsOpen = false;
				e.Handled = true;
			}
		}
		
		void TextEditorMouseLeave(object sender, MouseEventArgs e)
		{
			if (popupToolTip != null && !popupToolTip.IsMouseOver) {
				// do not close popup if mouse moved from editor to popup
				TryCloseExistingPopup(false);
			}
		}

		void ToolTipClosed(object sender, RoutedEventArgs e)
		{
			toolTip = null;
		}
		
		#region GetTooltipTextForCollapsedSection
		string GetTooltipTextForCollapsedSection(FoldingSection foldingSection)
		{
			// This fixes SD-1394:
			// Each line is checked for leading indentation whitespaces. If
			// a line has the same or more indentation than the first line,
			// it is reduced. If a line is less indented than the first line
			// the indentation is removed completely.
			//
			// See the following example:
			// 	line 1
			// 		line 2
			// 			line 3
			//  line 4
			//
			// is reduced to:
			// line 1
			// 	line 2
			// 		line 3
			// line 4
			
			const int maxLineCount = 15;
			
			TextDocument document = this.Document;
			int startOffset = foldingSection.StartOffset;
			int endOffset = foldingSection.EndOffset;
			
			DocumentLine startLine = document.GetLineByOffset(startOffset);
			DocumentLine endLine = document.GetLineByOffset(endOffset);
			StringBuilder builder = new StringBuilder();
			
			DocumentLine current = startLine;
			ISegment startIndent = TextUtilities.GetLeadingWhitespace(document, startLine);
			int lineCount = 0;
			while (current != endLine.NextLine && lineCount < maxLineCount) {
				ISegment currentIndent = TextUtilities.GetLeadingWhitespace(document, current);
				
				if (current == startLine && current == endLine)
					builder.Append(document.GetText(startOffset, endOffset - startOffset));
				else if (current == startLine) {
					if (current.EndOffset - startOffset > 0)
						builder.AppendLine(document.GetText(startOffset, current.EndOffset - startOffset).TrimStart());
				} else if (current == endLine) {
					if (startIndent.Length <= currentIndent.Length)
						builder.Append(document.GetText(current.Offset + startIndent.Length, endOffset - current.Offset - startIndent.Length));
					else
						builder.Append(document.GetText(current.Offset + currentIndent.Length, endOffset - current.Offset - currentIndent.Length));
				} else {
					if (startIndent.Length <= currentIndent.Length)
						builder.AppendLine(document.GetText(current.Offset + startIndent.Length, current.Length - startIndent.Length));
					else
						builder.AppendLine(document.GetText(current.Offset + currentIndent.Length, current.Length - currentIndent.Length));
				}
				
				current = current.NextLine;
				lineCount++;
			}
			if (current != endLine.NextLine)
				builder.Append("...");
			
			return builder.ToString();
		}
		#endregion
		#endregion
		
		#region Ctrl+Click Go To Definition
		void TextViewMouseDown(object sender, MouseButtonEventArgs e)
		{
			// close existing debugger popup immediately on text editor mouse down
			TryCloseExistingPopup(false);
			
			if (options.CtrlClickGoToDefinition && e.ChangedButton == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.Control) {
				// Ctrl+Click Go to definition
				var position = GetPositionFromPoint(e.GetPosition(this));
				if (position == null)
					return;
				SD.AnalyticsMonitor.TrackFeature(typeof(GoToDefinition).FullName, "Ctrl+Click");
				var resolveResult = SD.ParserService.Resolve(Adapter, position.Value.Location);
				var goToDefinitionCommand = new GoToDefinition();
				goToDefinitionCommand.Run(resolveResult);
				e.Handled = true;
			}
		}
		#endregion
		
		public void JumpTo(int line, int column)
		{
			// closes Debugger popup on debugger step
			TryCloseExistingPopup(true);
			
			// the adapter sets the caret position and takes care of scrolling
			this.Adapter.JumpTo(line, column);
			this.Focus();
			
			if (CodeEditorOptions.Instance.EnableAnimations)
				Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)DisplayCaretHighlightAnimation);
		}
		
		void DisplayCaretHighlightAnimation()
		{
			TextArea textArea = Adapter.GetService(typeof(TextArea)) as TextArea;
			
			if (textArea == null)
				return;
			
			AdornerLayer layer = AdornerLayer.GetAdornerLayer(textArea.TextView);
			
			if (layer == null)
				return;
			
			CaretHighlightAdorner adorner = new CaretHighlightAdorner(textArea);
			layer.Add(adorner);
			
			SD.MainThread.CallLater(TimeSpan.FromSeconds(1), () => layer.Remove(adorner));
		}
		
		#region UpdateParseInformation - Folding
		void UpdateParseInformationForFolding()
		{
			UpdateParseInformationForFolding(null);
		}
		
		bool disableParseInformationFolding;
		
		public bool DisableParseInformationFolding {
			get { return disableParseInformationFolding; }
			set {
				if (disableParseInformationFolding != value) {
					disableParseInformationFolding = value;
					UpdateParseInformationForFolding();
				}
			}
		}
		
		public void UpdateParseInformationForFolding(ParseInformation parseInfo)
		{
			if (!CodeEditorOptions.Instance.EnableFolding || disableParseInformationFolding) {
				parseInfo = null;
			} else {
				if (parseInfo == null || !parseInfo.IsFullParseInformation)
					parseInfo = SD.ParserService.Parse(this.FileName, this.Document);
			}
			
			IServiceContainer container = this.Adapter.GetService(typeof(IServiceContainer)) as IServiceContainer;
			ParserFoldingStrategy folding = container.GetService(typeof(ParserFoldingStrategy)) as ParserFoldingStrategy;
			if (parseInfo == null) {
				if (folding != null) {
					folding.Dispose();
					container.RemoveService(typeof(ParserFoldingStrategy));
				}
			} else {
				if (folding == null) {
					TextArea textArea = this.Adapter.GetService(typeof(TextArea)) as TextArea;
					folding = new ParserFoldingStrategy(textArea);
					container.AddService(typeof(ParserFoldingStrategy), folding);
				}
				folding.UpdateFoldings(parseInfo);
			}
		}
		#endregion
		
		protected override IVisualLineTransformer CreateColorizer(IHighlightingDefinition highlightingDefinition)
		{
			return null;
		}
		
		// TODO: move this into SharpDevelopTextEditor
		public void UpdateCustomizedHighlighting()
		{
			string language = this.SyntaxHighlighting != null ? this.SyntaxHighlighting.Name : null;
			CustomizableHighlightingColorizer.ApplyCustomizationsToDefaultElements(this, FetchCustomizations(language));
			BracketHighlightRenderer.ApplyCustomizationsToRendering(this.bracketRenderer, FetchCustomizations(language));
			HighlightingOptions.ApplyToRendering(this, FetchCustomizations(language));
			this.TextArea.TextView.Redraw(); // manually redraw if default elements didn't change but customized highlightings did
		}
		
		static IEnumerable<CustomizedHighlightingColor> FetchCustomizations(string languageName)
		{
			return CustomizedHighlightingColor.FetchCustomizations(languageName);
		}
	}
}
