using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gtags_util
{
	public partial class SelectForm : Form
	{
		public int result_index;

		public SelectForm()
		{
			InitializeComponent();
		}

		public void SetList(List<gnu_global.result> list)
		{
			//ディレクトリ部分の表示を省略するため先頭からの共通文字数を調べる
			string str = list[0].m_file;
			int match = str.Length;
			for (int i = 0; i < list.Count; i++)	//フォルダ区切りを見つける必要があるので最初の文字列から比較
			{
				int j;
				int separator_pos;

				separator_pos = 0;
				for (j = 0; j < match; j++)
				{
					if (str[j] != list[i].m_file[j])
					{
						break;
					}
					if (str[j] == '\\')
					{
						separator_pos = j;
					}

				}
				match = separator_pos + 1;
			}
			foreach (gnu_global.result item in list)
			{
				ListViewItem listitem = new ListViewItem();
				listitem.Text = item.m_file.Substring(match);	//一致する部分を削除
				listitem.SubItems.Add(Convert.ToString(item.m_line));
				listitem.SubItems.Add(item.m_description.Replace('\t', ' '));
				this.listView.Items.Add(listitem);

				this.listView.Items[0].Focused = true;
				this.listView.Items[0].Selected = true;
			}
			//幅調整
			this.listView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (this.listView.SelectedItems.Count >= 1)
			{
				this.result_index = this.listView.SelectedItems[0].Index;
			}
			else
			{
				this.result_index = 0;
			}
			//フォームを閉じる
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private void buttonList_Click(object sender, EventArgs e)
		{
			//リスト出力の場合は-1
			this.result_index = -1;
			//フォームを閉じる
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private void listView_DoubleClick(object sender, EventArgs e)
		{
			if (this.listView.SelectedItems.Count >= 1)
			{
				this.result_index = this.listView.SelectedItems[0].Index;
			}
			else
			{
				this.result_index = 0;
			}
			//フォームを閉じる
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private void SelectForm_Load(object sender, EventArgs e)
		{
			//フォームを前面に出す
			this.Activate();
			//リストビューにフォーカスを当てる
			this.ActiveControl = this.listView;
		}

	}
}
