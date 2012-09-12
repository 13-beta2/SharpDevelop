﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels
{
	/// <summary>
	/// The Output Window options panel.
	/// </summary>
	public class OutputWindowOptionsPanel : XmlFormsOptionPanel
	{
		public static readonly string OutputWindowsProperty = "SharpDevelop.UI.OutputWindowOptions";
		FontSelectionPanel fontSelectionPanel;
		
		public OutputWindowOptionsPanel()
		{
		}
		
		
		public override void LoadPanelContents()
		{
			SetupFromXmlStream(this.GetType().Assembly.GetManifestResourceStream("ICSharpCode.SharpDevelop.Resources.OutputWindowOptionsPanel.xfrm"));
			
			Properties properties = PropertyService.NestedProperties(OutputWindowsProperty);
			fontSelectionPanel = new FontSelectionPanel();
			fontSelectionPanel.Dock = DockStyle.Fill;
			ControlDictionary["FontGroupBox"].Controls.Add(fontSelectionPanel);
			((CheckBox)ControlDictionary["wordWrapCheckBox"]).Checked = properties.Get("WordWrap", true);
			
			fontSelectionPanel.CurrentFontString = properties.Get("DefaultFont", SD.WinForms.DefaultMonospacedFont.ToString()).ToString();
		}
		
		public override bool StorePanelContents()
		{
			Properties properties = PropertyService.NestedProperties(OutputWindowsProperty);
			properties.Set("WordWrap", ((CheckBox)ControlDictionary["wordWrapCheckBox"]).Checked);
			string currentFontString = fontSelectionPanel.CurrentFontString;
			if (currentFontString != null)
				properties.Set("DefaultFont", currentFontString);
			
			return true;
		}
	}
}
