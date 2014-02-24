using System;
using System.Text;
using System.Windows.Forms;

namespace createpw
{
	public static class ConsoleText
	{
		public static bool ReadLine( out string line, int left, int maxlength, bool echo = true, bool passwd = false, bool previewpw = true, bool allowspace = true, bool allowalpha = true, bool allowdigits = true, bool allowsymbols = true, bool newLineOnEnter = true )
		{
			bool cursorVisible, controlC, canceled;
			ConsoleKeyInfo key;
			StringBuilder s = new StringBuilder();
			int sx = 0;
			bool lastBeep = false;

			canceled = false;

			controlC = Console.TreatControlCAsInput;
			Console.TreatControlCAsInput = true;
			cursorVisible = Console.CursorVisible;
			Console.CursorVisible = true;

			while (true) {
				key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.Escape || (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)) {
					canceled = true;
					break;
				} else if (key.Key == ConsoleKey.Enter) {
					if (newLineOnEnter && echo) {
						Console.WriteLine();
					}
					break;
				} else if (key.Key == ConsoleKey.Backspace) {
					if (sx > 0) {
						sx--;
						s.Remove(sx, 1);
						writeln(left, left + sx, s.ToString(), passwd: passwd, previewpw: previewpw);
					} else {
						Console.Beep();
					}
					lastBeep = false;
				} else if (key.Key == ConsoleKey.Delete) {
					if (sx < s.Length && echo && !passwd) {
						s.Remove(sx, 1);
						writeln(left, left + sx, s.ToString(), passwd: passwd, previewpw: false);
					} else {
						Console.Beep();
					}
					lastBeep = false;
				} else if (key.Key == ConsoleKey.Home) {
					if (echo && !passwd) {
						sx = 0;
						Console.CursorLeft = left + sx;
					} else {
						Console.Beep();
					}
				} else if (key.Key == ConsoleKey.End) {
					if (echo && !passwd) {
						sx = s.Length;
						Console.CursorLeft = left + sx;
					} else {
						Console.Beep();
					}
				} else if (key.Key == ConsoleKey.LeftArrow) {
					if (sx > 0 && echo) {
						if (passwd) {
							// emulate backspace
							sx--;
							s.Remove(sx, 1);
							writeln(left, left + sx, s.ToString(), passwd: passwd, previewpw: false);
						} else {
							sx--;
							Console.CursorLeft = left + sx;
						}
						lastBeep = false;
					} else {
						Console.Beep();
					}

				} else if (key.Key == ConsoleKey.RightArrow) {
					if (sx < s.Length && echo && !passwd) {
						sx++;
						Console.CursorLeft = left + sx;
					} else {
						Console.Beep();
					}
				} else if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control && key.Key == ConsoleKey.V) {
					// paste the clipboard..
					// NOTE: This isn't actually getting called.
					if (Clipboard.ContainsText(TextDataFormat.Text)) {
						string cliptext = Clipboard.GetText(TextDataFormat.Text);
						if (sx + cliptext.Length > maxlength) {
							cliptext.Substring(0, maxlength - sx);
							Console.Beep();
							lastBeep = true;
						}
						sx += cliptext.Length;
						s.Append(cliptext);
						writeln(left, left + sx, s.ToString(), passwd: passwd, previewpw: previewpw);
					}
				} else if (key.KeyChar.isspace() || key.KeyChar.isalpha() || key.KeyChar.isdigit() || key.KeyChar.issymbol()) {
					if (s.Length < maxlength) {
						if ((allowspace && (key.KeyChar.isspace() || key.KeyChar.istab()))
								|| (allowalpha && key.KeyChar.isalpha())
								|| (allowdigits && key.KeyChar.isdigit())
								|| (allowsymbols && key.KeyChar.issymbol())) {
							if (echo) {
								if (passwd) {
									sx++;
									s.Append(key.KeyChar);
									writeln(left, left + sx, s.ToString(), passwd: passwd, previewpw: previewpw);
								} else {
									if (sx < s.Length) {
										// overwriting somewhere within string..
										string t = s.ToString();
										s.Clear();
										s.Append(t.Substring(0, sx));
										s.Append(key.KeyChar);
										s.Append(t.Substring(sx + 1));
									} else {
										// appending to string
										s.Append(key.KeyChar);
									}
									sx++;
									writeln(left, left + sx, s.ToString(), passwd: passwd, previewpw: previewpw);
								}
							} else {
								// no echo, so just append to s..
								s.Append(key.KeyChar);
							}
						} else {
							Console.Beep();
						}
					} else {
						if (!lastBeep) {
							Console.Beep();
							lastBeep = true;
						}
					}
				} else {
					//if ((key.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt
					//	|| (key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control) {
					// Ignore anything else..
					Console.Beep();
				}
			}

			if (passwd) {
				Console.CursorTop -= 1;
				writeln(left, left + sx, s.ToString(), passwd: passwd, previewpw: false);
				Console.WriteLine();
			}

			Console.TreatControlCAsInput = controlC;
			Console.CursorVisible = cursorVisible;

			line = s.ToString();

			if (canceled) {
				return false;
			} else {
				return true;
			}
		}

		static void writeln( int left, int insertion, string s, int padlen = 1, bool passwd = false, bool previewpw = false )
		{
			Console.CursorLeft = left;
			if (passwd) {
				if (s.Length > 0) {
					if (previewpw) {
						Console.Write(new string('*', s.Length - 1) + s[s.Length - 1]);
					} else {
						Console.Write(new string('*', s.Length));
					}
				}
			} else {
				Console.Write(s);
			}
			Console.Write(new string(' ', padlen));
			Console.CursorLeft = insertion;
		}
	}
}
