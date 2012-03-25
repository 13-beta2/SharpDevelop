﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.Dialogs.ReferenceDialog.ServiceReference
{
	public class ProjectWithServiceReferences : IProjectWithServiceReferences
	{
		IProject project;
		string serviceReferencesFolder;
		
		public static readonly string DefaultServiceReferencesFolderName = "Service References";
		
		public ProjectWithServiceReferences(IProject project)
		{
			this.project = project;
		}
		
		public string ServiceReferencesFolder {
			get {
				if (serviceReferencesFolder == null) {
					GetServiceReferencesFolder();
				}
				return serviceReferencesFolder;
			}
		}
		
		void GetServiceReferencesFolder()
		{
			serviceReferencesFolder = Path.Combine(project.Directory, DefaultServiceReferencesFolderName);
		}
		
		public string Language {
			get { return project.Language; }
		}
		
		public ServiceReferenceFileName GetServiceReferenceFileName(string serviceReferenceName)
		{
			return new ServiceReferenceFileName(ServiceReferencesFolder, serviceReferenceName);
		}
		
		public ServiceReferenceMapFileName GetServiceReferenceMapFileName(string serviceReferenceName)
		{
			return new ServiceReferenceMapFileName(ServiceReferencesFolder, serviceReferenceName);
		}
		
		public void AddServiceReferenceProxyFile(ServiceReferenceFileName fileName)
		{
			AddServiceReferenceFileToProject(fileName);
			AddServiceReferencesItemToProject();
			AddServiceReferenceItemToProject(fileName);
		}
		
		void AddServiceReferenceFileToProject(ServiceReferenceFileName fileName)
		{
			var projectItem = new FileProjectItem(project, ItemType.Compile);
			projectItem.FileName = fileName.Path;
			projectItem.DependentUpon = "Reference.svcmap";
			AddProjectItemToProject(projectItem);
		}
		
		void AddProjectItemToProject(ProjectItem item)
		{
			ProjectService.AddProjectItem(project, item);
		}
		
		void AddServiceReferencesItemToProject()
		{
			var projectItem = new ServiceReferencesProjectItem(project);
			projectItem.Include = "Service References";
			AddProjectItemToProject(projectItem);
		}
		
		void AddServiceReferenceItemToProject(ServiceReferenceFileName fileName)
		{
			var projectItem = new ServiceReferenceProjectItem(project);
			projectItem.Include = @"Service References\" + fileName.ServiceName;
			AddProjectItemToProject(projectItem);
		}
		
		public void Save()
		{
			project.Save();
		}
		
		public void AddServiceReferenceMapFile(ServiceReferenceMapFileName fileName)
		{
			var projectItem = new ServiceReferenceMapFileProjectItem(project, fileName.Path);
			AddProjectItemToProject(projectItem);
		}
		
		public void AddAssemblyReference(string referenceName)
		{
			if (!AssemblyReferenceExists(referenceName)) {
				var projectItem = new ReferenceProjectItem(project, referenceName);
				AddProjectItemToProject(projectItem);
			}
		}
		
		bool AssemblyReferenceExists(string referenceName)
		{
			return project
				.GetItemsOfType(ItemType.Reference)
				.Any(item => IsAssemblyReferenceMatch((ReferenceProjectItem)item, referenceName));
		}
		
		bool IsAssemblyReferenceMatch(ReferenceProjectItem item, string referenceName)
		{
			return IsMatchIgnoringCase(item.AssemblyName.ShortName, referenceName);
		}
		
		static bool IsMatchIgnoringCase(string a, string b)
		{
			return String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}
	}
}
