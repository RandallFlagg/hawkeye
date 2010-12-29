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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ACorns.Hawkeye.Options;
using ACorns.Hawkeye.Tabs.Events;
using ACorns.Hawkeye.Tabs.Fields;
using ACorns.Hawkeye.Tabs.Methods;
using ACorns.Hawkeye.Tabs.ProcessInfo;
using ACorns.Hawkeye.Tabs.Properties;
using ACorns.Hawkeye.Tabs.Toolbar;
using ACorns.Hawkeye.Tools.Reflector;
using ACorns.Hawkeye.Utils;
using ACorns.Hawkeye.Utils.Menus;
using ACorns.Hawkeye.Core.Utils.Accessors;

namespace ACorns.Hawkeye.Tabs
{
    internal delegate void SelectedObjectRequestHandler(object newObject);

    internal class XPropertyGrid : PropertyGrid
    {
        public event SelectedObjectRequestHandler SelectRequest;
        public event EventHandler AddToolbarButtons;

        #region Context Menu
        private ContextMenu contextMenu;
        private MenuItem selectThisItem;
        private MenuItem showSourceCodeForItem;
        private MenuItem goBackOneItem;
        private MenuItem goForwardOneItem;
        #endregion

        private ArrayList historyObjects = new ArrayList();
        private int activeObject = -1;

        private ToolBar externalToolBar;
        private ToolBar gridToolBar;

        private FieldAccesor gridViewAccessor;
        private FieldAccesor peMainAccessor;
        private MethodAccesor recursivelyExpandAccessor;

        private ToolBarButton btnRefresh;
        private ToolBarButton btnCollapseAll;
        private ToolBarButton btnExpandAll;
        private ToolBarButton btnShowSourceCode;

        private ToolBarButton btnAddExtender;

        private ToolBarButton btnHighlightWindow;
        private WindowProperties highlightWindowProperties = new WindowProperties();

        public XPropertyGrid()
        {
            InitializeComponent();
        }

        #region Component Designer generated code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            this.ResumeLayout(false);
            this.SelectedObjectsChanged += new EventHandler(XPropertyGrid_SelectedObjectsChanged);
            this.PropertyTabChanged += new PropertyTabChangedEventHandler(XPropertyGrid_PropertyTabChanged);
        }

        #endregion

        #region Context Menu
        private void InitContextMenu()
        {
            contextMenu = new ContextMenu();
            goBackOneItem = new MenuItem("Back");
            goForwardOneItem = new MenuItem("Forward");
            selectThisItem = new MenuItem("Select");
            showSourceCodeForItem = new MenuItem("Show source code");

            selectThisItem.Click += new EventHandler(selectThisItem_Click);
            showSourceCodeForItem.Click += new EventHandler(showSourceCodeForItem_Click);
            goBackOneItem.Click += new EventHandler(goBackOneItem_Click);
            goForwardOneItem.Click += new EventHandler(goForwardOneItem_Click);

            contextMenu.MenuItems.AddRange(new MenuItem[] { selectThisItem, showSourceCodeForItem, new MenuItem("-"), goBackOneItem, goForwardOneItem });

            this.ContextMenu = contextMenu;
        }
        #endregion

        #region Properties

        public override bool CanShowCommands
        {
            get { return true; }
        }

        public override bool CommandsVisibleIfAvailable
        {
            get { return true; }
            set { base.CommandsVisibleIfAvailable = value; }
        }

        #endregion

        #region OnCreateControl
        protected override void OnCreateControl()
        {
            DrawFlatToolbar = true;
            HelpVisible = true;

            PropertySort = PropertySort.Alphabetical;

            InitContextMenu();

            goBackOneItem.Enabled = false;
            goForwardOneItem.Enabled = false;

            base.OnCreateControl();

            // Add New Tabs here
            base.PropertyTabs.AddTabType(typeof(AllPropertiesTab));
            base.PropertyTabs.AddTabType(typeof(AllFieldsTab));
            base.PropertyTabs.AddTabType(typeof(InstanceEventsTab));
            base.PropertyTabs.AddTabType(typeof(MethodsTab));
            base.PropertyTabs.AddTabType(typeof(ProcessInfoTab));

            historyObjects.Clear();

            InitCustomToolbar();

            InitCustomContextMenus();

            Type type = typeof(PropertyGrid);
            if (ApplicationOptions.Instance.HasDynamicExtenders(type))
            {
                DynamicExtenderInfo extender = ApplicationOptions.Instance.GetDynamicExtender(type);
                if (extender != null) extender.CreateExtender(this);
                // TODO: log if extender == null
            }
        }
        #endregion

        #region Navigation

        protected override void OnSelectedObjectsChanged(EventArgs e)
        { // put in history
            if (SelectedObject != null)
            {
                if (!historyObjects.Contains(SelectedObject))
                {
                    if (activeObject < historyObjects.Count - 1)
                    {
                        historyObjects.RemoveRange(activeObject + 1, historyObjects.Count - activeObject - 1);
                    }
                    activeObject = historyObjects.Add(SelectedObject);
                    goBackOneItem.Enabled = true;
                    goForwardOneItem.Enabled = false;

                    if (historyObjects.Count > 10)
                    {
                        historyObjects.RemoveRange(0, historyObjects.Count - 10);
                    }
                }
                else
                {
                    activeObject = historyObjects.IndexOf(SelectedObject);
                }
            }
            base.OnSelectedObjectsChanged(e);
        }

