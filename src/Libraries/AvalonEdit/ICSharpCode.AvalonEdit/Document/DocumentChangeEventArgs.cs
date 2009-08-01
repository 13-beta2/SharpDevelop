// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.AvalonEdit.Utils;
using System;

namespace ICSharpCode.AvalonEdit.Document
{
	/// <summary>
	/// Describes a change of the document text.
	/// This class is thread-safe.
	/// </summary>
	[Serializable]
	public class DocumentChangeEventArgs : EventArgs
	{
		/// <summary>
		/// The offset at which the change occurs.
		/// </summary>
		public int Offset { get; private set; }
		
		/// <summary>
		/// The number of characters removed.
		/// </summary>
		public int RemovalLength { get; private set; }
		
		/// <summary>
		/// The text that was inserted.
		/// </summary>
		public string InsertedText { get; private set; }
		
		/// <summary>
		/// The number of characters inserted.
		/// </summary>
		public int InsertionLength {
			get { return InsertedText.Length; }
		}
		
		volatile OffsetChangeMap offsetChangeMap;
		
		/// <summary>
		/// Gets the OffsetChangeMap associated with this document change.
		/// </summary>
		/// <remarks>The OffsetChangeMap instance is guaranteed to be frozen and thus thread-safe.</remarks>
		public OffsetChangeMap OffsetChangeMap {
			get {
				OffsetChangeMap map = offsetChangeMap;
				if (map == null) {
					// create OffsetChangeMap on demand
					map = OffsetChangeMap.FromSingleElement(CreateSingleChangeMapEntry());
					offsetChangeMap = map;
				}
				return map;
			}
		}
		
		internal OffsetChangeMapEntry CreateSingleChangeMapEntry()
		{
			return new OffsetChangeMapEntry(this.Offset, this.RemovalLength, this.InsertionLength);
		}
		
		/// <summary>
		/// Gets the OffsetChangeMap, or null if the default offset map (=single replacement) is being used.
		/// </summary>
		internal OffsetChangeMap OffsetChangeMapOrNull {
			get {
				return offsetChangeMap;
			}
		}
		
		/// <summary>
		/// Gets the new offset where the specified offset moves after this document change.
		/// </summary>
		public int GetNewOffset(int offset, AnchorMovementType movementType)
		{
			if (offsetChangeMap != null)
				return offsetChangeMap.GetNewOffset(offset, movementType);
			else
				return CreateSingleChangeMapEntry().GetNewOffset(offset, movementType);
		}
		
		/// <summary>
		/// Creates a new DocumentChangeEventArgs object.
		/// </summary>
		public DocumentChangeEventArgs(int offset, int removalLength, string insertedText)
			: this(offset, removalLength, insertedText, null)
		{
		}
		
		/// <summary>
		/// Creates a new DocumentChangeEventArgs object.
		/// </summary>
		public DocumentChangeEventArgs(int offset, int removalLength, string insertedText, OffsetChangeMap offsetChangeMap)
		{
			ThrowUtil.CheckNotNegative(offset, "offset");
			ThrowUtil.CheckNotNegative(removalLength, "removalLength");
			ThrowUtil.CheckNotNull(insertedText, "insertedText");
			
			this.Offset = offset;
			this.RemovalLength = removalLength;
			this.InsertedText = insertedText;
			
			if (offsetChangeMap != null) {
				if (!offsetChangeMap.IsFrozen)
					throw new ArgumentException("The OffsetChangeMap must be frozen before it can be used in DocumentChangeEventArgs");
				if (!offsetChangeMap.IsValidForDocumentChange(offset, removalLength, insertedText.Length))
					throw new ArgumentException("OffsetChangeMap is not valid for this document change", "offsetChangeMap");
				this.offsetChangeMap = offsetChangeMap;
			}
		}
	}
}
