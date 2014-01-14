using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Controls;

namespace XmlEditor {
  public class TreeViewItemXNode : TreeViewItem {
    private XNode mXNode;
    private TreeViewItemXNode mEndingItem, mBeginItem, mValueItem;

    public TreeViewItemXNode()
      : base() {

    }

    public XNode XNode {
      get {
        return mXNode;
      }
      set {
        mXNode = value;
      }
    }

    public TreeViewItemXNode EndingItem {
      get {
        return mEndingItem;
      }
      set {
        mEndingItem = value;
      }
    }

    public TreeViewItemXNode BeginItem {
      get {
        return mBeginItem;
      }
      set {
        mBeginItem = value;
      }
    }

    public TreeViewItemXNode ValueItem {
      get {
        return mValueItem;
      }
      set {
        mValueItem = value;
      }
    }
  }
}