        private void selectThisItem_Click(object sender, EventArgs e)
        {
            GridItem selectedGridItem = this.SelectedGridItem;

            if (selectedGridItem != null)
            {
                object value = selectedGridItem.Value;
                IRealValueHolder valueHolder = value as IRealValueHolder;
                if (valueHolder != null)
                {
                    value = valueHolder.RealValue;
                }
                InvokeSelectRequest(value);
            }
        }

        private void goBackOneItem_Click(object sender, EventArgs e)
        {
            if (activeObject > 0)
            {
                activeObject--;
                goForwardOneItem.Enabled = true;
            }
            else
            {
                goBackOneItem.Enabled = false;
            }
            InvokeSelectRequest();
        }

        private void goForwardOneItem_Click(object sender, EventArgs e)
        {
            if (activeObject < historyObjects.Count)
            {
                activeObject++;
                goBackOneItem.Enabled = true;
            }
            else
            {
                goForwardOneItem.Enabled = false;
            }
            InvokeSelectRequest();
        }

        private object GetActiveObject()
        {
            if (activeObject >= 0 && activeObject < historyObjects.Count)
            {
                return historyObjects[activeObject];
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Invoke

        private void InvokeSelectRequest()
        {
            if (SelectRequest != null)
            {
                SelectRequest(GetActiveObject());
            }
        }

        private void InvokeSelectRequest(object newOBject)
        {
            if (SelectRequest != null)
            {
                SelectRequest(newOBject);
            }
        }

        #endregion

        #region Tool Bar Buttons
        public ToolBar ExternalToolBar
        {
            get { return externalToolBar; }
            set { externalToolBar = value; }
        }

        private void InitCustomToolbar()
        {
            gridToolBar = externalToolBar;
            if (externalToolBar == null)
                return;
            externalToolBar.ButtonClick += new ToolBarButtonClickEventHandler(externalToolBar_ButtonClick);
            RegisterToolbarButtons(false);

            // Grab the toolbar inside the Property Grid
            //			FieldAccesor toolBarAccesor = new FieldAccesor(typeof(PropertyGrid), this, "toolBar");

            /*			if ( toolBarAccesor.IsValid )
                        {
                            // we might be running on a box with .Net 2.0 and NO .Net 1.1
                            toolBarAccesor.Save();
                            gridToolBar = toolBarAccesor.Value as ToolBar;

                            RegisterToolbarButtons(true);
                        }
                        else
                        {
                            Trace.WriteLine("We might be using .Net 2.0 and we have to use a ToolStrip!");

                            gridToolBar = externalToolBar;
                            externalToolBar.Visible = true;
                            externalToolBar.ButtonClick += new ToolBarButtonClickEventHandler(externalToolBar_ButtonClick);

                            RegisterToolbarButtons(false);


                            FieldAccesor toolStripAccessor = new FieldAccesor(typeof(PropertyGrid), this, "toolStrip");
                            if ( toolStripAccessor.IsValid )
                            {
                                toolStripAccessor.Save();
                                object toolStrip = toolStripAccessor.Value;
					
                                PropertyAccesor toolStripItemsAccessor = new PropertyAccesor(toolStrip, "Items");
                                toolStripItemsAccessor.Save();

                                object items = toolStripItemsAccessor.Value;
                                ObjectHandle instance = Activator.CreateInstance("System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Windows.Forms.ToolStripButton");
                                object newButton = instance.Unwrap();

                                PropertyAccesor countAcc = new PropertyAccesor(items, "Count").Save();
                                int count = (int)countAcc.Value;

                                MethodAccesor addItem = new MethodAccesor(items.GetType(), "Insert");
                                addItem.Invoke(items, new object[]{ count, newButton } );
                            }
                        }*/
        }

        private void RegisterToolbarButtons(bool modifyExisting)
        {
            if (modifyExisting)
            {
                ToolbarUtils.AddDelimiter(gridToolBar);
                ToolbarUtils.DelButton(gridToolBar, "Property Pages");
            }

            btnRefresh = ToolbarUtils.AddButton(gridToolBar, "Refresh", "Refresh.bmp", new EventHandler(btnRefresh_Click));
            btnHighlightWindow = ToolbarUtils.AddButton(gridToolBar, "Highlight Window", "Highlight.bmp", new EventHandler(highlightWindow_Click));
            btnExpandAll = ToolbarUtils.AddButton(gridToolBar, "Collapse and Group by Category", "CollapseAll.bmp", new EventHandler(btnCollapseAll_Click));
            btnCollapseAll = ToolbarUtils.AddButton(gridToolBar, "Expand All", "ExpandAll.bmp", new EventHandler(btnExpandAll_Click));
            btnShowSourceCode = ToolbarUtils.AddButton(gridToolBar, "Show SourceCode In Reflector", "Reflector.bmp", new EventHandler(showSourceCodeForItem_Click));
            btnAddExtender = ToolbarUtils.AddButton(gridToolBar, "Add Dynamic Extender", "AddDynamicExtender.bmp", new EventHandler(addDynamicExtender_click));

            btnAddExtender.Enabled = false;

            if (AddToolbarButtons != null)
            {
                AddToolbarButtons(this, EventArgs.Empty);
            }

            RefreshToolbarButtonsState();
        }

        private void highlightWindow_Click(object sender, EventArgs e)
        {
            if (this.SelectedObject is Control)
            {
                IntPtr handle = (this.SelectedObject as Control).Handle;
                if (handle != IntPtr.Zero)
                {
                    highlightWindowProperties.SetWindowHandle(handle, Point.Empty);
                }
            }
        }

        public ToolBarButton AddToolbarButton(int index, string text, string imageName, EventHandler eventHandler)
        {
            return ToolbarUtils.AddButton(index, gridToolBar, text, imageName, eventHandler);
        }

        private void RefreshToolbarButtonsState()
        {
            bool enabled = this.SelectedObject != null;
            btnRefresh.Enabled = enabled;
            btnExpandAll.Enabled = enabled;
            btnCollapseAll.Enabled = enabled;
            btnShowSourceCode.Enabled = enabled;
            btnAddExtender.Enabled = false;

            object selected = this.SelectedObject;
            if (selected != null)
            {
                if (ApplicationOptions.Instance.HasDynamicExtenders(selected.GetType()))
                    btnAddExtender.Enabled = true;
            }

        }
        private void externalToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            ToolBarButton btn = e.Button;
            if (btn != null)
            {
                EventHandler handler = btn.Tag as EventHandler;
                if (handler != null)
                {
                    handler(externalToolBar, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Menu Items Handling
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.Refresh();
        }
        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            if (this.SelectedObject == null)
                return;
            this.Visible = false;
            try
            {
                this.PropertySort = System.Windows.Forms.PropertySort.CategorizedAlphabetical;
                this.CollapseAllGridItems();
            }
            finally
            {
                this.Visible = true;
            }
        }
        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            try
            {
                ExpandAll();
            }
            finally
            {
                this.Visible = true;
            }
        }
        private void showSourceCodeForItem_Click(object sender, EventArgs e)
        {
            GridItem selectedGridItem = this.SelectedGridItem;
            if (selectedGridItem != null)
            {
                ReflectorRouter.Instance.ShowSourceCode(selectedGridItem.PropertyDescriptor);
            }
        }
        #endregion

        #region Expand
        private void ExpandAll()
        {
            if (this.SelectedObject == null)
                return;

            if (gridViewAccessor == null)
            {
                gridViewAccessor = new FieldAccesor(this, "gridView");
                peMainAccessor = new FieldAccesor(this, "peMain");
            }

            object gridView = gridViewAccessor.Get();
            object peMain = peMainAccessor.Get();
            if (recursivelyExpandAccessor == null)
            {
                recursivelyExpandAccessor = new MethodAccesor(gridView.GetType(), "RecursivelyExpand");
            }

            recursivelyExpandAccessor.Invoke(gridView, new object[] { peMain, false, true, 2 });
        }

        #endregion

        private void XPropertyGrid_SelectedObjectsChanged(object sender, EventArgs e)
        {
            RefreshToolbarButtonsState();
        }

        #region Context Menus
        private void InitCustomContextMenus()
        {
            foreach (PropertyTab tab in this.PropertyTabs)
            {
                IPropertyGridTab handler = tab as IPropertyGridTab;
                if (handler != null)
                {
                    handler.PropertyGrid = this;
                }
            }

            foreach (PropertyTab tab in this.PropertyTabs)
            {
                ICustomMenuHandler handler = tab as ICustomMenuHandler;
                if (handler != null)
                {
                    handler.RegisterMenuItems(this.contextMenu);
                }
            }
            RefreshValidContextMenus();
        }

        private void RefreshValidContextMenus()
        {
            foreach (MenuItem menuItem in this.contextMenu.MenuItems)
            {
                CustomMenuItem cMenuItem = menuItem as CustomMenuItem;
                if (cMenuItem != null)
                {
                    if (cMenuItem.OwnerTab != this.SelectedTab)
                    {
                        cMenuItem.Visible = false;
                    }
                    else
                    {
                        cMenuItem.Visible = true;
                    }
                }
            }
        }

        private void XPropertyGrid_PropertyTabChanged(object s, PropertyTabChangedEventArgs e)
        {
            RefreshValidContextMenus();
        }
        #endregion

        #region Dynamic Extenders
        private void addDynamicExtender_click(object sender, EventArgs e)
        {
            if (SelectedObject == null) return;

            Type selectedObjectType = SelectedObject.GetType();
            if (ApplicationOptions.Instance.HasDynamicExtenders(selectedObjectType))
            {
                DynamicExtenderInfo extender = ApplicationOptions.Instance.GetDynamicExtender(selectedObjectType);
                if (extender != null) extender.CreateExtender(SelectedObject);
                // TODO: log if extender == null
            }
        }
        #endregion
    }
}