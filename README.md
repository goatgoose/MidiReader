# MidiReader

C# midi reader intended for Unity

## Example usage

```C#
void play () {
  midi = new Midi (this, gameObject.GetComponent<AudioSource> ().clip.name);

  midi.on (Midi.EventType.NOTE_ON, delegate(MidiEvent midiEvent) {
    NoteOnEvent noteOnEvent = midiEvent as NoteOnEvent;
    pressKey (noteOnEvent.note);
  });

  midi.on (Midi.EventType.NOTE_OFF, delegate(MidiEvent midiEvent) {
    NoteOffEvent noteOffEvent = midiEvent as NoteOffEvent;
    liftKey (noteOffEvent.note);
  });

  midi.start ();
  gameObject.GetComponent<AudioSource> ().Play ();
}
```
