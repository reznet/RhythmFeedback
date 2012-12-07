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

            notesControl1.Clock = clock;
            notesControl1.NoteCollector = noteCollector;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clock.Start();

            inputDevice = InputDevice.InstalledDevices[0];
            inputDevice.Open();
            inputDevice.NoteOn += msg => { noteCollector.ProcessMidiMessage(msg); messages.Add(msg); notesControl1.Invalidate(); };
            inputDevice.NoteOff += msg => { noteCollector.ProcessMidiMessage(msg); notesControl1.Invalidate(); };
            inputDevice.StartReceiving(clock);
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
            notesControl1.Invalidate();
        }
    }
}
