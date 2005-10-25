﻿// <file>
//     <copyright see="prj:///doc/copyright.txt">2002-2005 AlphaSierraPapa</copyright>
//     <license see="prj:///doc/license.txt">GNU General Public License</license>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Diagnostics;
using System.IO;

namespace ICSharpCode.SharpAssembly.PE {
	
	public class PEFileHeader
	{
		const ushort machineSign = 0x14C;
		const ushort IMAGE_FILE_DLL = 0x2000;
		
		ushort numberOfSections;
		uint   time;
		uint   ptrToSymbolTable;
		uint   numerOfSymbols;
		ushort optionalHeaderSize;
		ushort characteristics;
		
		// optional header:
		StandardFields   standardFields    = new StandardFields();
		NTSpecificFields ntSpecificFields  = new NTSpecificFields();
		DataDirectories  dataDirectories   = new DataDirectories();
		
		public ushort NumberOfSections {
			get {
				return numberOfSections;
			}
			set {
				numberOfSections = value;
			}
		}
		
		public uint Time {
			get {
				return time;
			}
			set {
				time = value;
			}
		}
		
		public uint PtrToSymbolTable {
			get {
				return ptrToSymbolTable;
			}
			set {
				ptrToSymbolTable = value;
			}
		}
		
		public uint NumerOfSymbols {
			get {
				return numerOfSymbols;
			}
			set {
				numerOfSymbols = value;
			}
		}
		
		public ushort OptionalHeaderSize {
			get {
				return optionalHeaderSize;
			}
			set {
				optionalHeaderSize = value;
			}
		}
		
		public bool IsDLL {
			get {
				return (characteristics & IMAGE_FILE_DLL) == IMAGE_FILE_DLL;
			}
			set {
				if (value) {
					characteristics |= IMAGE_FILE_DLL;
				} else {
					characteristics = (ushort)(characteristics & ~IMAGE_FILE_DLL);
				}
			}
		}
		
		public StandardFields StandardFields {
			get {
				return standardFields;
			}
		}
		
		public NTSpecificFields NtSpecificFields {
			get {
				return ntSpecificFields;
			}
		}
		
		public DataDirectories DataDirectories {
			get {
				return dataDirectories;
			}
		}
		
		
		public void LoadFrom(BinaryReader binaryReader)
		{
			// pe signature (always PE\0\0)
			byte[] signature = new byte[4];
			binaryReader.Read(signature, 0, 4);
			if (signature[0] != (byte)'P' && signature[1] != (byte)'E' && signature[2] != 0 && signature[3] != 0) {
				Console.WriteLine("NO PE FILE");
				return;
			}
			ushort machine = binaryReader.ReadUInt16();
			
			if (machine != machineSign) {
				Console.WriteLine("Wrong machine : " + machineSign);
				return;
			}
			
			numberOfSections = binaryReader.ReadUInt16();
			time             = binaryReader.ReadUInt32();
			
			ptrToSymbolTable = binaryReader.ReadUInt32();
			if (ptrToSymbolTable != 0) {
				Console.WriteLine("warning: ptrToSymbolTable != 0");
			}
			
			numerOfSymbols = binaryReader.ReadUInt32();
			if (numerOfSymbols != 0) {
				Console.WriteLine("warning: numerOfSymbols != 0");
			}
			
			optionalHeaderSize = binaryReader.ReadUInt16();
			characteristics    = binaryReader.ReadUInt16();
			
			standardFields.LoadFrom(binaryReader);
			ntSpecificFields.LoadFrom(binaryReader);
			dataDirectories.LoadFrom(binaryReader);
		}
	}
}
