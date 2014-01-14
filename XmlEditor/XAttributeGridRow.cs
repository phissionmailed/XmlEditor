using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows;

namespace XmlEditor {
  public class XAttributeGridRow {
    public event EventHandler XAttributeValueChanged;

    private XAttribute mXAttribute;
    private Label mLabel;
    private TextBox mTextBox;
    private RowDefinition mRowDefinition;

    public XAttributeGridRow()
      : base() {
        mLabel = new Label();
        Grid.SetColumn(mLabel, 0);

        mTextBox = new TextBox();
        Grid.SetColumn(mTextBox, 1);
        mTextBox.TextChanged += new TextChangedEventHandler(mTextBox_TextChanged);

        mRowDefinition = new RowDefinition();
        mRowDefinition.Height = GridLength.Auto;
    }

    ~XAttributeGridRow() {
      mTextBox.TextChanged -= new TextChangedEventHandler(mTextBox_TextChanged);
    }

    private void mTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      mXAttribute.Value = mTextBox.Text;

      if (this.XAttributeValueChanged != null)
        this.XAttributeValueChanged(this, EventArgs.Empty);
    }

    public void AddToGrid(Grid target) {
      Grid.SetRow(mLabel, target.RowDefinitions.Count);
      Grid.SetRow(mTextBox, target.RowDefinitions.Count);

      target.RowDefinitions.Add(mRowDefinition);
      target.Children.Add(mLabel);
      target.Children.Add(mTextBox);
    }

    private void RemoveFromGrid(Grid target) {
      target.RowDefinitions.Remove(mRowDefinition);
      target.Children.Remove(mLabel);
      target.Children.Remove(mTextBox);
    }

    public XAttribute XAttribute {
      get {
        return mXAttribute;
      }
      set {
        mXAttribute = value;

        mLabel.Content = mXAttribute.Name;
        mTextBox.Text = mXAttribute.Value;
      }
    }
  }
}