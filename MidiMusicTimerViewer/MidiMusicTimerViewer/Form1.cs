using Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiMusicTimerViewer
{
    public partial class Form1 : Form
    {
        private const int SecondsPerScreen = 10;

        private List<NoteOnMessage> messages;

        Clock clock;

        NoteCollector noteCollector;

        InputDevice inputDevice;
        
        public Form1()
        {
            InitializeComponent();

            messages = new List<NoteOnMessage>();
            clock = new Clock(128);
            noteCollector = new NoteCollector(clock);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clock.Start();

            inputDevice = InputDevice.InstalledDevices[0];
            inputDevice.Open();
            inputDevice.NoteOn += msg => { noteCollector.ProcessMidiMessage(msg); messages.Add(msg); Invalidate(); };
            inputDevice.NoteOff += msg => { noteCollector.ProcessMidiMessage(msg); Invalidate(); };
            inputDevice.StartReceiving(clock);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int currentScreen = (int)(clock.Time / SecondsPerScreen);
            float widthOneSecond = (float)ClientSize.Width / (float)SecondsPerScreen;
            float pixelsPerPitch = ClientSize.Height / 127f;

            string currentScreenText = currentScreen.ToString(CultureInfo.InvariantCulture);
            SizeF currentScreenTextSize = e.Graphics.MeasureString(currentScreenText, this.Font);
            e.Graphics.DrawString(currentScreen.ToString(CultureInfo.InvariantCulture), this.Font, Brushes.Black, new RectangleF(new PointF(ClientRectangle.Right - currentScreenTextSize.Width, currentScreenTextSize.Height), currentScreenTextSize));

            float widthOfSecond = ClientSize.Width / SecondsPerScreen;

            foreach (var note in noteCollector.GetNotes().Where(n => SecondsPerScreen * currentScreen <= n.StartTime && n.StartTime <= (currentScreen + 1) * SecondsPerScreen))
            {
                float x = (note.StartTime % SecondsPerScreen) * widthOfSecond;
                float y = ClientSize.Height - (((int)note.Pitch + 1) * pixelsPerPitch);
                float w = ((note.EndTime - note.StartTime) * widthOneSecond);
                float h = pixelsPerPitch;
                e.Graphics.FillRectangle(Brushes.Blue, new RectangleF(x, y, w, h));
                e.Graphics.DrawLine(Pens.Black, x, 0, x, ClientSize.Height);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (clock.IsRunning)
            {
                clock.Stop();
            }

            if (inputDevice.IsReceiving)
            {
                inputDevice.StopReceiving();
            }

            inputDevice.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
