//ドラッグ中アイテムを半透明にする機能⇒実装を断念（機能するが、移動先の表示と相性が悪い…）。
//#define __ENABLE_DRAG_GHOST__ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace nabejiru.UserControl
{

    /// <summary>
    /// リストアイテムのドラッグ操作による移動を実装したListViewコントロール
    /// </summary>
    /// <author>nabejiru</author>
    public class DraggableListView : ListView
    {
        private ImageList imageList = new ImageList();
        /// <summary>アイテムの移動先を表現する線の色</summary>
        private readonly Color DRAG_TARGET_LINE_COLOR = Color.Red;

        ListViewItem m_prevItem = null;
        bool m_prevFlag = false;

        public DraggableListView()
            : base()
        {
            this.AllowItemDrag = true;
        }


        [Category("独自プロパティ")
        , DefaultValue(true)
        , Description("リストアイテムのドラッグ操作が可能かどうかを取得、設定します。")]
        public bool AllowItemDrag
        {
            get;
            set;
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            // ドラッグをキャンセル
            if (AllowItemDrag == false) return;

            base.OnItemDrag(e);

            if (e.Button == MouseButtons.Left)
            {
#if __ENABLE_DRAG_GHOST__
                Point mousePosition = this.PointToClient(Control.MousePosition);

                // ドラッグするイメージのサイズを求める
                Rectangle imageRectangle = new Rectangle();
                foreach (ListViewItem item in this.SelectedItems)
                {
                    if (imageRectangle.IsEmpty)
                    {
                        imageRectangle = item.Bounds;
                    }
                    else
                    {
                        imageRectangle = Rectangle.Union(imageRectangle, item.Bounds);
                    }
                }

                Rectangle cursorNearRectangle = new Rectangle(mousePosition.X - 128, mousePosition.Y - 128, 256, 256);
                imageRectangle.Intersect(cursorNearRectangle);

                // ドラッグするイメージを作成する
                Bitmap bitmap = new Bitmap(imageRectangle.Width, imageRectangle.Height);
                Graphics graphics = Graphics.FromImage(bitmap);

                var r = this.ClientRectangle;
                using (var bm = new Bitmap(r.Width, r.Height))
                {
                    //クライアント領域全体のイメージを取得
                    this.DrawToBitmap(bm, r);

                    foreach (ListViewItem item in this.SelectedItems)
                    {
                        Point itemPosition = item.Position;
                        itemPosition.Offset(-imageRectangle.X, -imageRectangle.Y);

                        Rectangle itemRect = item.GetBounds(ItemBoundsPortion.Label);
                        itemRect.Offset(2,2); //微調整
                        graphics.DrawImage(bm
                                            , new Rectangle(0, 0,
                                                            itemRect.Width, itemRect.Height)
                                            , itemRect
                                            , GraphicsUnit.Pixel
                                            );

                    }
                }


                this.imageList.Images.Clear();

                this.imageList.ImageSize = bitmap.Size;
                this.imageList.Images.Add(bitmap);

                graphics.Dispose();


                Point dragPoint = new Point(mousePosition.X - imageRectangle.X, mousePosition.Y - imageRectangle.Y);
                Win32ImageList.ImageList_BeginDrag(this.imageList.Handle, 0, dragPoint.X, dragPoint.Y);
#endif

                ListViewItem[] dragData = new ListViewItem[this.SelectedItems.Count];
                this.SelectedItems.CopyTo(dragData, 0);

                DragDropEffects dragDropEffect = DoDragDrop(dragData, DragDropEffects.Move);

                // ドラッグ後の処理を行う
                if (dragDropEffect == DragDropEffects.Move)
                {
                    BeginUpdate();
                    foreach (ListViewItem item in dragData)
                    {
                        this.Items.Remove(item);
                    }
                    EndUpdate();
#if __ENABLE_DRAG_GHOST__
                    Win32ImageList.ImageList_EndDrag();
#endif
                }
            }
        }




        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

#if __ENABLE_DRAG_GHOST__
            IntPtr ownerWindow = Win32ImageList.GetDesktopWindow();
            Win32ImageList.ImageList_DragEnter(ownerWindow, Cursor.Position.X, Cursor.Position.Y);
#endif

            if (drgevent.Data.GetDataPresent(typeof(ListViewItem[])))
            {
                drgevent.Effect = DragDropEffects.Move;
            }
            else
            {
                drgevent.Effect = DragDropEffects.None;
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            base.OnQueryContinueDrag(e);

#if __ENABLE_DRAG_GHOST__
            Win32ImageList.ImageList_DragMove(Cursor.Position.X, Cursor.Position.Y);
#endif
            const int RIGHT_BUTTON = 2;
            
            if ((e.KeyState & RIGHT_BUTTON) == RIGHT_BUTTON)
            {
                e.Action = DragAction.Cancel;
            }
            else
            {
                // アイテムの挿入位置を線で表示する
                bool isUpper = false;
                Point p = this.PointToClient(new Point(MousePosition.X, MousePosition.Y));
                var listItem = getListItemByPoint(p, ref isUpper);

                if (listItem != null && (m_prevItem != listItem || m_prevFlag != isUpper))
                {
                    this.Refresh(); // 以前に表示した線を消す
                    drawInsertLine(listItem, isUpper);
                }

                m_prevItem = listItem;
                m_prevFlag = isUpper;
            }

            if (e.Action == DragAction.Drop)
            {
                // ドロップ⇒ドラッグ操作終了
#if __ENABLE_DRAG_GHOST__
                Win32ImageList.ImageList_EndDrag();
#endif
                Invalidate(false);          
            }
        }


        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
#if __ENABLE_DRAG_GHOST__
            IntPtr ownerWindow = Win32ImageList.GetDesktopWindow();
            Win32ImageList.ImageList_DragLeave(ownerWindow);
#endif

            if (e.Data.GetDataPresent(typeof(ListViewItem[])))
            {
                ListViewItem[] dragData = (ListViewItem[])e.Data.GetData(typeof(ListViewItem[]));
                bool isUpper = false;
                Point p = this.PointToClient(new Point(e.X, e.Y));
                var item = getListItemByPoint(p, ref isUpper);

                int index = getInsertTargetIndex(dragData.First(), item, isUpper);

                if (index >= 0)
                {
                    //if (index >= this.Items.Count) index = this.Items.Count - 1;
                    System.Diagnostics.Debug.Print("OnDragDrop: index{0} を index{1} に移動", dragData.First().Index, index);
                    // ListItemMovingイベントを発生させる ⇒ イベント発生先でキャンセルもあり
                    var eventArg = new ListItemMovingEventArgs(dragData, index);
                    OnListItemMoving(eventArg);

                    if (eventArg.Cancel == false)
                    {
                        BeginUpdate();
                        foreach (ListViewItem listViewItem in dragData)
                            this.Items.Remove(listViewItem);
                        EndUpdate();

                        // リスト項目を移動
                        foreach (ListViewItem listViewItem in dragData)
                        {
                            string name = listViewItem.Name; //ListItem#Clone()はNameプロパティまで複製しないので、後で再設定する
                            ListViewItem insertedItem = this.Items.Insert(index, (ListViewItem)listViewItem.Clone());
                            insertedItem.Name = name;
                            insertedItem.Selected = true;
                            insertedItem.Focused = true;
                            index++;
                        }
                    }
                }

                m_prevItem = null;
                m_prevFlag = false;

                Invalidate(false);          
            }
        }

        /// <summary>
        /// アイテム挿入先のインデックス番号を取得します。
        /// </summary>
        /// <param name="draggedItem">ドラッグしているリストアイテム</param>
        /// <param name="droppedTargetItem">ドロップ先のリストアイテム</param>
        /// <param name="isUpperHalf">ドロップ先がリストアイテムの上半分であればtrue。下半分であればfalseを指定します。</param>
        /// <returns></returns>
        private int getInsertTargetIndex(ListViewItem draggedItem, ListViewItem droppedTargetItem, bool isUpperHalf)
        {

            if (droppedTargetItem == null || droppedTargetItem == draggedItem)
            {
                return -1;
            }else
            {
                if (droppedTargetItem.Index == 0)
                {
                    System.Diagnostics.Debug.Print("先頭-{0} にドロップ", isUpperHalf ? "↑" : "↓");
                    return isUpperHalf ? 0 : 1;
                }
                else if (droppedTargetItem.Index >= this.Items.Count-1)
                {
                    System.Diagnostics.Debug.Print("最後-{0} にドロップ", isUpperHalf ? "↑" : "↓");
                    return isUpperHalf ? this.Items.Count - 2 : this.Items.Count - 1 ;
                }
                else
                {
                    int index = -1;
                    System.Diagnostics.Debug.Print("中間-{0} にドロップ", isUpperHalf ? "↑" : "↓");
                    index = droppedTargetItem.Index + (isUpperHalf ? -1 : 0);
                    if (index >= 0 && index < draggedItem.Index) index++;
                    return index;
                }
            }
        }

        public event ListItemMovingEventHandler ListItemMoving;
        
        protected virtual void OnListItemMoving(ListItemMovingEventArgs e)
        {
            if (this.ListItemMoving != null)
                this.ListItemMoving((object)this, e);
        }

        protected override void OnDragLeave(EventArgs e)
        {
            System.Diagnostics.Debug.Print("* DragLeave");
            base.OnDragLeave(e);

            this.Invalidate(false);
        }

        /// <summary>
        /// 指定した位置 p にあるリストアイテムを取得します。
        /// </summary>
        /// <param name="p">検査するリストアイテムの位置</param>
        /// <param name="isUpperHalf">指定位置がリストアイテムの上半分の場合 true。下半分の場合 falseが設定されます。</param>
        /// <returns>指定位置にあるリストアイテム</returns>
        private ListViewItem getListItemByPoint(Point p, ref bool isUpperHalf)
        {
            ListViewItem result = null;
            if (p.X >= 0 && p.X <= this.ClientSize.Width)
            {
                foreach (ListViewItem item in this.Items)
                {
                    var rect = item.GetBounds(ItemBoundsPortion.Entire);
                    if (p.Y >= rect.Top && p.Y <= rect.Bottom)
                    {
                        result = item;
                        isUpperHalf = (p.Y < (rect.Top + rect.Bottom) / 2);
                        break;
                    }
                }


                if (result == null && this.Items.Count > 0)
                {
                    var firstItem = this.Items[0];
                    var firstRect = firstItem.GetBounds(ItemBoundsPortion.Entire);
                    if (p.Y <= firstRect.Y)
                    {
                        result = firstItem;
                        isUpperHalf = true;
                    }
                    else
                    {
                        var lastItem = this.Items[this.Items.Count - 1];
                        var lastRect = lastItem.GetBounds(ItemBoundsPortion.Entire);
                        if (p.Y >= lastRect.Bottom)
                        {
                            result = lastItem;
                            isUpperHalf = false;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 移動アイテムの挿入位置を表現する線を描画します。
        /// </summary>
        /// <param name="listItem">描画する位置のリストアイテム</param>
        /// <param name="isUpper">線の描画場所を指定します。引数 listItem の上側であれば true、下側であれば false を指定します。 </param>
        private void drawInsertLine(ListViewItem listItem, bool isUpper)
        {
            using (var g = this.CreateGraphics())
            {
                var rect = listItem.GetBounds(ItemBoundsPortion.ItemOnly);
                int y = isUpper ? rect.Top : rect.Bottom;
                Point ptL = new Point(0, y);
                Point ptR = new Point(rect.Width, y);
                g.DrawLine(new Pen(DRAG_TARGET_LINE_COLOR, 2), ptL, ptR);
            }
        }

#if __ENABLE_DRAG_GHOST__
        private static class Win32ImageList
        {
            [DllImport("comctl32.dll")]
            public static extern bool ImageList_BeginDrag(
                IntPtr himlTrack,  // イメージリストのハンドル
                int iTrack,        // ドラッグするイメージの番号
                int dxHotspot,     // ドラッグ位置 (イメージ位置との相対座標)
                int dyHotspot      //
                );

            [DllImport("comctl32.dll")]
            public static extern bool ImageList_DragEnter(
                IntPtr hwndLock,   // ドラッグするイメージの親となるウィンドウのハンドル
                int x,    // ドラッグするイメージの表示位置 (ウィンドウ位置との相対座標)
                int y     //
                );

            [DllImport("comctl32.dll")]
            public static extern bool ImageList_DragLeave(
                IntPtr hwndLock    // ドラッグするイメージの親となるウィンドウのハンドル
                );

            [DllImport("comctl32.dll")]
            public static extern bool ImageList_DragMove(
                int x,    // ドラッグするイメージの表示位置 (ウィンドウ位置との相対座標)
                int y     //
                );

            [DllImport("comctl32.dll")]
            public static extern void ImageList_EndDrag();


            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
        }
#endif
    }


    public class ListItemMovingEventArgs : EventArgs
    {
        public ListItemMovingEventArgs(IEnumerable<ListViewItem> listItems,int targetIndex)
        {
            List<ListViewItem> itemList = new List<ListViewItem>(listItems);
            this.ListItems = itemList.AsReadOnly();
            this.TargetIndex = targetIndex;
        }

        public readonly IList<ListViewItem> ListItems;
        public readonly int TargetIndex;
        public bool Cancel { get; set; }
    }

    public delegate void ListItemMovingEventHandler(object sender, ListItemMovingEventArgs e);

}
