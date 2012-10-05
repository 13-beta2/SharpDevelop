﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.OptionPanels;
using ICSharpCode.SharpDevelop.Project.Converter;

namespace ICSharpCode.SharpDevelop.Project
{
	public sealed class DefaultProjectBehavior : ProjectBehavior
	{
		public DefaultProjectBehavior(AbstractProject project)
			: base(project)
		{
		}
		
		new AbstractProject Project {
			get {
				return (AbstractProject)base.Project;
			}
		}
		
		public override bool IsStartable {
			get { return false; }
		}
		
		public override void Start(bool withDebugging)
		{
			ProcessStartInfo psi;
			try {
				// we have to call CreateStartInfo through IProject, because otherwise the
				// project behavior chain would not be processed!
				psi = Project.CreateStartInfo();
			} catch (ProjectStartException ex) {
				MessageService.ShowError(ex.Message);
				return;
			}
			if (withDebugging) {
				DebuggerService.CurrentDebugger.Start(psi);
			} else {
				DebuggerService.CurrentDebugger.StartWithoutDebugging(psi);
			}
		}
		
		public override ProcessStartInfo CreateStartInfo()
		{
			throw new NotSupportedException();
		}
		
		public override ItemType GetDefaultItemType(string fileName)
		{
			return ItemType.None;
		}
		
		public override ProjectItem CreateProjectItem(IProjectItemBackendStore item)
		{
			return new UnknownProjectItem(Project, item);
		}
		
		public override void ProjectCreationComplete()
		{
			
		}
		
		public override IEnumerable<CompilerVersion> GetAvailableCompilerVersions()
		{
			return Enumerable.Empty<CompilerVersion>();
		}
		
		public override void UpgradeProject(CompilerVersion newVersion, TargetFramework newFramework)
		{
			throw new NotSupportedException();
		}
		
		
		/// <summary>
		/// Saves project preferences (currently opened files, bookmarks etc.) to the
		/// a property container.
		/// </summary>
		public override Properties CreateMemento()
		{
			SD.MainThread.VerifyAccess();
			
			// breakpoints and files
			Properties properties = new Properties();
			properties.SetList("bookmarks", SD.BookmarkManager.GetProjectBookmarks(Project));
			List<string> files = new List<string>();
			foreach (string fileName in FileService.GetOpenFiles()) {
				if (fileName != null && Project.IsFileInProject(fileName)) {
					files.Add(fileName);
				}
			}
			properties.SetList("files", files);
			
			// other project data
			properties.SetNestedProperties("projectSavedData", Project.ProjectSpecificProperties.Clone());
			
			return properties;
		}
		
		public override void SetMemento(Properties memento)
		{
			SD.MainThread.VerifyAccess();
			
			foreach (var mark in memento.GetList<ICSharpCode.SharpDevelop.Editor.Bookmarks.SDBookmark>("bookmarks")) {
				SD.BookmarkManager.AddMark(mark);
			}
			List<string> filesToOpen = new List<string>();
			foreach (string fileName in memento.GetList<string>("files")) {
				if (File.Exists(fileName)) {
					filesToOpen.Add(fileName);
				}
			}
			System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
				System.Windows.Threading.DispatcherPriority.Background,
				new Action(
					delegate {
						foreach (string file in filesToOpen)
							FileService.OpenFile(file);
					}));
			
			base.SetMemento(memento);
		}
	}
}
