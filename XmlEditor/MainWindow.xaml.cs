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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace XmlEditor {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    XDocument mDocument;
    TreeViewItemXNode mSelectedItem;
    XNode mSelectedXNode;
    string mCurrentFile;
    List<XAttributeGridRow> mAttributeRows;
    ContextMenu treeViewMenu;

    public MainWindow() {
      InitializeComponent();

      mAttributeRows = new List<XAttributeGridRow>();

      this.Title += " " + typeof(MainWindow).Assembly.GetName().Version.ToString();

      MenuItem menu;

      treeViewMenu = new ContextMenu();

      menu = new MenuItem();
      menu.Header = "Insert Node Above";
      menu.Click += new RoutedEventHandler(this.InsertNodeAbove);
      treeViewMenu.Items.Add(menu);
 
      menu = new MenuItem();
      menu.Header = "Insert Node Inside";
      menu.Click += new RoutedEventHandler(this.InsertNodeInside);
      treeViewMenu.Items.Add(menu);

      menu = new MenuItem();
      menu.Header = "Insert Node Below";
      menu.Click += new RoutedEventHandler(this.InsertNodeBelow);
      treeViewMenu.Items.Add(menu);
      
      treeViewMenu.Items.Add(new Separator());

      menu = new MenuItem();
      menu.Header = "Delete Node";
      menu.Click += new RoutedEventHandler(this.DeleteNode);
      menu.Icon = new Image();
      ((Image)menu.Icon).Source = new BitmapImage(new Uri("/Images/delete.ico", UriKind.RelativeOrAbsolute));
      ((Image)menu.Icon).Width = 16;
      ((Image)menu.Icon).Height = 16;
      treeViewMenu.Items.Add(menu);

      tvXmlDocument.ContextMenu = treeViewMenu;
    }

    private void btnNew_Click(object sender, RoutedEventArgs e) {
      //var doctype = new XDocumentType(null, null, null, null);
      var rootNode = new XElement("Root");

      mCurrentFile = null;
      mDocument = new XDocument();

      //mDocument.Add(doctype);
      mDocument.Add(rootNode);

      this.ShowXDocument();
    }

    private void btnOpen_Click(object sender, RoutedEventArgs e) {
      var openDialog = new Microsoft.Win32.OpenFileDialog();

      openDialog.DefaultExt = ".xml";
      openDialog.Filter = "XML documents|*.xml|Text documents|*.txt|All documents|*.*";

      try {
        if (openDialog.ShowDialog().Value) {
          mDocument = XDocument.Load(openDialog.FileName);

          mCurrentFile = openDialog.FileName;

          this.ShowXDocument();
        }
      } catch (Exception ex) {
        App.HandleException(ex);
      }
    }

    private void btnSave_Click(object sender, RoutedEventArgs e) {
      if (mDocument != null) {
        bool askForFile = false;

        try {
          if (mCurrentFile != null && MessageBox.Show("Overwrite file in " + mCurrentFile + "?", "Overwrite Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
            mDocument.Save(mCurrentFile);
          } else
            askForFile = true;

          if (askForFile) {
            var saveDialog = new Microsoft.Win32.SaveFileDialog();

            saveDialog.DefaultExt = ".xml";
            saveDialog.Filter = "XML documents|*.xml|Text documents|*.txt|All documents|*.*";
            saveDialog.FileName = mCurrentFile;

            if (saveDialog.ShowDialog().Value) {
              mDocument.Save(saveDialog.FileName);
              mCurrentFile = saveDialog.FileName;
            }
          }
        } catch (Exception ex) {
          App.HandleException(ex);
        }
      }
    }

    private void ShowXDocument() {
      var elementsEnumerator = mDocument.Nodes().GetEnumerator();

      for (int i = 0; i < mAttributeRows.Count; i++) {
        mAttributeRows[i].XAttributeValueChanged -= new EventHandler(this.XAttribute_ValueChanged);
      }
      mAttributeRows.Clear();
      grdAttributes.Children.Clear();
      grdAttributes.RowDefinitions.Clear();

      txNodeName.TextChanged -= new TextChangedEventHandler(txNodeName_TextChanged);
      txNodeName.GotFocus -= new RoutedEventHandler(txNodeName_GotFocus);
      txNodeValue.TextChanged -= new TextChangedEventHandler(txNodeValue_TextChanged);

      for (int i = 0; i < tvXmlDocument.Items.Count; i++) {
        if (((TreeViewItem)tvXmlDocument.Items[i]).HasItems)
          this.DetachNodeEvents((TreeViewItem)tvXmlDocument.Items[i]);
      }

      tvXmlDocument.Items.Clear();
      
      while (elementsEnumerator.MoveNext())
        ShowXNode(elementsEnumerator.Current, null);
    }

    private void DetachNodeEvents(TreeViewItem node) {
      node.Collapsed -= new RoutedEventHandler(ItemCollapsed);
      node.Expanded -= new RoutedEventHandler(ItemExpanded);

      for (int i = 0; i < node.Items.Count; i++) {
        if (((TreeViewItem)node.Items[i]).HasItems)
          this.DetachNodeEvents((TreeViewItem)node.Items[i]);
      }
    }

    private void ShowXNode(XNode xnode, TreeViewItemXNode parentNode, int position = -1) {
      var newItem = new TreeViewItemXNode();

      newItem.XNode = xnode;

      if (typeof(XComment).IsAssignableFrom(xnode.GetType())) {
        var comment = (XComment)xnode;

        newItem.Header = comment.Value;

      } else if (typeof(XElement).IsAssignableFrom(xnode.GetType())) {
        var element = (XElement)xnode;

        //TreeViewItem to close opening tag.
        newItem.EndingItem = new TreeViewItemXNode();
        newItem.EndingItem.XNode = element;
        newItem.EndingItem.BeginItem = newItem;
        newItem.EndingItem.Header = "</" + element.Name + ">";

        if (element.HasElements) {
          var elementsEnumerator = element.Nodes().GetEnumerator();

          while (elementsEnumerator.MoveNext()) {
            this.ShowXNode(elementsEnumerator.Current, newItem);
          }
        } else if (element.Value.Length > 0) {
          //TreeViewItem to show element value.
          //When an XElement has a value, it has an XText node, which is in the first position.
          newItem.ValueItem = new TreeViewItemXNode();
          newItem.ValueItem.BeginItem = newItem;
          newItem.ValueItem.XNode = element.FirstNode;
          newItem.ValueItem.Header = element.Value;
          newItem.Items.Add(newItem.ValueItem);
        }

        if (newItem.ValueItem == null) {
          //Create an XNode to hold the element's value.
          var text = new XText("");
          element.AddFirst(text);

          //TreeViewItem to show element value.
          newItem.ValueItem = new TreeViewItemXNode();
          newItem.ValueItem.BeginItem = newItem;
          newItem.ValueItem.XNode = text;
          newItem.ValueItem.Header = "";
        }

        this.RefreshXElementItemHeader(newItem);

      } else if (typeof(XText).IsAssignableFrom(xnode.GetType())) {
        var text = (XText)xnode;

        newItem.BeginItem = parentNode;
        newItem.Header = text.Value;
        parentNode.ValueItem = newItem;

      } else {
        newItem.Header = "UNKNOWN XNODE!!!";
      }

      if (parentNode == null)
        tvXmlDocument.Items.Add(newItem);
      else {
        if (position == -1)
          parentNode.Items.Add(newItem);
        else
          parentNode.Items.Insert(position, newItem);
      }

      if (newItem.HasItems) {
        newItem.Collapsed += new RoutedEventHandler(ItemCollapsed);
        newItem.Expanded += new RoutedEventHandler(ItemExpanded);
      }
    }

    private void RefreshXElementItemHeader(TreeViewItemXNode item) {
      var element = (XElement)item.XNode;
      var newItemHeader = "<" + element.Name;

      if (element.HasAttributes) {
        using (var attEnumerator = element.Attributes().GetEnumerator()) {
          while (attEnumerator.MoveNext()) {
            newItemHeader += " " + attEnumerator.Current.Name + "=" + attEnumerator.Current.Value;
          }
        }
      }

      if (!item.HasItems)
        newItemHeader += "/";

      item.Header = newItemHeader + ">";
    }

    private void ItemCollapsed(object sender, RoutedEventArgs e) {
      if (sender == e.OriginalSource) {
        var item = (TreeViewItemXNode)sender;

        if (item.Parent == tvXmlDocument)
          tvXmlDocument.Items.Remove(item.EndingItem);
        else
          ((TreeViewItemXNode)item.Parent).Items.Remove(item.EndingItem);
      }
    }

    private void ItemExpanded(object sender, RoutedEventArgs e) {
      if (sender == e.OriginalSource) {
        var item = (TreeViewItemXNode)sender;

        if (item.Parent == tvXmlDocument) {
          tvXmlDocument.Items.Insert(tvXmlDocument.Items.IndexOf(item) + 1, item.EndingItem);
        } else {
          var parentNode = (TreeViewItemXNode)item.Parent;

          parentNode.Items.Insert(parentNode.Items.IndexOf(item) + 1, item.EndingItem);
        }
      }
    }

    private void tvXmlDocument_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
      var selectedItem = (TreeViewItemXNode)tvXmlDocument.SelectedItem;

      txNodeName.TextChanged -= new TextChangedEventHandler(txNodeName_TextChanged);
      txNodeName.GotFocus -= new RoutedEventHandler(txNodeName_GotFocus);
      txNodeValue.TextChanged -= new TextChangedEventHandler(txNodeValue_TextChanged);

      if (selectedItem == null) {
        grdNodeDetails.Visibility = Visibility.Hidden;
        mSelectedItem = null;
        mSelectedXNode = null;

      } else {
        grdNodeDetails.Visibility = Visibility.Visible;

        /*if (selectedItem.EndingItem != null || (selectedItem.BeginItem == null && selectedItem.EndingItem == null)) {
          //This is item is an XElement because it either:
          //  has ending item, which only XElement items can have; OR
          //  doesn't have either a begin item or an end item, which only XElement items cannot have both.
          //Do nothing, just let execution fall through.
        } else*/ if (selectedItem.BeginItem != null) {
          //The real item is the one at the beginning.
          selectedItem = selectedItem.BeginItem;
        }

        if (selectedItem != mSelectedItem) {
          //Selected item is different from the previously selected one.
          mSelectedItem = selectedItem;
          mSelectedXNode = mSelectedItem.XNode;

          mAttributeRows.Clear();
          grdAttributes.Children.Clear();
          grdAttributes.RowDefinitions.Clear();
          
          if (typeof(XComment).IsAssignableFrom(mSelectedXNode.GetType())) {
            //User selected a comment. Only its value can be changed.
            var comment = (XComment)mSelectedXNode;

            lbNodeType.Text = "This is a comment node. Only its value can be changed.";
            grpAttributes.Visibility = Visibility.Hidden;
            txNodeValue.Text = comment.Value;
            txNodeName.IsEnabled = false;
            txNodeName.Text = "Comment Node";

            txNodeName.TextChanged -= new TextChangedEventHandler(txNodeName_TextChanged);
            txNodeName.GotFocus -= new RoutedEventHandler(txNodeName_GotFocus);
            txNodeValue.TextChanged += new TextChangedEventHandler(txNodeValue_TextChanged);

          } else if (typeof(XElement).IsAssignableFrom(mSelectedXNode.GetType())) {
            var element = (XElement)mSelectedXNode;
            XAttributeGridRow newRow;

            lbNodeType.Text = "This is a normal node. You can change its name, value, and attributes.";
            txNodeName.IsEnabled = true;
            txNodeName.Text = element.Name.LocalName;
            txNodeValue.Text = (string)mSelectedItem.ValueItem.Header;

            grpAttributes.Visibility = Visibility.Visible;

            if (element.HasAttributes) {
              using (var attributesEnumerator = element.Attributes().GetEnumerator()) {
                while (attributesEnumerator.MoveNext()) {
                  newRow = new XAttributeGridRow();

                  newRow.XAttribute = attributesEnumerator.Current;
                  newRow.AddToGrid(grdAttributes);
                  newRow.XAttributeValueChanged += new EventHandler(this.XAttribute_ValueChanged);

                  mAttributeRows.Add(newRow);
                }
              }
            }

            txNodeName.TextChanged += new TextChangedEventHandler(txNodeName_TextChanged);
            txNodeName.GotFocus += new RoutedEventHandler(txNodeName_GotFocus);
            txNodeValue.TextChanged += new TextChangedEventHandler(txNodeValue_TextChanged);
          }

        } else if (typeof(XComment).IsAssignableFrom(mSelectedXNode.GetType())) {
          txNodeName.TextChanged -= new TextChangedEventHandler(txNodeName_TextChanged);
          txNodeName.GotFocus -= new RoutedEventHandler(txNodeName_GotFocus);
          txNodeValue.TextChanged += new TextChangedEventHandler(txNodeValue_TextChanged);

        } else if (typeof(XElement).IsAssignableFrom(mSelectedXNode.GetType())) {
          txNodeName.TextChanged += new TextChangedEventHandler(txNodeName_TextChanged);
          txNodeName.GotFocus += new RoutedEventHandler(txNodeName_GotFocus);
          txNodeValue.TextChanged += new TextChangedEventHandler(txNodeValue_TextChanged);

        }
      }
    }  //tvXmlDocument_SelectedItemChanged

    private void XAttribute_ValueChanged(object sender, EventArgs e) {
      this.RefreshXElementItemHeader(mSelectedItem);
    }

    private void txNodeName_TextChanged(object sender, TextChangedEventArgs e) {
      string oldName = "";

      try {
        oldName = ((XElement)mSelectedXNode).Name.LocalName;

        ((XElement)mSelectedXNode).Name = txNodeName.Text;

        mSelectedItem.Header = ((string)mSelectedItem.Header).Replace("<" + oldName, "<" + txNodeName.Text);
        mSelectedItem.EndingItem.Header = "</" + txNodeName.Text + ">";
      } catch {
        txNodeName.TextChanged -= new TextChangedEventHandler(txNodeName_TextChanged);
        txNodeName.Text = oldName;
        txNodeName.SelectionStart = oldName.Length;
        txNodeName.TextChanged += new TextChangedEventHandler(txNodeName_TextChanged);

        MessageBox.Show("The new name is not valid for an XML Node name.  Please check your input.\nIf you wish to enter a new name, select all the text and begin typing.");
      }

    }

    private void txNodeValue_TextChanged(object sender, TextChangedEventArgs e) {
      if (typeof(XComment).IsAssignableFrom(mSelectedXNode.GetType())) {
        ((XComment)mSelectedXNode).Value = txNodeValue.Text;
        mSelectedItem.Header = txNodeValue.Text;

      } else if (typeof(XElement).IsAssignableFrom(mSelectedXNode.GetType())) {
        var oldValue = (string)mSelectedItem.ValueItem.Header;

        if (txNodeValue.Text != "") {
          //There is a new value.

          if (oldValue == "") {
            //There is no value in the screen for the user to see.
            if (!mSelectedItem.HasItems) {
              //Selected node had no items, update its header text to remove the / character.
              mSelectedItem.Header = ((string)mSelectedItem.Header).Substring(0, ((string)mSelectedItem.Header).Length - 2) + ">";

              //Attach to collapsed/expanded events.
              mSelectedItem.Collapsed += new RoutedEventHandler(ItemCollapsed);
              mSelectedItem.Expanded += new RoutedEventHandler(ItemExpanded);
            }

            //Insert new value node at the beginning of the sub-items.
            mSelectedItem.Items.Insert(0, mSelectedItem.ValueItem);
          }
        } else {
          //New value is empty.
          mSelectedItem.Items.Remove(mSelectedItem.ValueItem);

          if (mSelectedItem.Items.Count == 0) {
            //Node was left without sub-items.
            //Update its header to show / character.
            mSelectedItem.Header = ((string)mSelectedItem.Header).Substring(0, ((string)mSelectedItem.Header).Length - 1) + "/>";

            //Dettach from collapsed/expanded events.
            mSelectedItem.IsExpanded = false;
            mSelectedItem.Collapsed -= new RoutedEventHandler(ItemCollapsed);
            mSelectedItem.Expanded -= new RoutedEventHandler(ItemExpanded);

            //Remove ending item from TreeView.
            ((TreeViewItemXNode)mSelectedItem.Parent).Items.Remove(mSelectedItem.EndingItem);
          }
        }

        mSelectedItem.ValueItem.Header = txNodeValue.Text;
        ((XText)mSelectedItem.ValueItem.XNode).Value = txNodeValue.Text;
      }
    }

    private void txNodeName_GotFocus(object sender, RoutedEventArgs e) {
      txNodeName.SelectAll();
    }

    private void tvXmlDocument_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
      var hitTestResult = tvXmlDocument.InputHitTest(e.GetPosition(tvXmlDocument));

      e.Handled = true;

      if (typeof(TextBlock).IsAssignableFrom(hitTestResult.GetType())) {
        var item = (TreeViewItemXNode)e.Source;

        item.IsSelected = true;

        treeViewMenu.PlacementTarget = item;
        treeViewMenu.IsOpen = true;
      } 

    }

    private void InsertNodeAbove(object sender, RoutedEventArgs e) {
      if (sender == e.OriginalSource) {
        var selectedNode = (TreeViewItemXNode)tvXmlDocument.SelectedItem;

        if (selectedNode != null && selectedNode.Parent != tvXmlDocument) {
          var newXNode = new XElement("newNode");

          selectedNode.XNode.AddBeforeSelf(newXNode);

          this.ShowXNode(newXNode, (TreeViewItemXNode)selectedNode.Parent, ((TreeViewItemXNode)selectedNode.Parent).Items.IndexOf(selectedNode));
        }
      }
    }

    private void InsertNodeBelow(object sender, RoutedEventArgs e) {
      if (sender == e.OriginalSource) {
        var selectedNode = (TreeViewItemXNode)tvXmlDocument.SelectedItem;

        if (selectedNode != null && selectedNode.Parent != tvXmlDocument) {
          var newXNode = new XElement("newNode");

          selectedNode.XNode.AddAfterSelf(newXNode);

          this.ShowXNode(newXNode, (TreeViewItemXNode)selectedNode.Parent, ((TreeViewItemXNode)selectedNode.Parent).Items.IndexOf(selectedNode) + 1);
        }
      }
    }

    private void InsertNodeInside(object sender, RoutedEventArgs e) {
      if (sender == e.OriginalSource) {
        var selectedNode = (TreeViewItemXNode)tvXmlDocument.SelectedItem;

        if (selectedNode != null && selectedNode.Parent != tvXmlDocument && typeof(XElement).IsAssignableFrom(selectedNode.XNode.GetType())) {
          var newXNode = new XElement("newNode");
          var selectedXElement = (XElement)selectedNode.XNode;
          
          selectedXElement.AddFirst(newXNode);

          this.ShowXNode(newXNode, selectedNode);

          this.RefreshXElementItemHeader(selectedNode);

          if (selectedNode.Items.Count == 1) {
            //Parent node just gained a child. Attach to its events.
            selectedNode.Collapsed += new RoutedEventHandler(this.ItemCollapsed);
            selectedNode.Expanded += new RoutedEventHandler(this.ItemExpanded);
          }

        }
      }
    }

    private void DeleteNode(object sender, RoutedEventArgs e) {
      if (sender == e.OriginalSource) {
        var nodeToDelete = (TreeViewItemXNode)tvXmlDocument.SelectedItem;

        if (nodeToDelete != null && nodeToDelete.Parent != tvXmlDocument) {
          TreeViewItemXNode parent = (TreeViewItemXNode)nodeToDelete.Parent;

          this.DetachNodeEvents(nodeToDelete);

          parent.Items.Remove(nodeToDelete);

          nodeToDelete.XNode.Remove();

          this.RefreshXElementItemHeader(parent);

          if (parent.Items.Count == 0) {
            //Parent lost its last child, dettach from its events.
            parent.Collapsed -= new RoutedEventHandler(this.ItemCollapsed);
            parent.Expanded -= new RoutedEventHandler(this.ItemExpanded);

            //Remove the ending item from the parent's parent hierarchy.
            ((TreeViewItemXNode)parent.Parent).Items.Remove(parent.EndingItem);
          }
        }
      }
    }

    private void btnProcessDatFile_Click(object sender, RoutedEventArgs e) {
      var process = new ProcessDatFile();

      process.ShowDialog();
    }
  }  //MainWindow
}  //Namespace