﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace ICSharpCode.AvalonEdit.Utils
{
	// Creates TextFormatter instances that with the correct TextFormattingMode, if running on .NET 4.0.
	static class TextFormatterFactory
	{
		readonly static DependencyProperty TextFormattingModeProperty;
		
		static TextFormatterFactory()
		{
			Assembly presentationFramework = typeof(FrameworkElement).Assembly;
			Type textOptionsType = presentationFramework.GetType("System.Windows.Media.TextOptions", false);
			if (textOptionsType != null) {
				TextFormattingModeProperty = textOptionsType.GetField("TextFormattingModeProperty").GetValue(null) as DependencyProperty;
			}
		}
		
		public static TextFormatter Create(DependencyObject owner)
		{
			// return TextFormatter.Create(TextOptions.GetTextFormattingMode(this));
			if (TextFormattingModeProperty != null) {
				object formattingMode = owner.GetValue(TextFormattingModeProperty);
				return (TextFormatter)typeof(TextFormatter).InvokeMember(
					"Create",
					BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
					null, null,
					new object[] { formattingMode });
			} else {
				return TextFormatter.Create();
			}
		}
		
		public static bool PropertyChangeAffectsTextFormatter(DependencyProperty dp)
		{
			// return dp == TextOptions.TextFormattingModeProperty;
			return dp == TextFormattingModeProperty && TextFormattingModeProperty != null;
		}
	}
}
