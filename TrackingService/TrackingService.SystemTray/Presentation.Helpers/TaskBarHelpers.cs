namespace ImmotionAR.ImmotionRoom.TrackingService.Presentation.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;

    // See: https://winsharp93.wordpress.com/2009/06/29/find-out-size-and-position-of-the-taskbar/
    internal sealed class TaskbarHelpers
    {
        // See: http://stackoverflow.com/questions/1264406/how-do-i-get-the-taskbars-position-and-size
        // 0 rectangles means the taskbar is hidden;
        // 1 rectangle is the position of the taskbar;
        // 2+ is very rare, it means that we have multiple monitors, and we are not using Extend these displays to create a single virtual desktop.
        private static List<Rectangle> FindDockedTaskBars()
        {
            var dockedRects = new List<Rectangle>();

            foreach (var tmpScrn in Screen.AllScreens)
            {
                if (!tmpScrn.Bounds.Equals(tmpScrn.WorkingArea))
                {
                    var rect = new Rectangle();

                    var leftDockedWidth = Math.Abs(Math.Abs(tmpScrn.Bounds.Left) - Math.Abs(tmpScrn.WorkingArea.Left));
                    var topDockedHeight = Math.Abs(Math.Abs(tmpScrn.Bounds.Top) - Math.Abs(tmpScrn.WorkingArea.Top));
                    var rightDockedWidth = tmpScrn.Bounds.Width - leftDockedWidth - tmpScrn.WorkingArea.Width;
                    var bottomDockedHeight = tmpScrn.Bounds.Height - topDockedHeight - tmpScrn.WorkingArea.Height;
                    if (leftDockedWidth > 0)
                    {
                        rect.X = tmpScrn.Bounds.Left;
                        rect.Y = tmpScrn.Bounds.Top;
                        rect.Width = leftDockedWidth;
                        rect.Height = tmpScrn.Bounds.Height;
                    }
                    else if (rightDockedWidth > 0)
                    {
                        rect.X = tmpScrn.WorkingArea.Right;
                        rect.Y = tmpScrn.Bounds.Top;
                        rect.Width = rightDockedWidth;
                        rect.Height = tmpScrn.Bounds.Height;
                    }
                    else if (topDockedHeight > 0)
                    {
                        rect.X = tmpScrn.WorkingArea.Left;
                        rect.Y = tmpScrn.Bounds.Top;
                        rect.Width = tmpScrn.WorkingArea.Width;
                        rect.Height = topDockedHeight;
                    }
                    else if (bottomDockedHeight > 0)
                    {
                        rect.X = tmpScrn.WorkingArea.Left;
                        rect.Y = tmpScrn.WorkingArea.Bottom;
                        rect.Width = tmpScrn.WorkingArea.Width;
                        rect.Height = bottomDockedHeight;
                    }

                    dockedRects.Add(rect);
                }
            }

            if (dockedRects.Count == 0)
            {
                // Taskbar is set to "Auto-Hide".
            }

            return dockedRects;
        }

        private enum TaskBarLocation
        {
            TOP,
            BOTTOM,
            LEFT,
            RIGHT
        }

        private TaskBarLocation GetTaskBarLocation()
        {
            //System.Windows.SystemParameters....
            if (SystemParameters.WorkArea.Left > 0)
                return TaskBarLocation.LEFT;
            if (SystemParameters.WorkArea.Top > 0)
                return TaskBarLocation.TOP;
            if (SystemParameters.WorkArea.Left == 0 && SystemParameters.WorkArea.Width < SystemParameters.PrimaryScreenWidth)
                return TaskBarLocation.RIGHT;
            return TaskBarLocation.BOTTOM;
        }
    }
}