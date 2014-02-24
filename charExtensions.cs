using System;

namespace createpw
{
	public static class charExtensions
	{
		public static bool iswhitespace( this char c )
		{
			return c == ' '
				|| c == '\t'
				|| c == '\r'
				|| c == '\n';
		}

		public static bool isspace( this char c )
		{
			return c == ' ';
		}

		public static bool istab( this char c )
		{
			return c == '\t';
		}

		public static bool isnewline( this char c )
		{
			return c == '\r'
				|| c == '\n';
		}

		public static bool isalpha( this char c )
		{
			return (c >= 'a' && c <= 'z')
				|| (c >= 'A' && c <= 'Z');
		}

		public static bool isdigit( this char c, bool allowSymbols = false )
		{
			if (allowSymbols) {
				return (c >= '0' && c <= '9')
					|| (c == '.')
					|| (c == '+')
					|| (c == '-');
			} else {
				return (c >= '0' && c <= '9');
			}
		}

		public static bool issymbol( this char c )
		{
			return (c >= '!' && c <= '/')
				|| (c >= ':' && c <= '@')
				|| (c >= '[' && c <= '`')
				|| (c >= '{' && c <= '~');
		}

		public static bool isextra( this char c )
		{
			return (c >= 1 && c <= 6)
				|| (c == 11) || (c == 12)
				|| (c >= 14 && c <= 31)
				|| (c >= 127 && c <= 254);
		}
	}
}
