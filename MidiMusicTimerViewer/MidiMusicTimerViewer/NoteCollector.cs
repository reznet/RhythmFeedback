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
    /// <summary>
    /// Collects and aggregates note messages.
    /// </summary>
    public class NoteCollector
    {
        /// <summary>
        /// A mapping between channel, pitch, and note on message.  Used to build Notes when the note off message is received.
        /// </summary>
        private readonly ConcurrentDictionary<Channel, ConcurrentDictionary<Pitch, NoteOnMessage>> incompleteMessages = new ConcurrentDictionary<Channel, ConcurrentDictionary<Pitch, NoteOnMessage>>();

        /// <summary>
        /// A collection of notes which have started and ended.
        /// </summary>
        private readonly List<Note> completedNotes = new List<Note>();

        /// <summary>
        /// The clock used to determine start and end times for notes.
        /// </summary>
        private readonly Clock clock;

        /// <summary>
        /// An object used to protect access to the completed notes collection.
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the NoteCollector class.
        /// </summary>
        /// <param name="clock">The clock that serves a as a reference for note start and end times.</param>
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

        /// <summary>
        /// Process the Note On message.
        /// </summary>
        /// <param name="noteOnMessage">The note on message.</param>
        public void ProcessMidiMessage(NoteOnMessage noteOnMessage)
        {
            var channel = noteOnMessage.Channel;

            Trace.TraceInformation("Got note ON  for channel " + channel.Name() + " and pitch " + noteOnMessage.Pitch.NotePreferringFlats());

            Debug.Assert(incompleteMessages[channel][noteOnMessage.Pitch] == null, "there's already a note on message for channel " + channel.Name() + " and pitch " + noteOnMessage.Pitch);

            incompleteMessages[channel][noteOnMessage.Pitch] = noteOnMessage;
        }

        /// <summary>
        /// Process the Note Off message.
        /// </summary>
        /// <param name="message">The note off message.</param>
        public void ProcessMidiMessage(NoteOffMessage message)
        {
            var channel = message.Channel;

            Trace.TraceInformation("Got note OFF for channel " + channel.Name() + " and pitch " + message.Pitch.NotePreferringFlats());

            NoteOnMessage noteOnMessage = incompleteMessages[channel][message.Pitch];
            if (noteOnMessage == null)
            {
                Trace.TraceWarning("Got note off message but don't have a note on message for channel " + channel.Name() + " and pitch " + message.Pitch.NotePreferringFlats() + ".  Ignoring message.");
                return;
            }

            incompleteMessages[channel][message.Pitch] = null;

            lock (lockObject)
            {
                completedNotes.Add(new Note 
                { 
                    Channel = channel, 
                    StartTime = noteOnMessage.Time, 
                    EndTime = message.Time, 
                    Pitch = noteOnMessage.Pitch 
                });
            }
        }

        /// <summary>
        /// Get the set of Notes this collector has tracked.
        /// </summary>
        /// <returns>A sequence of Notes.</returns>
        /// <remarks>
        /// This returns the union of completed notes (notes with both start and end messages) as well as notes that are still on.  For notes that are still on
        /// the end time is taken from the clock's current time.
        /// </remarks>
        public IEnumerable<Note> GetNotes()
        {
            Note[] notes = null;
            lock (lockObject)
            {
                notes = completedNotes.ToArray();
            }

            return notes.Union(GetIncompleteNotes().ToArray());
        }

        /// <summary>
        /// Search the incomplete message map and return Notes for each note on message that isn't off yet.
        /// </summary>
        /// <returns></returns>
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
                        yield return new Note 
                        { 
                            Channel = (Channel)i, 
                            StartTime = message.Time, 
                            EndTime = clock.Time, 
                            Pitch = message.Pitch 
                        };
                    }
                }
            }
        }
    }
}
