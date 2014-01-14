using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace ClipImage {
  static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      var target = new Bitmap(32, 32);

      var source = Image.FromFile(@"C:\Users\yo\Documents\XmlEditor\XmlEditor\ClipImage\bin\Debug\Save_FloppyDisk.png");
      

      var graphics = Graphics.FromImage(target);

      graphics.DrawImageUnscaled(source, 0, 10, 32, 32);

      graphics.Dispose();

      target.Save(@"C:\Users\yo\Documents\XmlEditor\XmlEditor\ClipImage\bin\Debug\Save_FloppyDisk_32x32.png");

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
