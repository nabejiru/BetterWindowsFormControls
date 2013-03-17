//�h���b�O���A�C�e���𔼓����ɂ���@�\�ˎ�����f�O�i�@�\���邪�A�ړ���̕\���Ƒ����������c�j�B
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
    /// ���X�g�A�C�e���̃h���b�O����ɂ��ړ�����������ListView�R���g���[��
    /// </summary>
    /// <author>nabejiru</author>
    public class DraggableListView : ListView
    {
        private ImageList imageList = new ImageList();
        /// <summary>�A�C�e���̈ړ����\��������̐F</summary>
        private readonly Color DRAG_TARGET_LINE_COLOR = Color.Red;

        ListViewItem m_prevItem = null;
        bool m_prevFlag = false;

        public DraggableListView()
            : base()
        {
            this.AllowItemDrag = true;
        }


        [Category("�Ǝ��v���p�e�B")
        , DefaultValue(true)
        , Description("���X�g�A�C�e���̃h���b�O���삪�\���ǂ������擾�A�ݒ肵�܂��B")]
        public bool AllowItemDrag
        {
            get;
            set;
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            // �h���b�O���L�����Z��
            if (AllowItemDrag == false) return;

            base.OnItemDrag(e);

            if (e.Button == MouseButtons.Left)
            {
#if __ENABLE_DRAG_GHOST__
                Point mousePosition = this.PointToClient(Control.MousePosition);

                // �h���b�O����C���[�W�̃T�C�Y�����߂�
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

                // �h���b�O����C���[�W���쐬����
                Bitmap bitmap = new Bitmap(imageRectangle.Width, imageRectangle.Height);
                Graphics graphics = Graphics.FromImage(bitmap);

                var r = this.ClientRectangle;
                using (var bm = new Bitmap(r.Width, r.Height))
                {
                    //�N���C�A���g�̈�S�̂̃C���[�W���擾
                    this.DrawToBitmap(bm, r);

                    foreach (ListViewItem item in this.SelectedItems)
                    {
                        Point itemPosition = item.Position;
                        itemPosition.Offset(-imageRectangle.X, -imageRectangle.Y);

                        Rectangle itemRect = item.GetBounds(ItemBoundsPortion.Label);
                        itemRect.Offset(2,2); //������
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

                // �h���b�O��̏������s��
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
                // �A�C�e���̑}���ʒu����ŕ\������
                bool isUpper = false;
                Point p = this.PointToClient(new Point(MousePosition.X, MousePosition.Y));
                var listItem = getListItemByPoint(p, ref isUpper);

                if (listItem != null && (m_prevItem != listItem || m_prevFlag != isUpper))
                {
                    this.Refresh(); // �ȑO�ɕ\��������������
                    drawInsertLine(listItem, isUpper);
                }

                m_prevItem = listItem;
                m_prevFlag = isUpper;
            }

            if (e.Action == DragAction.Drop)
            {
                // �h���b�v�˃h���b�O����I��
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
                    System.Diagnostics.Debug.Print("OnDragDrop: index{0} �� index{1} �Ɉړ�", dragData.First().Index, index);
                    // ListItemMoving�C�x���g�𔭐������� �� �C�x���g������ŃL�����Z��������
                    var eventArg = new ListItemMovingEventArgs(dragData, index);
                    OnListItemMoving(eventArg);

                    if (eventArg.Cancel == false)
                    {
                        BeginUpdate();
                        foreach (ListViewItem listViewItem in dragData)
                            this.Items.Remove(listViewItem);
                        EndUpdate();

                        // ���X�g���ڂ��ړ�
                        foreach (ListViewItem listViewItem in dragData)
                        {
                            string name = listViewItem.Name; //ListItem#Clone()��Name�v���p�e�B�܂ŕ������Ȃ��̂ŁA��ōĐݒ肷��
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
        /// �A�C�e���}����̃C���f�b�N�X�ԍ����擾���܂��B
        /// </summary>
        /// <param name="draggedItem">�h���b�O���Ă��郊�X�g�A�C�e��</param>
        /// <param name="droppedTargetItem">�h���b�v��̃��X�g�A�C�e��</param>
        /// <param name="isUpperHalf">�h���b�v�悪���X�g�A�C�e���̏㔼���ł����true�B�������ł����false���w�肵�܂��B</param>
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
                    System.Diagnostics.Debug.Print("�擪-{0} �Ƀh���b�v", isUpperHalf ? "��" : "��");
                    return isUpperHalf ? 0 : 1;
                }
                else if (droppedTargetItem.Index >= this.Items.Count-1)
                {
                    System.Diagnostics.Debug.Print("�Ō�-{0} �Ƀh���b�v", isUpperHalf ? "��" : "��");
                    return isUpperHalf ? this.Items.Count - 2 : this.Items.Count - 1 ;
                }
                else
                {
                    int index = -1;
                    System.Diagnostics.Debug.Print("����-{0} �Ƀh���b�v", isUpperHalf ? "��" : "��");
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
        /// �w�肵���ʒu p �ɂ��郊�X�g�A�C�e�����擾���܂��B
        /// </summary>
        /// <param name="p">�������郊�X�g�A�C�e���̈ʒu</param>
        /// <param name="isUpperHalf">�w��ʒu�����X�g�A�C�e���̏㔼���̏ꍇ true�B�������̏ꍇ false���ݒ肳��܂��B</param>
        /// <returns>�w��ʒu�ɂ��郊�X�g�A�C�e��</returns>
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
        /// �ړ��A�C�e���̑}���ʒu��\���������`�悵�܂��B
        /// </summary>
        /// <param name="listItem">�`�悷��ʒu�̃��X�g�A�C�e��</param>
        /// <param name="isUpper">���̕`��ꏊ���w�肵�܂��B���� listItem �̏㑤�ł���� true�A�����ł���� false ���w�肵�܂��B </param>
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
                IntPtr himlTrack,  // �C���[�W���X�g�̃n���h��
                int iTrack,        // �h���b�O����C���[�W�̔ԍ�
                int dxHotspot,     // �h���b�O�ʒu (�C���[�W�ʒu�Ƃ̑��΍��W)
                int dyHotspot      //
                );

            [DllImport("comctl32.dll")]
            public static extern bool ImageList_DragEnter(
                IntPtr hwndLock,   // �h���b�O����C���[�W�̐e�ƂȂ�E�B���h�E�̃n���h��
                int x,    // �h���b�O����C���[�W�̕\���ʒu (�E�B���h�E�ʒu�Ƃ̑��΍��W)
                int y     //
                );

            [DllImport("comctl32.dll")]
            public static extern bool ImageList_DragLeave(
                IntPtr hwndLock    // �h���b�O����C���[�W�̐e�ƂȂ�E�B���h�E�̃n���h��
                );

            [DllImport("comctl32.dll")]
            public static extern bool ImageList_DragMove(
                int x,    // �h���b�O����C���[�W�̕\���ʒu (�E�B���h�E�ʒu�Ƃ̑��΍��W)
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
