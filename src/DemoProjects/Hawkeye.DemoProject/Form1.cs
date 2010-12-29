using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Hawkeye.DemoProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;

#if X64
            theLabel.TheText = "I am an x64 Test application";
#elif X86
            theLabel.TheText = "I am an x86 Test application";
#else
            theLabel.TheText = "I am an 'Any CPU' Test application";
#endif
            exitButton.Click += delegate { Close(); };

            base.Text = string.Format("Test: {0} bits", 8 * Marshal.SizeOf(typeof(IntPtr)));

        }
    }
}