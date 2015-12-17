using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace gtags_util
{
	public class gnu_global
	{
		public class result
		{
			public string m_file;
			public int m_line;
			public int m_column;
			public string m_description;
			public int m_sortval1;
			public int m_sortval2;

			public result() { }
			public result(string file, int line, int column, string description)
			{
				m_file = file;
				m_line = line;
				m_column = column;
				m_description = description;
				m_sortval1 = 0;
				m_sortval2 = 0;
			}
			public result(string file, string line, string column, string description)
			{
				m_file = file;
				m_line = Convert.ToInt32(line);
				m_column = Convert.ToInt32(column);
				m_description = description;
				m_sortval1 = 0;
				m_sortval2 = 0;
			}
		}

		public enum MODE
		{
			TAG,
			REFER,
			SYMBOL
		}
		public static List<gnu_global.result> search(string bindir, string dir, string keyword, MODE mode)
		{
			Process proc = new Process();
			String arg;
			List<gnu_global.result> reslist;

			arg = "--result grep -a --encode-path \":\" ";
			switch (mode) {
				case MODE.REFER:
					arg += "-r " + keyword;
				break;
				case MODE.SYMBOL:
					arg += "-s " + keyword;
					break;
				case MODE.TAG:
				default:
					arg += keyword;
					break;
			}

			proc.StartInfo.FileName = bindir + "/global.exe";
			proc.StartInfo.CreateNoWindow =true; //コンソールを開かない
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.Arguments = arg;
			proc.StartInfo.WorkingDirectory = dir;
			proc.Start();
			//出力読み込み
			BinaryReader bin_reader = new BinaryReader(proc.StandardOutput.BaseStream);
			List<byte> output = new List<byte>();
			byte[] rdata;
			do {
				rdata = bin_reader.ReadBytes(2048);
				if (rdata.Length == 0) {
					break;
				}
				output.AddRange(rdata);
			} while (true);
			//配列に変換
			byte[] output_bytes = output.ToArray();
			//文字コード判定
			Encoding encode = GetCode(output_bytes);
			if (encode == null)
			{
				encode = System.Text.Encoding.GetEncoding(932);
			}
			//文字列に変換
			string output_string = encode.GetString(output_bytes);
			//出力解析
			reslist =  parse_output(output_string);

			return reslist;
		}

		private static List<gnu_global.result> parse_output(string output)
		{
			StringReader sr = new StringReader(output);
			List<result> reslist = new List<result>();
			String line;
			char[] separator = new char[]{':'};
			string[] split;

			while ((line = sr.ReadLine()) != null) {
				split = line.Split(separator, 3);
				//--encode-pathで変換した%3aを戻す
				string filename;
				filename = split[0].Replace("%3a", ":");
				// '/'を'\\'に置き換える
				filename = filename.Replace('/', '\\');
				reslist.Add(new result(filename, Convert.ToInt32(split[1]), 1, split[2]));
			}

			return reslist;
		}

		/// http://dobon.net/vb/dotnet/string/detectcode.html
		/// <summary>
		/// 文字コードを判別する
		/// </summary>
		/// <remarks>
		/// Jcode.pmのgetcodeメソッドを移植したものです。
		/// Jcode.pm(http://openlab.ring.gr.jp/Jcode/index-j.html)
		/// Jcode.pmのCopyright: Copyright 1999-2005 Dan Kogai
		/// </remarks>
		/// <param name="bytes">文字コードを調べるデータ</param>
		/// <returns>適当と思われるEncodingオブジェクト。
		/// 判断できなかった時はnull。</returns>
		private static System.Text.Encoding GetCode(byte[] bytes)
		{
			const byte bEscape = 0x1B;
			const byte bAt = 0x40;
			const byte bDollar = 0x24;
			const byte bAnd = 0x26;
			const byte bOpen = 0x28;    //'('
			const byte bB = 0x42;
			const byte bD = 0x44;
			const byte bJ = 0x4A;
			const byte bI = 0x49;

			int len = bytes.Length;
			byte b1, b2, b3, b4;

			//Encode::is_utf8 は無視

			bool isBinary = false;
			for (int i = 0; i < len; i++)
			{
				b1 = bytes[i];
				if (b1 <= 0x06 || b1 == 0x7F || b1 == 0xFF)
				{
					//'binary'
					isBinary = true;
					if (b1 == 0x00 && i < len - 1 && bytes[i + 1] <= 0x7F)
					{
						//smells like raw unicode
						return System.Text.Encoding.Unicode;
					}
				}
			}
			if (isBinary)
			{
				return null;
			}

			//not Japanese
			bool notJapanese = true;
			for (int i = 0; i < len; i++)
			{
				b1 = bytes[i];
				if (b1 == bEscape || 0x80 <= b1)
				{
					notJapanese = false;
					break;
				}
			}
			if (notJapanese)
			{
				return System.Text.Encoding.ASCII;
			}

			for (int i = 0; i < len - 2; i++)
			{
				b1 = bytes[i];
				b2 = bytes[i + 1];
				b3 = bytes[i + 2];

				if (b1 == bEscape)
				{
					if (b2 == bDollar && b3 == bAt)
					{
						//JIS_0208 1978
						//JIS
						return System.Text.Encoding.GetEncoding(50220);
					}
					else if (b2 == bDollar && b3 == bB)
					{
						//JIS_0208 1983
						//JIS
						return System.Text.Encoding.GetEncoding(50220);
					}
					else if (b2 == bOpen && (b3 == bB || b3 == bJ))
					{
						//JIS_ASC
						//JIS
						return System.Text.Encoding.GetEncoding(50220);
					}
					else if (b2 == bOpen && b3 == bI)
					{
						//JIS_KANA
						//JIS
						return System.Text.Encoding.GetEncoding(50220);
					}
					if (i < len - 3)
					{
						b4 = bytes[i + 3];
						if (b2 == bDollar && b3 == bOpen && b4 == bD)
						{
							//JIS_0212
							//JIS
							return System.Text.Encoding.GetEncoding(50220);
						}
						if (i < len - 5 &&
							b2 == bAnd && b3 == bAt && b4 == bEscape &&
							bytes[i + 4] == bDollar && bytes[i + 5] == bB)
						{
							//JIS_0208 1990
							//JIS
							return System.Text.Encoding.GetEncoding(50220);
						}
					}
				}
			}

			//should be euc|sjis|utf8
			//use of (?:) by Hiroki Ohzaki <ohzaki@iod.ricoh.co.jp>
			int sjis = 0;
			int euc = 0;
			int utf8 = 0;
			for (int i = 0; i < len - 1; i++)
			{
				b1 = bytes[i];
				b2 = bytes[i + 1];

				//2015/4/23 EUCの文字列"★A"が判定できないため、SJIS不正バイトの場合は判定を中断する。
				if ((0x9F < b1 && b1 < 0xE0) || (0xFC < b1)) {
					break;
				}

				if (((0x81 <= b1 && b1 <= 0x9F) || (0xE0 <= b1 && b1 <= 0xFC)) &&
					((0x40 <= b2 && b2 <= 0x7E) || (0x80 <= b2 && b2 <= 0xFC)))
				{
					//SJIS_C
					sjis += 2;
					i++;
				}
			}
			for (int i = 0; i < len - 1; i++)
			{
				b1 = bytes[i];
				b2 = bytes[i + 1];
				if (((0xA1 <= b1 && b1 <= 0xFE) && (0xA1 <= b2 && b2 <= 0xFE)) ||
					(b1 == 0x8E && (0xA1 <= b2 && b2 <= 0xDF)))
				{
					//EUC_C
					//EUC_KANA
					euc += 2;
					i++;
				}
				else if (i < len - 2)
				{
					b3 = bytes[i + 2];
					if (b1 == 0x8F && (0xA1 <= b2 && b2 <= 0xFE) &&
						(0xA1 <= b3 && b3 <= 0xFE))
					{
						//EUC_0212
						euc += 3;
						i += 2;
					}
				}
			}
			for (int i = 0; i < len - 1; i++)
			{
				b1 = bytes[i];
				b2 = bytes[i + 1];
				if ((0xC0 <= b1 && b1 <= 0xDF) && (0x80 <= b2 && b2 <= 0xBF))
				{
					//UTF8
					utf8 += 2;
					i++;
				}
				else if (i < len - 2)
				{
					b3 = bytes[i + 2];
					if ((0xE0 <= b1 && b1 <= 0xEF) && (0x80 <= b2 && b2 <= 0xBF) &&
						(0x80 <= b3 && b3 <= 0xBF))
					{
						//UTF8
						utf8 += 3;
						i += 2;
					}
				}
			}
			//M. Takahashi's suggestion
			//utf8 += utf8 / 2;

			System.Diagnostics.Debug.WriteLine(
				string.Format("sjis = {0}, euc = {1}, utf8 = {2}", sjis, euc, utf8));
			if (euc > sjis && euc > utf8)
			{
				//EUC
				return System.Text.Encoding.GetEncoding(51932);
			}
			else if (sjis > euc && sjis > utf8)
			{
				//SJIS
				return System.Text.Encoding.GetEncoding(932);
			}
			else if (utf8 > euc && utf8 > sjis)
			{
				//UTF8
				return System.Text.Encoding.UTF8;
			}

			return null;
		}
	}
}
