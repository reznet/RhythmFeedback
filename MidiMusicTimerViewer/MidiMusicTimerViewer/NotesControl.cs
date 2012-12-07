using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using Midi;

namespace MidiMusicTimerViewer
{
    public partial class NotesControl : UserControl
    {
        private const int SecondsPerScreen = 10;

        public NotesControl()
        {
            // enable double buffering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            InitializeComponent();
        }

        [DefaultValue(null)]
        [Browsable(false)]
        public Clock Clock { get; set; }

        [DefaultValue(null)]
        [Browsable(false)]
        public NoteCollector NoteCollector { get; set; }

        private void NotesControl_Paint(object sender, PaintEventArgs e)
        {
            int currentScreen = Clock == null ? 0 : (int)(Clock.Time / SecondsPerScreen);
            float widthOneSecond = (float)ClientSize.Width / (float)SecondsPerScreen;
            float pixelsPerPitch = ClientSize.Height / 127f;

            string currentScreenText = currentScreen.ToString(CultureInfo.InvariantCulture);
            SizeF currentScreenTextSize = e.Graphics.MeasureString(currentScreenText, this.Font);
            e.Graphics.DrawString(currentScreen.ToString(CultureInfo.InvariantCulture), this.Font, Brushes.Black, new RectangleF(new PointF(ClientRectangle.Right - currentScreenTextSize.Width, currentScreenTextSize.Height), currentScreenTextSize));

            float widthOfSecond = ClientSize.Width / SecondsPerScreen;

            if (NoteCollector == null)
            {
                return;
            }

            foreach (var note in NoteCollector.GetNotes().Where(n => SecondsPerScreen * currentScreen <= n.StartTime && n.StartTime <= (currentScreen + 1) * SecondsPerScreen))
            {
                float x = (note.StartTime % SecondsPerScreen) * widthOfSecond;
                float y = ClientSize.Height - (((int)note.Pitch + 1) * pixelsPerPitch);
                float w = ((note.EndTime - note.StartTime) * widthOneSecond);
                float h = pixelsPerPitch;
                e.Graphics.FillRectangle(Brushes.Blue, new RectangleF(x, y, w, h));
                e.Graphics.DrawLine(Pens.Black, x, 0, x, ClientSize.Height);
            }
        }
    }
}
