﻿using G2D_Monitor.Manager;

namespace G2D_Monitor.Plugins
{
    public abstract class Plugin
    {
        private static TabPage? CurrentTab = null;

        private static readonly List<Plugin> Plugins = new();

        public static void Register<T>() where T : Plugin, new() => Register(new T());

        public static void Register(Plugin plugin)
        {
            if (plugin.UIRequired)
            {
                var tab = new TabPage(plugin.Name);
                MainForm.Instance.MainTabControl.SuspendLayout();
                tab.SuspendLayout();
                MainForm.Instance.MainTabControl.TabPages.Add(tab);
                plugin.Initialize(plugin.bindedTab = tab);
                MainForm.Instance.MainTabControl.ResumeLayout(false);
                tab.ResumeLayout(false);
            }
            plugin.Activate();
            Plugins.Add(plugin);
        }

        public static void Close()
        {
            foreach (var plugin in Plugins) plugin.OnClosing();
        }

        public static void Update(Context? context)
        {
            try 
            {
                MainForm.Instance.Invoke(() =>
                {
                    CurrentTab = MainForm.Instance.MainTabControl.SelectedTab;
                    if (context == null)
                    {
                        foreach (var plugin in Plugins) plugin.OnGameExit();
                    }
                    else
                    {
                        foreach (var plugin in Plugins) plugin.DoUpdate(context);
                        FramePlugin.FrameUpdate(context);
                    }
                });
                Thread.Sleep(5);
            } 
            catch { }
        }

        protected static ListView NewListView()
        {
            var listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.Columns.Add("Key", 200);
            listView.Columns.Add("Value", 540);
            return listView;
        }

        private TabPage? bindedTab = null;

        protected bool IsTabActive => bindedTab == CurrentTab;

        protected virtual bool UIRequired => true;

        protected virtual string Name => string.Empty;

        protected virtual void Initialize(TabPage tab) { }

        protected virtual void Activate() { }

        protected virtual void DoUpdate(Context context) { }

        protected virtual void OnGameExit() { }

        protected virtual void OnClosing() { }
    }
}
