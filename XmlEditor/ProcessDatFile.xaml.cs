using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XmlEditor {
  /// <summary>
  /// Interaction logic for ProcessDatFile.xaml
  /// </summary>
  public partial class ProcessDatFile : Window {
    public ProcessDatFile() {
      InitializeComponent();
    }

    private void btnSelectFile_Click(object sender, RoutedEventArgs e) {
      var openDialog = new Microsoft.Win32.OpenFileDialog();

      openDialog.DefaultExt = ".dat";
      openDialog.Filter = "DAT files|*.dat|All documents|*.*";

      if (openDialog.ShowDialog().Value) {

        lbFileName.Text = openDialog.FileName;
      }
    }

    private void btnProcessFile_Click(object sender, RoutedEventArgs e) {
      if (lbFileName.Text == "")
        MessageBox.Show("Select the file you want to process.");

      else if (!System.IO.File.Exists(lbFileName.Text))
        MessageBox.Show("The specificed file does not exist anymore.");

      else {
        int minimumCharacterCount = 0;

        try {
          minimumCharacterCount = int.Parse(txMinimumCharacterCount.Text);

          if (minimumCharacterCount == 0)
            MessageBox.Show("You must enter a positive value for minimum character count.");
        } catch {
          MessageBox.Show("Be sure to only enter digits in the minimum character count.");
        }

        if (minimumCharacterCount > 0 && MessageBox.Show("This process will overwrite the specified file. Continue?", "Confirm Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
          var fileContents = System.IO.File.ReadAllText(lbFileName.Text);
          var reader = new System.IO.StringReader(fileContents);
          string line;
          var newFileContents = new System.Text.StringBuilder();

          line = reader.ReadLine();

          while (line != null) {
            if (line.Length >= minimumCharacterCount)
              newFileContents.AppendLine(line);

            line = reader.ReadLine();
          }

          reader.Close();

          System.IO.File.WriteAllText(lbFileName.Text, newFileContents.ToString());

          MessageBox.Show("Process Completed");
        }
      }
    }
  }
}