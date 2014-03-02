using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Bricksoft.PowerCode;

namespace createpw
{
	class Program
	{
		static bool pause = false;
		static bool quiet = false;
		static bool verbose = false;

		static bool useCache = false;
		static bool cachePw = false;

		[STAThreadAttribute]
		static int Main( string[] args )
		{
			Config config = new Config();
			string site = "",
				password = "",
				username = "",
				cachefile = "";
			//bool allowalpha = true,
			//	allowdigits = true,
			//	allowsymbols = true;
			bool addsymbols = true;
			bool setclip = true;
			bool fl_config = false;
			int passlen = 26;

			config.read(true);
			if (config.contains("cachefile")) {
				cachefile = config.attr<string>("cachefile");
			} else {
				cachefile = "createpw.enc";
			}

			for (int i = 0; i < args.Length; i++) {
				string a = args[i];

				if (a.Length == 0) {
					continue;
				}

				if (a.StartsWith("/") || a.StartsWith("-") || a.StartsWith("!")) {
					while (a.StartsWith("/") || a.StartsWith("-")) {
						a = a.Substring(1);
					}

					string al = a.ToLowerInvariant();

					if (al == "?" || al.StartsWith("h")) {
						usage();
						return 0;

					} else if (al.Equals("pause") || al.Equals("!pause") || al.Equals("nopause") || al.Equals("no-pause")) {
						pause = al.Equals("pause");
					} else if (al.StartsWith("q") || al.StartsWith("!q") || al.StartsWith("noq") || al.StartsWith("no-q")) {
						quiet = al.StartsWith("q");
					} else if (al.StartsWith("v") || al.StartsWith("!v") || al.StartsWith("nov") || al.StartsWith("no-v")) {
						verbose = al.StartsWith("v");

					} else if (al.Equals("config")) {
						fl_config = true;

					} else if (al.StartsWith("cache") || al.StartsWith("!cache") || al.Equals("nocache") || al.Equals("no-cache")) {
						useCache = al.StartsWith("cache");

					} else if (al.StartsWith("p")) {
						int j = al.IndexOfAny(new char[] { ':', '=' });
						if (j > -1) {
							password = a.Substring(j + 1).Trim();
						} else {
							if (args.Length > i) {
								password = args[i + 1];
							} else {
								Console.WriteLine("Missing master password argument.");
							}
						}

					} else if (al.StartsWith("len") || al.StartsWith("passlen") || al.StartsWith("pass-len")) {
						int j = al.IndexOfAny(new char[] { ':', '=' });
						if (j > -1) {
							if (!int.TryParse(al.Substring(j + 1).Trim(), out passlen)) {
								Console.WriteLine("Could not parse the password length: " + args[i + 1]);
							}
						} else {
							if (args.Length > i) {
								if (!int.TryParse(args[i + 1], out passlen)) {
									Console.WriteLine("Could not parse the password length: " + args[i + 1]);
								}
							} else {
								Console.WriteLine("Missing new password length argument.");
							}
						}

					} else if (al == "clip" || al == "noclip" || al == "no-clip") {
						setclip = (al == "clip");

						//} else if (al.StartsWith("a") || al.StartsWith("!a") || al.StartsWith("noa") || al.StartsWith("no-a")) {
						//	allowalpha = al.StartsWith("a");
						//} else if (al.StartsWith("d") || al.StartsWith("!d") || al.StartsWith("nod") || al.StartsWith("no-d")) {
						//	allowdigits = al.StartsWith("d");
					} else if (al.StartsWith("s") || al.StartsWith("adds") || al.StartsWith("add-s")
							|| al.StartsWith("!s") || al.StartsWith("nos") || al.StartsWith("no-s")) {
						addsymbols = al.StartsWith("s") || al.StartsWith("adds") || al.StartsWith("add-s");

					} else if (al.StartsWith("l")) {
						//int pos;
						//if ((pos = a.IndexOf(":")) > -1 || (pos = a.IndexOf("=")) > -1) {
						//	if (!int.TryParse(a.Substring(pos), out passlen)) {
						//		Console.WriteLine("Error in length argument: " + a);
						//		return 2;
						//	}
						//} else {
						//	Console.WriteLine("Error in length argument: " + a);
						//	return 2;
						//}
					} else {
						if (!quiet) {
							Console.WriteLine("Unknown option: " + a);
						}
					}
				} else {
					if (site.Length == 0) {
						site = a;
					} else {
						if (!quiet) {
							Console.WriteLine("Unknown option: " + a);
						}
					}
				}
			}

			if (!quiet) {
				header();
			}

			if (site.Length == 0) {
				if (quiet) {
					Console.Write("**** ERROR: Cannot ask for user input (site) when --quiet is specified. exiting.");
					return 100;
				} else {
					Console.Write("Enter the site name or key: ");
					if (!ConsoleText.ReadLine(out site, left: Console.CursorLeft, maxlength: 40, echo: true) || site.Length == 0) {
						return 0;
					}
				}
			}
			//if (verbose) {
			//	Console.WriteLine("using '" + site + "'");
			//}

			if (username.Length == 0 && !quiet) {
				Console.Write("Enter the username:         ");
				if (!ConsoleText.ReadLine(out username, left: Console.CursorLeft, maxlength: 40, echo: true)) {
					return 0;
				}
			}
			//if (verbose) {
			//	if (username.Length == 0) {
			//		Console.WriteLine("no username");
			//	} else {
			//		Console.WriteLine("using '" + username + "'");
			//	}
			//}

			string tmp = "",
				hash = "",
				passhash = "",
				base64 = "",
				file_dthash = "",
				read_dthash = "";

			if (useCache) {
				// Use the last password that was used.
				if (File.Exists(cachefile)) {
					tmp = File.ReadAllText(cachefile);
					// line 1 is a hash of the file's last written date/time.
					// line 2 is the actual password hash.
					read_dthash = tmp.Substring(0, tmp.IndexOf('\n')).Trim();
					file_dthash = getSHA256(File.GetLastWriteTimeUtc(cachefile).ToString("YYYY-MM-DD:HH:NN:SS")).Trim();
					if (read_dthash == file_dthash) {
						passhash = tmp.Substring(tmp.IndexOf('\n')).Trim();
					} else {
						// Invalid signature on file!
						File.SetAttributes(cachefile, FileAttributes.Normal);
						File.Delete(cachefile);
						passhash = "";
					}
				}
			}

			if (passhash.Length == 0 && password.Length == 0) {
				if (quiet) {
					Console.Write("**** ERROR: Cannot ask for user input (master password) when --quiet is specified. exiting.");
					return 101;
				} else {
					if (useCache) {
						Console.WriteLine("The master password has not yet been cached.");
						cachePw = true;
					}
					Console.Write("Enter the master password:  ");
					if (!ConsoleText.ReadLine(out password, left: Console.CursorLeft, maxlength: 40, echo: true, passwd: true) || site.Length == 0) {
						return 51;
					}
				}
			}

			if (!quiet) {
				Console.WriteLine();
			}

			if (passhash.Length == 0) {
				passhash = getSHA256(password);
			}

			if (cachePw) {
				// Save the new password (as a hash) into the cachefile.
				DateTime n = DateTime.Now.ToUniversalTime();
				file_dthash = getSHA256(n.ToString("YYYY-MM-DD:HH:NN:SS"));
				File.WriteAllText(cachefile, file_dthash + "\n" + passhash);
				File.SetLastWriteTimeUtc(cachefile, n);
			}

			hash = getSHA256(passhash + ":" + site + ":" + username);
			if (verbose && !quiet) {
				Console.WriteLine("hash1    = " + hash);
			}

			SHA256Managed crypt = new SHA256Managed();
			byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(hash), 0, Encoding.UTF8.GetByteCount(hash));

