﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using ICSharpCode.AvalonEdit.Document;
using System.Threading;

namespace ICSharpCode.AvalonEdit.Xml
{
	/// <summary>
	/// Creates object tree from XML document.
	/// </summary>
	/// <remarks>
	/// The created tree fully describes the document and thus the orginal XML file can be
	/// exactly reproduced.
	/// 
	/// Any further parses will reparse only the changed parts and the existing tree will
	/// be updated with the changes.  The user can add event handlers to be notified of
	/// the changes.  The parser tries to minimize the number of changes to the tree.
	/// (for example, it will add a single child at the start of collection rather than
	/// clearing the collection and adding new children)
	/// 
	/// The object tree consists of following types:
	///   RawObject - Abstact base class for all types
	///     RawContainer - Abstact base class for all types that can contain child nodes
	///       RawDocument - The root object of the XML document
	///       RawElement - Logical grouping of other nodes together.  The first child is always the start tag.
	///       RawTag - Represents any markup starting with "&lt;" and (hopefully) ending with ">"
	///     RawAttribute - Name-value pair in a tag
	///     RawText - Whitespace or character data
	/// 
	/// For example, see the following XML and the produced object tree:
	/// <![CDATA[
	///   <!-- My favourite quote -->
	///   <quote author="Albert Einstein">
	///     Make everything as simple as possible, but not simpler.
	///   </quote>
	/// 
	///   RawDocument
	///     RawTag "<!--" "-->"
	///       RawText " My favourite quote "
	///     RawElement
	///       RawTag "<" "quote" ">"
	///         RawText " "
	///         RawAttribute 'author="Albert Einstein"'
	///       RawText "\n  Make everything as simple as possible, but not simpler.\n"
	///       RawTag "</" "quote" ">"
	/// ]]>
	/// 
	/// The precise content of RawTag depends on what it represents:
	/// <![CDATA[
	///   Start tag:  "<"  Name?  (RawText+ RawAttribute)* RawText* (">" | "/>")
	///   End tag:    "</" Name?  (RawText+ RawAttribute)* RawText* ">"
	///   P.instr.:   "<?" Name?  (RawText)* "?>"
	///   Comment:    "<!--"      (RawText)* "-->"
	///   CData:      "<![CDATA[" (RawText)* "]]" ">"
	///   DTD:        "<!DOCTYPE" (RawText+ RawTag)* RawText* ">"    (DOCTYPE or other DTD names)
	///   UknownBang: "<!"        (RawText)* ">"
	/// ]]>
	/// 
	/// The type of tag can be identified by the opening backet.
	/// There are helpper properties in the RawTag class to identify the type, exactly
	/// one of the properties will be true.
	/// 
	/// The closing bracket may be missing or may be different for mallformed XML.
	/// 
	/// Note that there can always be multiple consequtive RawText nodes.
	/// This is to ensure that idividual texts are not too long.
	/// 
	/// XML Spec:  http://www.w3.org/TR/xml/
	/// XML EBNF:  http://www.jelks.nu/XML/xmlebnf.html
	/// 
	/// Internals:
	/// 
	/// "Try" methods can silently fail by returning false.
	/// MoveTo methods do not move if they are already at the given target
	/// If methods return some object, it must be no-empty.  It is up to the caller to ensure
	/// the context is appropriate for reading.
	/// 
	/// </remarks>
	public class AXmlParser
	{
		string input;
		AXmlDocument userDocument;
		TextDocument textDocument;
		
		internal TrackedSegmentCollection TrackedSegments { get; private set; }
		ChangeTrackingCheckpoint lastCheckpoint;
		ReaderWriterLockSlim lockObject;
		
		/// <summary>
		/// Generate syntax error when seeing enity reference other then the build-in ones
		/// </summary>
		public bool UknonwEntityReferenceIsError { get; set; }
		
		/// <summary> Create new parser </summary>
		public AXmlParser(string input)
		{
			this.input = input;
			this.UknonwEntityReferenceIsError = true;
			this.TrackedSegments = new TrackedSegmentCollection();
			this.lockObject = new ReaderWriterLockSlim();
			
			this.userDocument = new AXmlDocument() { Parser = this };
			this.userDocument.Document = this.userDocument;
			// Track the document
			this.TrackedSegments.AddParsedObject(this.userDocument, null);
			this.userDocument.IsCached = false;
		}
		
		/// <summary>
		/// Create new parser, but do not parse the text yet.
		/// </summary>
		public AXmlParser(TextDocument textDocument)
			: this(textDocument.Text)
		{
			this.textDocument = textDocument;
		}
		
		/// <summary> Throws exception if condition is false </summary>
		internal static void Assert(bool condition, string message)
		{
			if (!condition) {
				throw new Exception("Assertion failed: " + message);
			}
		}

		/// <summary> Throws exception if condition is false </summary>
		[Conditional("DEBUG")]
		internal static void DebugAssert(bool condition, string message)
		{
			if (!condition) {
				throw new Exception("Assertion failed: " + message);
			}
		}
		
		internal static void Log(string text, params object[] pars)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("XML: " + text, pars));
		}
		
		public AXmlDocument Parse()
		{
			if (!Lock.IsWriteLockHeld)
				throw new InvalidOperationException("Lock needed!");
			
			if (textDocument != null) { // incremental parse
				ChangeTrackingCheckpoint checkpoint;
				input = textDocument.CreateSnapshot(out checkpoint).Text;
				
				// Use changes to invalidate cache
				if (lastCheckpoint != null) {
					var changes = lastCheckpoint.GetChangesTo(checkpoint);
					if (!changes.Any())
						return userDocument;
					this.TrackedSegments.UpdateOffsetsAndInvalidate(changes);
				}
				lastCheckpoint = checkpoint;
			}
			
			TagReader tagReader = new TagReader(this, input);
			List<AXmlObject> tags = tagReader.ReadAllTags();
			AXmlDocument parsedDocument = new TagMatchingHeuristics(this, input, tags).ReadDocument();
			tagReader.PrintStringCacheStats();
			AXmlParser.Log("Updating main DOM tree...");
			userDocument.UpdateTreeFrom(parsedDocument);
			userDocument.DebugCheckConsistency(true);
			Assert(userDocument.GetSelfAndAllChildren().Count() == parsedDocument.GetSelfAndAllChildren().Count(), "Parsed document and updated document have different number of children");
			return userDocument;
		}
		
		/// <summary>
		/// Makes calls to Parse() thread-safe. Use Lock everywhere Parse() is called.
		/// </summary>
		public ReaderWriterLockSlim Lock {
			get { return this.lockObject; }
		}
	}
}
