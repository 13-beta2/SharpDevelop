﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;

namespace ICSharpCode.AvalonEdit.Xml
{
	/// <summary>
	/// Holds all valid parsed items.
	/// Also tracks their offsets as document changes.
	/// </summary>
	class Cache
	{
		/// <summary> Previously parsed items as long as they are valid  </summary>
		TextSegmentCollection<AXmlObject> parsedItems = new TextSegmentCollection<AXmlObject>();
		
		/// <summary>
		/// Is used to identify what memory range was touched by object
		/// The default is (StartOffset, EndOffset + 1) which is not stored
		/// </summary>
		TextSegmentCollection<TouchedMemoryRange> touchedMemoryRanges = new TextSegmentCollection<TouchedMemoryRange>();
		
		/// <summary>
		/// Track offsets of syntax errors as well.
		/// Objects can not change in the cache, so not need to update it.
		/// </summary>
		TextSegmentCollection<SyntaxError> syntaxErrors = new TextSegmentCollection<SyntaxError>();
		
		class TouchedMemoryRange: TextSegment
		{
			public AXmlObject TouchedByObject { get; set; }
		}
		
		public void UpdateOffsetsAndInvalidate(IEnumerable<DocumentChangeEventArgs> changes)
		{
			foreach(DocumentChangeEventArgs change in changes) {
				// Update offsets of all items
				parsedItems.UpdateOffsets(change);
				touchedMemoryRanges.UpdateOffsets(change);
				syntaxErrors.UpdateOffsets(change);
				
				// Remove any items affected by the change
				AXmlParser.Log("Changed offset {0}", change.Offset);
				// Removing will cause one of the ends to be set to change.Offset
				// FindSegmentsContaining includes any segments touching
				// so that conviniently takes care of the +1 byte
				foreach(AXmlObject obj in parsedItems.FindSegmentsContaining(change.Offset)) {
					Remove(obj, false);
				}
				foreach(TouchedMemoryRange memory in touchedMemoryRanges.FindSegmentsContaining(change.Offset)) {
					AXmlParser.Log("Found that {0} dependeds on memory ({1}-{2})", memory.TouchedByObject, memory.StartOffset, memory.EndOffset);
					Remove(memory.TouchedByObject, true);
					touchedMemoryRanges.Remove(memory);
				}
			}
		}
		
		/// <summary> Add object to cache, optionally adding extra memory tracking </summary>
		public void Add(AXmlObject obj, int? maxTouchedLocation)
		{
			AXmlParser.Assert(obj.Length > 0 || obj is AXmlDocument, string.Format("Invalid object {0}.  It has zero length.", obj));
			if (obj is AXmlContainer) {
				int objStartOffset = obj.StartOffset;
				int objEndOffset = obj.EndOffset;
				foreach(AXmlObject child in ((AXmlContainer)obj).Children) {
					AXmlParser.Assert(objStartOffset <= child.StartOffset && child.EndOffset <= objEndOffset, "Wrong nesting");
				}
			}
			parsedItems.Add(obj);
			foreach(SyntaxError syntaxError in obj.SyntaxErrors) {
				syntaxErrors.Add(syntaxError);
			}
			obj.IsInCache = true;
			if (maxTouchedLocation != null) {
				// location is assumed to be read so the range ends at (location + 1)
				// For example eg for "a_" it is (0-2)
				TouchedMemoryRange memRange = new TouchedMemoryRange() {
					StartOffset = obj.StartOffset,
					EndOffset = maxTouchedLocation.Value + 1,
					TouchedByObject = obj
				};
				touchedMemoryRanges.Add(memRange);
				AXmlParser.Log("{0} touched memory range ({1}-{2})", obj, memRange.StartOffset, memRange.EndOffset);
			}
		}
		
		List<AXmlObject> FindParents(AXmlObject child)
		{
			List<AXmlObject> parents = new List<AXmlObject>();
			foreach(AXmlObject parent in parsedItems.FindSegmentsContaining(child.StartOffset)) {
				// Parent is anyone wholy containg the child
				if (parent.StartOffset <= child.StartOffset && child.EndOffset <= parent.EndOffset && parent != child) {
					parents.Add(parent);
				}
			}
			return parents;
		}
		
		/// <summary> Remove from cache </summary>
		public void Remove(AXmlObject obj, bool includeParents)
		{
			if (includeParents) {
				List<AXmlObject> parents = FindParents(obj);
				
				foreach(AXmlObject r in parents) {
					if (parsedItems.Remove(r)) {
						foreach(SyntaxError syntaxError in r.SyntaxErrors) {
							syntaxErrors.Remove(syntaxError);
						}
						r.IsInCache = false;
						AXmlParser.Log("Removing cached item {0} (it is parent)", r);
					}
				}
			}
			
			if (parsedItems.Remove(obj)) {
				foreach(SyntaxError syntaxError in obj.SyntaxErrors) {
					syntaxErrors.Remove(syntaxError);
				}
				obj.IsInCache = false;
				AXmlParser.Log("Removed cached item {0}", obj);
			}
		}
		
		public T GetObject<T>(int offset, int lookaheadCount, Predicate<T> conditon) where T: AXmlObject, new()
		{
			AXmlObject obj = parsedItems.FindFirstSegmentWithStartAfter(offset);
			while(obj != null && offset <= obj.StartOffset && obj.StartOffset <= offset + lookaheadCount) {
				if (obj is T && conditon((T)obj)) {
					return (T)obj;
				}
				obj = parsedItems.GetNextSegment(obj);
			}
			return null;
		}
	}
}
