/* ****************************************************************************
 *  Hawkeye - The .Net Runtime Object Editor
 * 
 * Copyright (c) 2005 Corneliu I. Tusnea
 * 
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author be held liable for any damages arising from 
 * the use of this software.
 * Permission to use, copy, modify, distribute and sell this software for any 
 * purpose is hereby granted without fee, provided that the above copyright 
 * notice appear in all copies and that both that copyright notice and this 
 * permission notice appear in supporting documentation.
 * 
 * Corneliu I. Tusnea (corneliutusnea@yahoo.com.au)
 * http://www.acorns.com.au/hawkeye/
 * ****************************************************************************/


using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ACorns.Hawkeye.Utils;
using ACorns.Hawkeye.Utils.Menus;
using ACorns.Hawkeye.Options;
using ACorns.Hawkeye.Core.Utils;
using Microsoft.Win32;

namespace ACorns.Hawkeye.Tabs.ProcessInfo
{
	/// <summary>
	/// ProcessInfoTab - Tab showing all the methods of an object
	/// </summary>
	internal class ProcessInfoTab : PropertyTab, ICustomMenuHandler
	{
		private ApplicationInfo cachedApplicationInfo;
		private object cachedComponentInfo;

		public ProcessInfoTab() { }

		public override Bitmap Bitmap
		{
			get { return SystemUtils.LoadBitmap( "Tabs.ProcessInfo.bmp"); }
		}

		public override string TabName
		{
			get { return "ProcessInfo"; }
		}

		public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
		{
			return GetProperties(null, component, attributes);
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
		{
			if (context.PropertyDescriptor is RemapPropertyDescriptor)
			{
				RemapPropertyDescriptor remapDescriptor = context.PropertyDescriptor as RemapPropertyDescriptor;
				if (remapDescriptor.Name == "CurrentApplication")
				{
					PropertyDescriptorCollection realProperties = DescriptorUtils.GetStaticProperties(typeof (Application));
					PropertyDescriptorCollection remappedProperties = DescriptorUtils.RemapComponent(realProperties, component, null, null, new ChildTypeConverter());
					return remappedProperties;
				}
				else
				{
					PropertyDescriptorCollection realProperties = ReadProperties(null, component, attributes, component, null);
					return realProperties;
				}
			}
			else
			{
				if (cachedComponentInfo != component)
				{
					cachedApplicationInfo = new ApplicationInfo();
					cachedComponentInfo = component;
				}

				PropertyDescriptorCollection result = ReadProperties(context, component, attributes, cachedApplicationInfo, null);
				//PropertyDescriptorCollection appProps = PropertyDescriptorUtils.GetStaticProperties(typeof(Application));
				//result = PropertyDescriptorUtils.MergeProperties(result, appProps);

				return result;
			}

			//temp = ReadProperties(context, component, attributes, cachedApplicationInfo.CurrentPrincipal, "CurrentPrincipal");
			//result = PropertyDescriptorUtils.MergeProperties(result, temp);

			//temp = ReadProperties(context, component, attributes, cachedApplicationInfo.CurrentPrincipal.Identity, "CurrentIdentity");
			//result = PropertyDescriptorUtils.MergeProperties(result, temp);

			//temp = ReadProperties(context, component, attributes, cachedApplicationInfo.CurrentPrincipal, "CurrentPrincipal");
			//result = PropertyDescriptorUtils.MergeProperties(result, temp);

			//temp = ReadProperties(context, component, attributes, cachedApplicationInfo.CurrentThread, "CurrentThread");
			//result = PropertyDescriptorUtils.MergeProperties(result, temp);

			//temp = ReadProperties(context, component, attributes, cachedApplicationInfo.CurrentProcess, "CurrentProcess");
			//result = PropertyDescriptorUtils.MergeProperties(result, temp);
		}

		private PropertyDescriptorCollection ReadProperties(ITypeDescriptorContext context, object realComponent, Attribute[] attributes, object realObject, string name)
		{
			PropertyDescriptorCollection remappedProperties;
			PropertyDescriptorCollection realProperties = DescriptorUtils.GetAllProperties(context, realObject, attributes);
			remappedProperties = DescriptorUtils.RemapComponent(realProperties, realComponent, realObject, name, new ChildTypeConverter());
			return remappedProperties;
		}
        
		#region ICustomMenuHandler Members

		public void RegisterMenuItems(System.Windows.Forms.ContextMenuStrip contextMenu) { }

		#endregion
	}
}