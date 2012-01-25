﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.TypeSystem.TestCase;
using ICSharpCode.NRefactory.Utils;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Parser
{
	[TestFixture]
	public class TypeSystemConvertVisitorTests : TypeSystemTests
	{
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			compilation = ParseTestCase().CreateCompilation();
		}
		
		internal static IProjectContent ParseTestCase()
		{
			const string fileName = "TypeSystemTests.TestCase.cs";
			
			CSharpParser parser = new CSharpParser();
			CompilationUnit cu;
			using (Stream s = typeof(TypeSystemTests).Assembly.GetManifestResourceStream(typeof(TypeSystemTests), fileName)) {
				cu = parser.Parse(s, fileName);
			}
			
			var parsedFile = cu.ToTypeSystem();
			return new CSharpProjectContent()
				.UpdateProjectContent(null, parsedFile)
				.AddAssemblyReferences(new[] { CecilLoaderTests.Mscorlib })
				.SetAssemblyName(typeof(TypeSystemTests).Assembly.GetName().Name);
		}
		
		[Test]
		public void ExplicitDisposableImplementation()
		{
			ITypeDefinition disposable = GetTypeDefinition(typeof(NRefactory.TypeSystem.TestCase.ExplicitDisposableImplementation));
			IMethod method = disposable.Methods.Single(m => m.Name == "Dispose");
			Assert.IsTrue(method.IsExplicitInterfaceImplementation);
			Assert.AreEqual("System.IDisposable.Dispose", method.InterfaceImplementations.Single().FullName);
		}
		
		[Test]
		public void ExplicitGenericInterfaceImplementation()
		{
			ITypeDefinition impl = GetTypeDefinition(typeof(NRefactory.TypeSystem.TestCase.ExplicitGenericInterfaceImplementation));
			IType genericInterfaceOfString = compilation.FindType(typeof(IGenericInterface<string>));
			IMethod implMethod1 = impl.Methods.Single(m => m.Name == "Test" && !m.Parameters[1].IsRef);
			IMethod implMethod2 = impl.Methods.Single(m => m.Name == "Test" && m.Parameters[1].IsRef);
			Assert.IsTrue(implMethod1.IsExplicitInterfaceImplementation);
			Assert.IsTrue(implMethod2.IsExplicitInterfaceImplementation);
			
			IMethod interfaceMethod1 = (IMethod)implMethod1.InterfaceImplementations.Single();
			Assert.AreEqual(genericInterfaceOfString, interfaceMethod1.DeclaringType);
			Assert.IsTrue(!interfaceMethod1.Parameters[1].IsRef);
			
			IMethod interfaceMethod2 = (IMethod)implMethod2.InterfaceImplementations.Single();
			Assert.AreEqual(genericInterfaceOfString, interfaceMethod2.DeclaringType);
			Assert.IsTrue(interfaceMethod2.Parameters[1].IsRef);
		}
	}
	
	[TestFixture]
	public class SerializedTypeSystemConvertVisitorTests : TypeSystemTests
	{
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			FastSerializer serializer = new FastSerializer();
			using (MemoryStream ms = new MemoryStream()) {
				serializer.Serialize(ms, TypeSystemConvertVisitorTests.ParseTestCase());
				ms.Position = 0;
				var pc = (IProjectContent)serializer.Deserialize(ms);
				compilation = pc.CreateCompilation();
			}
		}
	}
}
