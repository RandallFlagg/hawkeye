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
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ACorns.Hawkeye.Utils.Menus
{
	/// <summary>
	/// Summary description for CustomMenuItem.
	/// </summary>
	internal class CustomMenuItem : MenuItem
	{
		private readonly PropertyTab ownerTab;

		public CustomMenuItem(PropertyTab ownerTab, string text, EventHandler handler)
			: base(text, handler)
		{
			this.ownerTab = ownerTab;
		}

		public PropertyTab OwnerTab
		{
			get { return ownerTab; }
		}
	}
}
