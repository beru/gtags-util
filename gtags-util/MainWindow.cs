using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;

namespace gtags_util
{
	public partial class MainWindow : Form
	{
		private List<gnu_global.result> _jumphistory;
		private int _jumphistory_pos;
		private NamedPipeServerStream _server;

		public MainWindow()
		{
			InitializeComponent();

			//通知アイコン変更
			this.notifyIcon.Icon = ((System.Drawing.Icon)(Properties.Resources.notify));
			//履歴初期化
			_jumphistory = new List<gnu_global.result>();
			_jumphistory_pos = 0;
			//通信用サーバ作成
			init_server();
		}

		//メニュー処理
		private void exitMenuItem_Click(object sender, EventArgs e)
		{
			//終了
			this.Close();
		}

		//通信用サーバー初期化
		private void init_server()
		{
			_server = new NamedPipeServerStream(
				"gtags-util-pipe",
				PipeDirection.InOut,
				1,
				PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			_server.BeginWaitForConnection(new AsyncCallback(connect_callback), this);
		}

		//通信接続コールバック
		private static void connect_callback(IAsyncResult ar)
		{
			((MainWindow)ar.AsyncState).server_connected(ar);
		}

		//コマンド受信処理
		private void server_connected(IAsyncResult ar)
		{
			if (ar.IsCompleted)
			{
				//接続完了待ち
				_server.EndWaitForConnection(ar);
				//コマンド読み込み
				StreamReader reader = new StreamReader(_server, Encoding.Unicode);
				string line = reader.ReadLine();
				//通知アイコン変更
				this.notifyIcon.Icon = ((System.Drawing.Icon)(Properties.Resources.notify_processing));
				//コマンド実行
				server_exec_cmd(line);
				//通知アイコン復元
				this.notifyIcon.Icon = ((System.Drawing.Icon)(Properties.Resources.notify));
				//切断
				_server.Disconnect();
				//再接続待ち
				_server.BeginWaitForConnection(new AsyncCallback(connect_callback), this);
			}
		}

		private void server_exec_cmd(string line)
		{
			//コマンド解析
			char[] separator = new char[] { '\t' };
			char[] trim = new char[] { '\n', '\r' };
			string[] arg;

			//末尾の改行削除
			line = line.TrimEnd(trim);

			arg = line.Split(separator);
			if (arg.Length == 0)
			{
				return;	//ERROR
			}

			List<gnu_global.result> result = null;

			if (arg[0].CompareTo("tag") == 0)
			{
				result = tagjump(arg[1], Convert.ToInt32(arg[2]), Convert.ToInt32(arg[3]), arg[4], gnu_global.MODE.TAG);
			}
			else if (arg[0].CompareTo("tagsym") == 0)
			{
				result = tagjump(arg[1], Convert.ToInt32(arg[2]), Convert.ToInt32(arg[3]), arg[4], gnu_global.MODE.TAG);
				if (result == null || result.Count == 0)
				{
					//タグが見つからなければシンボルも検索
					result = tagjump(arg[1], Convert.ToInt32(arg[2]), Convert.ToInt32(arg[3]), arg[4], gnu_global.MODE.SYMBOL);
				}
			}
			else if (arg[0].CompareTo("refer") == 0)
			{
				result = tagjump(arg[1], Convert.ToInt32(arg[2]), Convert.ToInt32(arg[3]), arg[4], gnu_global.MODE.REFER);
			}
			else if (arg[0].CompareTo("symbol") == 0)
			{
				result = tagjump(arg[1], Convert.ToInt32(arg[2]), Convert.ToInt32(arg[3]), arg[4], gnu_global.MODE.SYMBOL);
			}
			else if (arg[0].CompareTo("back") == 0)
			{
				result = tagjumpback();
			}
			else if (arg[0].CompareTo("forward") == 0)
			{
				result = tagjumpforward();
			}
			else if (arg[0].CompareTo("push") == 0)
			{
				addhistory(arg[1], Convert.ToInt32(arg[2]), Convert.ToInt32(arg[3]),
								arg[4], Convert.ToInt32(arg[5]), Convert.ToInt32(arg[6]));
				return;
			}
			if (result == null)
			{
				return; //NO RESULT
			}

			//結果文字列作成
			string result_str = "";
			bool output_description = false;
			if (result.Count() > 1)
			{
				result_str = "LIST\n";
				output_description = true;
			}
			foreach(gnu_global.result r in result)
			{
				result_str += r.m_file + "\t"
					+ Convert.ToString(r.m_line) + "\t"
					+ Convert.ToString(r.m_column);
				if (output_description)
				{
					result_str += "\t" + r.m_description;
				}
				result_str += "\n";
			}

			//結果送信
			byte[] message = Encoding.Unicode.GetBytes(result_str);
			_server.Write(message, 0, message.Length);
			_server.WaitForPipeDrain();
		}

		//タグジャンプ
		private List<gnu_global.result> tagjump(string cur_file, int cur_line, int cur_column, string keyword, gnu_global.MODE mode)
		{
			if (cur_file.Length == 0 || keyword.Length == 0)
			{
				return null;
			}

			string cur_dir = Path.GetDirectoryName(cur_file);
			List<gnu_global.result> result;

			//検索
			result = find_tags(cur_dir, cur_file, keyword, mode);
			if (result == null) {
					return null;
			}
			if (result.Count > 1)
			{
				return result;		//履歴に登録せずにリストを返す
			}
			//履歴の現在位置以降のデータを削除
			_jumphistory.RemoveRange(_jumphistory_pos, _jumphistory.Count - _jumphistory_pos);
			//ジャンプ元位置を保存
			_jumphistory.Add(new gnu_global.result(cur_file, cur_line, cur_column, ""));
			//ジャンプ先を保存
			_jumphistory.Add(result[0]);
			//履歴の現在位置更新
			_jumphistory_pos += 2;
				
			return result;
		}

		//ヒストリ追加
		private void addhistory(string cur_file, int cur_line, int cur_column, string next_file, int next_line, int next_column)
		{
			if (cur_file.Length == 0 || next_file.Length == 0)
			{
				return;
			}

			//履歴の現在位置以降のデータを削除
			_jumphistory.RemoveRange(_jumphistory_pos, _jumphistory.Count - _jumphistory_pos);

			//ジャンプ元位置を保存
			_jumphistory.Add(new gnu_global.result(cur_file, cur_line, cur_column, ""));
			//履歴の現在位置更新
			_jumphistory_pos++;
			//ジャンプ先位置を保存
			_jumphistory.Add(new gnu_global.result(next_file, next_line, next_column, ""));
			//履歴の現在位置更新
			_jumphistory_pos++;
		}

		//タグジャンプバック
		private List<gnu_global.result> tagjumpback()
		{
			List<gnu_global.result> resultlist = new List<gnu_global.result>();

			if (_jumphistory_pos < 2)
			{
				//もう戻れない
				return null;
			}
			_jumphistory_pos -= 2;	//ジャンプすると2つずつ追加されるので2減らす

			//タグジャンプしたときのジャンプ元位置を返す
			resultlist.Add(_jumphistory[_jumphistory_pos]);
			return resultlist;
		}

		//タグジャンプフォワード
		private List<gnu_global.result> tagjumpforward()
		{
			List<gnu_global.result> resultlist = new List<gnu_global.result>();

			if (_jumphistory_pos >= _jumphistory.Count)
			{
				//もう進めない
				return null;
			}

			//タグジャンプしたときのジャンプ先位置を返す
			resultlist.Add(_jumphistory[_jumphistory_pos + 1]);

			_jumphistory_pos += 2;

			return resultlist;
		}

		private List<gnu_global.result> find_tags(string dir, string file, string keyword, gnu_global.MODE mode)
		{
			List<gnu_global.result> findlist;
			List<gnu_global.result> resultlist = new List<gnu_global.result>();

			//global.exeのディレクトリ=自分自身のディレクトリ
			string bindir;
			bindir = Path.GetDirectoryName(Application.ExecutablePath);
			//タグ検索
			findlist = gnu_global.search(bindir, dir, keyword, mode);

			if (findlist.Count == 0)
			{
				//見つからない
				return null;
			}
			if (findlist.Count > 1)
			{
				//2カ所以上発見された場合は選択ウインドウ表示
				//ジャンプ元ファイル名に似ている順にソートする
				sort_result(findlist, file);
				//選択ウインドウ表示
				SelectForm	selector = new SelectForm();
				selector.SetList(findlist);
				if (selector.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
					//キャンセル
					return null;
				}
				//選択項目
				int select_index = selector.result_index;
				if (select_index != -1)
				{
					resultlist.Add(findlist[select_index]);
				}
				else
				{
					//全結果を返す
					resultlist = findlist;
				}
			} else {
				resultlist.Add(findlist[0]);
			}

			return resultlist;
		}

		private void sort_result(List<gnu_global.result> list, string file)
		{
			int i;
			for (i = 0; i < list.Count; i++)
			{
				//元のファイルと一致する文字数
				list[i].m_sortval1 = match_length(list[i].m_file, file);
				//元リストの順序
				list[i].m_sortval2 = i;
			}

			//一致する文字数が多い順に並び替える
			list.Sort((a, b) => (a.m_sortval1 != b.m_sortval1) ? b.m_sortval1 - a.m_sortval1 : a.m_sortval2 - b.m_sortval2);
		}

		private int match_length(string s1, string s2)
		{
			int l = 0;
			while (l < s1.Length && s1[l] == s2[l])
			{
				l++;
			}
			return l;
		}
	}
}
