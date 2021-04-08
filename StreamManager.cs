using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class StreamManager {

	private FileStream fileStream;

	private long bytesRemaining;

	public StreamManager(string path) {
		fileStream = new FileStream (path, FileMode.Open, FileAccess.Read);
		bytesRemaining = fileStream.Length;
	}

	public bool containsMore() {
		return bytesRemaining != 0;
	}

	public long getBytesRemaining() {
		return bytesRemaining;
	}

	public byte ReadByte() {
		bytesRemaining--;
		return (byte) fileStream.ReadByte ();
	}

	public int ReadInt() {
		int read = 0;
		for (int i = 0; i < 4; i++) {
			read <<= 8;
			read |= ReadByte ();
		}
		return read;
	}

	public int Read24BitInt() {
		int read = 0;
		for (int i = 0; i < 3; i++) {
			read <<= 8;
			read |= ReadByte ();
		}
		return read;
	}

	public short ReadShort() {
		short read = 0;
		for (int i = 0; i < 2; i++) {
			read <<= 8;
			read |= ReadByte ();
		}
		return read;
	}

	public char ReadChar() {
		return Convert.ToChar (ReadByte ());
	}

	public string ReadChars(int len) {
		string read = "";
		for (int i = 0; i < len; i++) {
			read += ReadChar ();
		}
		return read;
	}

	public int ReadVariableLengthInt() {
		int read = 0;
		while (true) {
			byte next = ReadByte ();
			read <<= 7;
			read |= next & 0x7F;
			if ((next & (1 << 7)) == 0) {
				return read;
			}
		}
	}

	public FileStream getFileStream() {
		return fileStream;
	}
}
