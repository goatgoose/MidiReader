using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoteOnEvent : MidiEvent {

	public Midi.EventType eventType { get; protected set; }

	public int delay { get; set; }

	public long executeTime { get; set; }

	public int channel { get; protected set; }

	public int note { get; protected set; }

	public int velocity { get; protected set; }

	public NoteOnEvent(int delay, int channel, int note, int velocity) {
		this.eventType = Midi.EventType.NOTE_ON;
		this.delay = delay;
		this.channel = channel;
		this.note = note;
		this.velocity = velocity;
	}
}
