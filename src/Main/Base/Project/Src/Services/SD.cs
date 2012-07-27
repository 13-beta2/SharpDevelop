﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using ICSharpCode.Core;
using ICSharpCode.Core.Implementation;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Parser;

namespace ICSharpCode.SharpDevelop
{
	/// <summary>
	/// Static entry point for retrieving SharpDevelop services.
	/// </summary>
	public static class SD
	{
		/// <summary>
		/// Gets the main service container for SharpDevelop.
		/// </summary>
		public static IServiceContainer Services {
			get { return GetRequiredService<IServiceContainer>(); }
		}
		
		/// <summary>
		/// Initializes the services for unit testing.
		/// This will replace the whole service container with a new container that
		/// contains only the following services:
		/// - ILoggingService (logging to Diagnostics.Trace)
		/// - IMessageService (writing to Console.Out)
		/// - PropertyService gets initialized with empty in-memory property container
		/// </summary>
		public static void InitializeForUnitTests()
		{
			var container = new SharpDevelopServiceContainer(ServiceSingleton.FallbackServiceProvider);
			ServiceSingleton.ServiceProvider = container;
		}
		
		/// <summary>
		/// Gets a service. Returns null if service is not found.
		/// </summary>
		public static T GetService<T>() where T : class
		{
			return ServiceSingleton.ServiceProvider.GetService<T>();
		}
		
		/// <summary>
		/// Gets a service. Returns null if service is not found.
		/// </summary>
		public static T GetRequiredService<T>() where T : class
		{
			return ServiceSingleton.ServiceProvider.GetRequiredService<T>();
		}
		
		/// <summary>
		/// Gets the workbench.
		/// </summary>
		public static IWorkbench Workbench {
			get { return GetRequiredService<IWorkbench>(); }
		}
		
		public static IMessageLoop MainThread {
			get { return GetRequiredService<IMessageLoop>(); }
		}
		
		/// <summary>
		/// Gets the status bar.
		/// </summary>
		public static IStatusBarService StatusBar {
			get { return GetRequiredService<IStatusBarService>(); }
		}
		
		public static ILoggingService LoggingService {
			get { return GetRequiredService<ILoggingService>(); }
		}
		
		public static IMessageService MessageService {
			get { return GetRequiredService<IMessageService>(); }
		}
		
		public static IPropertyService PropertyService {
			get { return GetRequiredService<IPropertyService>(); }
		}
		
		public static IEditorControlService EditorControlService {
			get { return GetRequiredService<IEditorControlService>(); }
		}
		
		public static IAnalyticsMonitor AnalyticsMonitor {
			get { return GetRequiredService<IAnalyticsMonitor>(); }
		}
		
		public static IParserService ParserService {
			get { return GetRequiredService<IParserService>(); }
		}
		
		public static IAssemblyParserService AssemblyParserService {
			get { return GetRequiredService<IAssemblyParserService>(); }
		}
		
		public static IFileService FileService {
			get { return GetRequiredService<IFileService>(); }
		}
		
		public static IGlobalAssemblyCacheService GlobalAssemblyCache {
			get { return GetRequiredService<IGlobalAssemblyCacheService>(); }
		}
		
		public static IAddInTree AddInTree {
			get { return GetRequiredService<IAddInTree>(); }
		}
	}
}
