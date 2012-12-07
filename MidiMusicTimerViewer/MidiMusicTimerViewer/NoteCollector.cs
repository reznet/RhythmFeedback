using Midi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiMusicTimerViewer
{
    class NoteCollector
    {
        //private readonly Dictionary<Channel, Dictionary<Pitch, NoteOnMessage>> incompleteMessages = new Dictionary<Channel, Dictionary<Pitch, NoteOnMessage>>();

        private readonly ConcurrentDictionary<Channel, ConcurrentDictionary<Pitch, NoteOnMessage>> incompleteMessages = new ConcurrentDictionary<Channel, ConcurrentDictionary<Pitch, NoteOnMessage>>();

        private List<Note> completedNotes = new List<Note>();

        private readonly Clock clock;

        private readonly object lockObject = new object();

        public NoteCollector(Clock clock)
        {
            this.clock = clock;

            foreach (var i in Enumerable.Range(0, 16))
            {
                incompleteMessages[(Channel)i] = new ConcurrentDictionary<Pitch, NoteOnMessage>();
                foreach (var j in Enumerable.Range(0, 127))
                {
                    incompleteMessages[(Channel)i][(Pitch)j] = null;
                }
            }
        }

        public void ProcessMidiMessage(NoteOnMessage noteOnMessage)
        {
            var channel = noteOnMessage.Channel;

            Trace.TraceInformation("Got note ON  for channel " + channel.Name() + " and pitch " + noteOnMessage.Pitch.NotePreferringFlats());

            Debug.Assert(incompleteMessages[channel][noteOnMessage.Pitch] == null, "there's already a note on message for channel " + channel.Name() + " and pitch " + noteOnMessage.Pitch);

            incompleteMessages[channel][noteOnMessage.Pitch] = noteOnMessage;
        }

        public void ProcessMidiMessage(NoteOffMessage message)
        {
            var channel = message.Channel;

            Trace.TraceInformation("Got note OFF for channel " + channel.Name() + " and pitch " + message.Pitch.NotePreferringFlats());

            NoteOnMessage noteOnMessage = incompleteMessages[channel][message.Pitch];
            if (noteOnMessage == null)
            {
                return;
            }

            incompleteMessages[channel][message.Pitch] = null;

            lock (lockObject)
            {
                completedNotes.Add(new Note { Channel = channel, StartTime = noteOnMessage.Time, EndTime = message.Time, Pitch = noteOnMessage.Pitch });
            }
        }

        public IEnumerable<Note> GetNotes()
        {
            Note[] notes = null;
            lock (lockObject)
            {
                notes = completedNotes.ToArray();
            }

            return notes.Union(GetIncompleteNotes().ToArray());
        }

        private IEnumerable<Note> GetIncompleteNotes()
        {
            foreach (var i in Enumerable.Range(0, 16))
            {
                var dictionary = incompleteMessages[(Channel)i];
                foreach (var j in Enumerable.Range(0, 127))
                {
                    NoteOnMessage message = dictionary[(Pitch)j];
                    if (message != null)
                    {
                        yield return new Note { Channel = (Channel)i, StartTime = message.Time, EndTime = clock.Time, Pitch = message.Pitch };
                    }
                }
            }
        }
    }
}
