// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Kr�ger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.SharpDevelop.Editor;
using System;
using System.Drawing;
using ICSharpCode.NRefactory;
using ICSharpCode.SharpDevelop.Dom.Refactoring;

namespace SearchAndReplace
{
	public class SearchResultMatch
	{
		ProvidedDocumentInformation providedDocumentInformation;
		int    offset;
		int    length;
		
		public string FileName {
			get {
				return providedDocumentInformation.FileName;
			}
		}
		
		public ProvidedDocumentInformation ProvidedDocumentInformation {
			set {
				providedDocumentInformation = value;
			}
		}
		
		public int Offset {
			get {
				return offset;
			}
		}
		
		public int Length {
			get {
				return length;
			}
		}
		
		public virtual string TransformReplacePattern(string pattern)
		{
			return pattern;
		}
		
		public IDocument CreateDocument()
		{
			return providedDocumentInformation.Document;
		}
		
		public SearchResultMatch(int offset, int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException("length");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");
			this.offset   = offset;
			this.length   = length;
		}
		
		public virtual Location GetStartPosition(IDocument document)
		{
			return document.OffsetToPosition(Math.Min(Offset, document.TextLength));
		}
		
		public virtual Location GetEndPosition(IDocument document)
		{
			return document.OffsetToPosition(Math.Min(Offset + Length, document.TextLength));
		}
		
		/// <summary>
		/// Gets a special text to display, or null to display the line's content.
		/// </summary>
		public virtual string DisplayText {
			get {
				return null;
			}
		}
		
		public override string ToString()
		{
			return String.Format("[{3}: FileName={0}, Offset={1}, Length={2}]",
			                     FileName, Offset, Length,
			                     GetType().Name);
		}
	}
	
	public class SimpleSearchResultMatch : SearchResultMatch
	{
		Location position;
		
		public override Location GetStartPosition(IDocument doc)
		{
			return position;
		}
		
		public override Location GetEndPosition(IDocument doc)
		{
			return position;
		}
		
		string displayText;
		
		public override string DisplayText {
			get {
				return displayText;
			}
		}
		
		public SimpleSearchResultMatch(string displayText, Location position) : base(0, 0)
		{
			this.position = position;
			this.displayText = displayText;
		}
	}
}
