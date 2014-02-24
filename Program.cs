using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace createpw
{
	class Program
	{
		static bool pause = false;
		static bool quiet = false;
		static bool verbose = false;

		[STAThreadAttribute]
		static int Main( string[] args )
		{
			string site = "",
				password = "",
				username = "";
			//bool allowalpha = true,
			//	allowdigits = true,
			//	allowsymbols = true;
			bool addsymbols = true;
			bool setclip = true;
			int passlen = 26;

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

					} else if (al.StartsWith("p") || al.StartsWith("!p") || al.StartsWith("nop") || al.StartsWith("no-p")) {
						pause = al.StartsWith("p");
					} else if (al.StartsWith("q") || al.StartsWith("!q") || al.StartsWith("noq") || al.StartsWith("no-q")) {
						quiet = al.StartsWith("q");
					} else if (al.StartsWith("v") || al.StartsWith("!v") || al.StartsWith("nov") || al.StartsWith("no-v")) {
						verbose = al.StartsWith("v");

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

			if (site.Length == 0) {
				Console.Write("Enter the site name or key: ");
				if (!ConsoleText.ReadLine(out site, left: Console.CursorLeft, maxlength: 40, echo: true) || site.Length == 0) {
					return 0;
				}
			}
			//if (verbose) {
			//	Console.WriteLine("using '" + site + "'");
			//}

			if (username.Length == 0) {
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

			Console.Write("Enter the master password:  ");
			if (!ConsoleText.ReadLine(out password, left: Console.CursorLeft, maxlength: 40, echo: true, passwd: true) || site.Length == 0) {
				return 0;
			}
			//Console.WriteLine("using '" + password + "'");

			Console.WriteLine();

			// echo -n "$key:$1" | sha256sum | perl -ne "s/([0-9a-f]{2})/print chr hex \$1/gie" | base64 | tr +/ Ea | cut -b 1-20
			string hash, base64;

			hash = getSHA256(password + ":" + site + ":" + username);
			if (verbose) {
				Console.WriteLine("hash1    = " + hash);
			}

			SHA256Managed crypt = new SHA256Managed();
			//SHA512Managed crypt = new SHA512Managed();
			byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(hash), 0, Encoding.UTF8.GetByteCount(hash));

			base64 = Convert.ToBase64String(crypto, Base64FormattingOptions.None);
			if (verbose) {
				Console.WriteLine("base64   = " + base64);
			}

			if (addsymbols) {
				//base64 = base64.Replace("+", "$").Replace("/", "*");
				char[] ca = new char[] { '~', '!', '@', '#', '$', '%', '^', '&', '*', '(' };
				int index = 0;
				char[] ba = base64.ToCharArray();
				StringBuilder s = new StringBuilder();

				s.Append('#'); // always prepend a symbol..

				for (int i = 0; i < ba.Length; i++) {
					char c = ba[i];
					if (!c.isalpha() && !c.isdigit()) {
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

			base64 = base64.Substring(0, passlen);

			if (setclip) {
				Clipboard.SetText(base64, TextDataFormat.Text);
				Console.WriteLine("password = <put into clipboard>");
			} else {
				Console.WriteLine("password = " + base64);
			}

			if (pause) {
				Console.ReadKey(true);
			}

			return 0;
		}

		static void usage()
		{
			Console.WriteLine("createpw.exe <key|site-name>");
			Console.WriteLine();
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
