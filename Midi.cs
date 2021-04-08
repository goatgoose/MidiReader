using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Midi {

	private MonoBehaviour monoBehaviour;

	private FileFormat fileFormat;

	private int trackCount;

	private int trackLength;

	private int resolution;

	private int tempo = 500000; // microseconds per quarter note

	private List<MidiEvent> events;
	private int eventsIndex = 0;

	private Dictionary<int, Action<MidiEvent>> callbacks; // event type index to callback

	private bool playing;

	public Midi(MonoBehaviour monoBehaviour, string midiName) {
		this.monoBehaviour = monoBehaviour;
		events = new List<MidiEvent> ();
		callbacks = new Dictionary<int, Action<MidiEvent>> ();
		playing = false;

		string midiPath = Application.dataPath + "/Resources/midis/" + midiName + ".mid";
		StreamManager stream = new StreamManager (midiPath);

		// http://www.petesqbsite.com/sections/express/issue18/midifilespart1.html
		if (!stream.ReadChars (4).Equals("MThd")) {
			throw new ApplicationException ("invalid header");;
		}
		stream.ReadInt (); // header len
		fileFormat = (FileFormat)stream.ReadShort ();
		trackCount = (int) stream.ReadShort ();
		resolution = stream.ReadShort ();

		if (!fileFormat.Equals(FileFormat.SINGLE_TRACK)) {
			throw new ApplicationException ("multi track not supported");
		}
		if (!stream.ReadChars (4).Equals("MTrk")) {
			throw new ApplicationException ("invalid track header");;
		}

		trackLength = stream.ReadInt ();

		Debug.Log ("fileFormat: " + fileFormat);
		Debug.Log ("trackCount: " + trackCount);
		Debug.Log ("resolution: " + resolution);
		Debug.Log ("trackLength: " + trackLength);

		byte previous = 0x00;

		bool containsMore = true;
		while (containsMore) {
			int deltaTime = stream.ReadVariableLengthInt ();

			byte next = (byte) stream.ReadByte ();
			switch (next) {
			case 0xF0: // sysex event
				{
					Debug.Log ("sysex event");
					int len = stream.ReadVariableLengthInt ();
					for (int i = 0; i < len; i++) {
						stream.ReadByte (); // ignore for now
					}
					break;
				}
			case 0xF7: // alternate sysex
				{
					Debug.Log ("alternate sysex event");
					int len = stream.ReadVariableLengthInt ();
					for (int i = 0; i < len; i++) {
						stream.ReadByte (); // ignore for now
					}
					break;
				}
			case 0xFF: // meta event
				{
					byte type = stream.ReadByte ();
					int len = stream.ReadVariableLengthInt ();
					switch (type) {
					case 0x51: // set tempo
						{
							int newTempo = stream.Read24BitInt ();
							Debug.Log (deltaTime + " - meta set tempo event: " + 60000000 / newTempo);
							events.Add (new ChangeTempoEvent (deltaTime, newTempo));
							break;
						}
					case 0x54:
						{
							byte hour = stream.ReadByte ();
							byte min = stream.ReadByte ();
							byte sec = stream.ReadByte ();
							byte frame = stream.ReadByte ();
							byte ff = stream.ReadByte ();
							Debug.Log ("SMPTE offset: " + hour + "/" + min + "/" + sec + "/" + frame + "/" + ff);
							break;
						}
					case 0x58: // time signature
						{
							byte numerator = stream.ReadByte ();
							byte denominatorExponent = stream.ReadByte ();
							byte clocksPerTick = stream.ReadByte ();
							byte clockResolution = stream.ReadByte ();
							Debug.Log ("time signature: " + numerator + "/" + Math.Pow (2, denominatorExponent) + ", clocksPerTick: " + clocksPerTick + ", 1/32 notes per 34 clocks: " + clockResolution);
							break;
						}
					case 0x2F: // end of track
						{
							events.Add (new EndOfTrackEvent (deltaTime));
							Debug.Log ("end of track");
							containsMore = false;
							break;
						}

					default: 
						{
							Debug.Log ("meta event: " + type.ToString ());
							// ignore for now
							for (int i = 0; i < len; i++) {
								stream.ReadByte (); 
							}
							break;
						}
					}
					break;
				}

			default: // midi event
				{
					//Debug.Log ("midi event: " + next.ToString ());

					byte data1;

					if (next <= 0x7f) {
						data1 = next;
						next = previous;
					} else {
						data1 = stream.ReadByte ();
					}

					if (next >= 0xF0) { // system message
						Debug.Log("system message: " + next.ToString());
						int status = next & 0xf;

						// ignore for now
						switch (status) {
						case 0xF1:
							{
								stream.ReadByte ();
								break;
							}
						case 0xF2:
							{
								stream.ReadByte ();
								stream.ReadByte ();
								break;
							}
						case 0xF3:
							{
								stream.ReadByte ();
								break;
							}

						default: // no others have data bytes
							{
								break;
							}
						}
					} else { // channel message
						int status = next >> 4;
						int channel = next & 0xf;

						switch (status) {
						case 0x8: // note off
							{
								byte data2 = stream.ReadByte ();
								//Debug.Log(deltaTime + " - note off: " + data1.ToString ());
								events.Add (new NoteOffEvent (deltaTime, channel, data1, data2));
								break;
							}
						case 0x9: // note on
							{
								byte data2 = stream.ReadByte ();
								if (data2 == 0) { // velocity 0 so actually note off
									events.Add (new NoteOffEvent (deltaTime, channel, data1, 127));
									//Debug.Log(deltaTime + " - note off: " + data1.ToString ());
								} else {
									events.Add (new NoteOnEvent (deltaTime, channel, data1, data2));
									//Debug.Log(deltaTime + " - note on: " + data1.ToString ());
								}
								break;
							}
						case 0xA: // polyphonic key pressure
							{
								Debug.Log ("polyphonic key pressure");
								byte data2 = stream.ReadByte ();
								break;
							}
						case 0xB: // control change
							{
								byte data2 = stream.ReadByte ();
								//Debug.Log ("control change: " + data1 + ", value: " + data2);
								events.Add (new ControlChangeEvent (deltaTime, data1, data2));
								break;
							}
						case 0xC: // program change
							{
								Debug.Log ("program change");
								break;
							}
						case 0xD: // channel pressure
							{
								Debug.Log ("channel pressure");
								break;
							}
						case 0xE: // pitch wheel change
							{
								byte data2 = stream.ReadByte ();
								Debug.Log ("pitch wheel change");
								break;
							}
						case 0xF: // channel mode message
							{
								byte data2 = stream.ReadByte ();
								Debug.Log ("channel mode message");
								break;
							}

						default: // other
							{
								Debug.Log ("should never happen - dt:" + deltaTime + " status: " + Convert.ToString (status, 2) + " byte: " + next.ToString ());
								break;
							}
						}
					}
					previous = next;
					break;
				}
			}
		}

	}

	public void start() {
		long timeToExecute = DateTime.Now.Ticks; // TODO sync to time relative to audiosource component
		for (int i = 0; i < events.Count; i++) {
			MidiEvent midiEvent = events [i];

			timeToExecute += Convert.ToInt64(midiEvent.delay * getTickLengthMicroSec() * 10); // micro to nanoseconds / 100
			midiEvent.executeTime = timeToExecute;

			if (midiEvent.eventType == EventType.CHANGE_TEMPO) {
				tempo = (midiEvent as ChangeTempoEvent).tempo;
			}
		}

		playing = true;
		monoBehaviour.StartCoroutine (waitForNextEvent ());
	}

	private IEnumerator waitForNextEvent() {
		yield return new WaitUntil (() => DateTime.Now.Ticks >= events[eventsIndex].executeTime);
		while (DateTime.Now.Ticks >= events [eventsIndex].executeTime && eventsIndex < events.Count) {
			executeEvent(events [eventsIndex]);
			eventsIndex++;
		}
		if (playing && eventsIndex < events.Count) {
			yield return monoBehaviour.StartCoroutine (waitForNextEvent ());
		}
	}

	private void executeEvent(MidiEvent midiEvent) {
		if (callbacks.ContainsKey ((int) midiEvent.eventType)) {
			callbacks [(int)midiEvent.eventType] (midiEvent);
		}
	}

	public void on(EventType eventType, Action<MidiEvent> callback) {
		callbacks.Add ((int)eventType, callback);
	}

	public enum EventType {
		NOTE_OFF,
		NOTE_ON,
		POLYPHONIC_KEY_PRESSURE,
		CONTROL_CHANGE,
		PROGRAM_CHANGE,
		CHANNEL_PRESSURE,
		PITCH_WHEEL_CHANGE,
		CHANNEL_MODE_MESSAGE,
		END_OF_TRACK,
		CHANGE_TEMPO
	}
	
	public enum FileFormat {
		SINGLE_TRACK,
		MULTIPLE_TRACKS_SYNC,
		MULTIPLE_TRACKS_ASYNC
	}

	public FileFormat getFileFormat() {
		return fileFormat;
	}

	public int getTrackCount() {
		return trackCount;
	}

	public int getTrackLength() {
		return trackLength;
	}

	public int getResolution() {
		return resolution;
	}

	public int getTempo() {
		return tempo;
	}
		
	public double getTickRate() { // ticks per ms
		return resolution / (tempo / 1000.0f);
	}

	public double getTickLength() { // ms of 1 tick
		return (tempo / 1000.0f) / resolution;
	}

	public double getTickLengthMicroSec() {
		return tempo / resolution;
	}
}
