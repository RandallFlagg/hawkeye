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
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace ACorns.Hawkeye.Core.Options
{
	/// <summary>
	/// Summary description for CoreApplicationOptions.
	/// </summary>
	public sealed class CoreApplicationOptions
	{
		#region Instance

		private static CoreApplicationOptions instance = new CoreApplicationOptions();

		/// <summary>
		/// Singleton instance of the ApplicationOptions.
		/// </summary>
		public static CoreApplicationOptions Instance
		{
			get { return instance; }
		}

		#endregion

		private bool allowSelectOwnedObjects = false;
		private bool allowInjectInOtherProcesses = true;
		private bool isInjected = false;
		private bool injectBasedOnRuntimeVersion = false;
		private bool saveGeneratedAssembly = false;
		
		private bool automaticExtenderMonitorAndAttach = true;

		private bool advancedFeatures = true;

        private string appExeName = "ACorns.Hawkeye.exe";

		private CoreApplicationOptions()
		{

		}



		public bool AllowSelectOwnedObjects
		{
			get { return allowSelectOwnedObjects; }
			set { allowSelectOwnedObjects = value; }
		}

		public bool AllowInjectInOtherProcesses
		{
			get { return allowInjectInOtherProcesses; }
			set { allowInjectInOtherProcesses = value; }
		}

		public bool InjectBasedOnRuntimeVersion
		{
			get { return injectBasedOnRuntimeVersion; }
			set { injectBasedOnRuntimeVersion = value; }
		}

		public bool IsInjected
		{
			get { return isInjected; }
			set { isInjected = value; }
		}

		public bool SaveGeneratedAssembly
		{
			get { return saveGeneratedAssembly; }
			set { saveGeneratedAssembly = value; }
		}

		public bool AutomaticExtenderMonitorAndAttach
		{
			get { return automaticExtenderMonitorAndAttach; }
			set { automaticExtenderMonitorAndAttach = value; }
		}

		public bool AdvancedFeatures
		{
			get { return advancedFeatures; }
			set { advancedFeatures = value; }
		}

		public string MutexName
		{
			get { return "Hawkeye.Inject." + Process.GetCurrentProcess().Id; }
		}

        public string AppExeName
        {
            get { return this.appExeName; }
            set { this.appExeName = value; }
        }

        public static string FolderName
        {
            get
            {
#if X64
				return Path.GetDirectoryName(typeof(CoreApplicationOptions).Assembly.Location) + "..\\";
#else
                return Path.GetDirectoryName(typeof(CoreApplicationOptions).Assembly.Location);
#endif
            }
        }
	}
}
