using System;
using System.Windows;
using System.Windows.Controls;
using TBT.App.Helpers;

namespace TBT.App.Views.Controls
{
    public partial class TimeEntryItemsControl : UserControl, IDisposable
    {
        private double _timeEntryControlHeight;

        public TimeEntryItemsControl()
        {
            var tempControl = new TimeEntryControl();
            tempControl.Measure(new Size(int.MaxValue, int.MaxValue));
            _timeEntryControlHeight = tempControl.DesiredSize.Height - 
                tempControl.timerTextBlock.DesiredSize.Height - tempControl.commentArea.DesiredSize.Height
                - tempControl.saveButton.DesiredSize.Height;
            RefreshEvents.ScrollTimeEntryItemsToTop += () => { RefreshScrollView(0); };
            InitializeComponent();
        }

        public void RefreshScrollView(int id)
        {
            TimeEntriesScrollView.ScrollToVerticalOffset(_timeEntryControlHeight * id);
        }

        private bool disposed = false;

        public virtual void Dispose()
        {
            if (disposed) { return; }

            disposed = true;
        }
    }
}
