using System;
using System.Drawing;
using System.Windows.Forms;

namespace EVEMon.Common.Controls
{
    public class NoFlickerListBox : ListBox
    {
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ListBox.DrawItem"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs"/> that contains the event data.</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Rectangle newBounds = new Rectangle(0, 0, e.Bounds.Width, e.Bounds.Height);

            if (newBounds.Width == 0 || newBounds.Height == 0)
                return;

            // stacked using blocks to avoid indentation, don't need to call IDisposable.Dispose explicitly
            using (BufferedGraphicsContext currentContext = new BufferedGraphicsContext())
            using (BufferedGraphics bufferedGraphics = currentContext.Allocate(e.Graphics, newBounds))
            {
                DrawItemEventArgs newArgs = new DrawItemEventArgs(
                    bufferedGraphics.Graphics, e.Font, newBounds, e.Index, e.State, e.ForeColor, e.BackColor);

                // Supply the real DrawItem with the off-screen graphics context
                base.OnDrawItem(newArgs);

                NativeMethods.CopyGraphics(e.Graphics, e.Bounds, bufferedGraphics.Graphics, new Point(0, 0));
            }
        }

        /// <summary>
        /// Enumerations of Window Messages
        /// </summary>
        private enum WM
        {
            WM_NULL = 0x0000,
            WM_ERASEBKGND = 0x0014
        }

        /// <summary>
        /// The list's window procedure.
        /// </summary>
        /// <param name="m">A Windows Message Object.</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WM.WM_ERASEBKGND:
                    PaintNonItemRegion();
                    m.Msg = (int)WM.WM_NULL;
                    m.LParam = IntPtr.Zero;
                    m.WParam = IntPtr.Zero;
                    m.Result = IntPtr.Zero;
                    break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Paints the non item region.
        /// </summary>
        private void PaintNonItemRegion()
        {
            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                using (Region r = new Region(ClientRectangle))
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        Rectangle itemRect = GetItemRectangle(i);
                        r.Exclude(itemRect);
                    }

                    g.FillRegion(SystemBrushes.Window, r);
                }
            }
        }
    }
}
