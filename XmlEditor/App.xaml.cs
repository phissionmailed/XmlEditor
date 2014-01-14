using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace XmlEditor {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    public static void HandleException(Exception ex) {
      MessageBox.Show(ex.Message);
    }
  }
}
