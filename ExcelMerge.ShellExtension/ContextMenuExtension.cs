using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace ExcelMerge.ShellExtension
{
    [Guid("C7471DED-BC6E-4A86-8B71-2B9FE239FE07")]
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    public class ContextMenuExtension : SharpContextMenu
    {
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var icon = new Bitmap(16, 16);
            var g = Graphics.FromImage(icon);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(Properties.Resources.app.ToBitmap(), 0, 0, icon.Width, icon.Height);

            var item = new ToolStripMenuItem
            {
                Text = "ExcelMerge",
                Image = icon,
            };

            item.Click += LaunchGUITool;

            menu.Items.Add(item);

            return menu;
        }

        protected override bool CanShowMenu()
        {
            return true;
        }

        private void LaunchGUITool(object sender, EventArgs e)
        {
            var srcPath = SelectedItemPaths.ElementAtOrDefault(0);
            var dstPath = SelectedItemPaths.ElementAtOrDefault(1);

            var arg = !string.IsNullOrEmpty(srcPath) ? $"--src=\"{srcPath}\" " : string.Empty;
            arg += !string.IsNullOrEmpty(dstPath) ? $"--dst=\"{dstPath}\" " : string.Empty;

            string exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ExcelMerge.GUI.exe");

            System.Diagnostics.Process.Start(exePath, arg);
        }
    }
}
