﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Debugger.AddIn.TreeModel;
using ICSharpCode.NRefactory;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Gui.Pads;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Services;
using ICSharpCode.TreeView;

namespace Debugger.AddIn.Pads.Controls
{
	public class SharpTreeNodeAdapter : SharpTreeNode
	{
		public SharpTreeNodeAdapter(TreeNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");
			this.Node = node;
			this.LazyLoading = true;
		}
		
		public TreeNode Node { get; private set; }
		
		public override object Icon {
			get { return this.Node.Image != null ? this.Node.Image.ImageSource : null; }
		}
		
		public override bool ShowExpander {
			get { return this.Node.GetChildren != null; }
		}
		
		protected override void LoadChildren()
		{
			if (this.Node.GetChildren != null) {
				var process = WindowsDebugger.CurrentProcess;
				process.EnqueueWork(Dispatcher.CurrentDispatcher, () => Children.AddRange(this.Node.GetChildren().Select(node => node.ToSharpTreeNode())));
			}
		}
		
		/*
		public override bool CanDrop(System.Windows.DragEventArgs e, int index)
		{
			e.Effects = DragDropEffects.None;
			if (e.Data.GetDataPresent(DataFormats.StringFormat)) {
				e.Effects = DragDropEffects.Copy;
				return true;
			}
			return false;
		}
		
		public override void Drop(DragEventArgs e, int index)
		{
			if (ProjectService.CurrentProject == null) return;
			if (e.Data == null) return;
			if (!e.Data.GetDataPresent(DataFormats.StringFormat)) return;
			if (string.IsNullOrEmpty(e.Data.GetData(DataFormats.StringFormat).ToString())) return;
			
			string language = ProjectService.CurrentProject.Language;
			
			var text = new TreeNode(e.Data.GetData(DataFormats.StringFormat).ToString(), null);

			var node = text.ToSharpTreeNode();
			if (!WatchPad.Instance.WatchList.WatchItems.Any(n => text.Name == ((SharpTreeNodeAdapter)n).Node.Name))
				WatchPad.Instance.WatchList.WatchItems.Add(node);
			
			WindowsDebugger.RefreshPads();
		}
		*/
	}
}