			base64 = Convert.ToBase64String(crypto, Base64FormattingOptions.None);
			if (verbose && !quiet) {
				Console.WriteLine("base64   = " + base64);
			}

			if (addsymbols) {
				char[] ca = new char[] { '~', '!', '@', '#', '$', '%', '^', '&', '*', '(' };
				int index = 0;
				char[] ba = base64.ToCharArray();
				StringBuilder s = new StringBuilder();

				s.Append('#'); // always prepend a symbol..

				for (int i = 0; i < ba.Length; i++) {
					char c = ba[i];
					if (c == 'o' || c == '1' || c == 'l' || (!c.isalpha() && !c.isdigit())) {
						s.Append(ca[index++]);
						if (index > ca.Length - 1) {
							index = 0;
						}
					} else {
						s.Append(ba[i]);
					}
				}

				base64 = s.ToString();
			} else {
				base64 = base64.Replace("+", "E").Replace("/", "a");
			}

			if (base64.Length >= passlen) {
				base64 = base64.Substring(0, passlen);
			}

			if (setclip) {
				Clipboard.SetText(base64, TextDataFormat.Text);
			}

			if (!quiet) {
				Console.WriteLine();
				Console.WriteLine("password = " + base64);
				Console.WriteLine();
			}

			if (!quiet && pause) {
				Console.ReadKey(true);
			}

			return 0;
		}

		static void header()
		{
			Console.WriteLine(@"createpw.exe | Copyright (C) 2014 @wasatchwizard
");
		}

		static void usage()
		{
			header();
			Console.WriteLine(@"SYNOPSIS:

  A simple utility to create unique passwords from a key (such as a 
  website name) and a master password. The idea for the code came from 
  http://ss64.com/pass/.

USAGE: createpw.exe [options] [key|site-name]

  key|site-name      Any unique key that you will remember.
                     If you’re using this for a website, put in
                     the url (such as: amazon or amazon.com).
                     If omitted, you will be prompted for it.

OPTIONS:

  --pause
  --quiet
  --verbose

  --no-symbols       Do not output symbols in the new password.
                     By default, symbols (ie: $#@!, etc.) are output.

  --clip             Put the newly created password onto the clipboard.
                     This is the default behavior.
  --no-clip          Output the new password to the console.");
		}

		static string getSHA256( string password )
		{
			SHA512Managed crypt = new SHA512Managed();
			StringBuilder hash = new StringBuilder();
			byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
			foreach (byte bit in crypto) {
				hash.Append(bit.ToString("x2"));
			}
			return hash.ToString();
		}
	}
}
