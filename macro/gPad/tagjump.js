var sh = new ActiveXObject("WScript.Shell");
var fs = new ActiveXObject("Scripting.FileSystemObject");

var RUN_HIDE = 0;
var RUN_WAIT = true;

var editorfile = Editor.FullName;
//var scriptdir = editorfile.match(/.*\\/) + "\\Macro\\�^�O";
var scriptdir = sh.SpecialFolders("Appdata") + "\\gPad\\Macro\\�^�O";

var exe = scriptdir + "\\gtags-util-ctrl.exe";
var tmpfile = scriptdir + "\\global.tmp";
var listfile = scriptdir + "\\result_list.txt";

var file = Document.FullName;
var filedir = Document.Path;
var line = Selection.GetActivePointY();
var column = Selection.GetActivePointX();

main();

//---End--

function main()
{
	var keyword;
	if (Selection.IsEmpty) {
		//���ݍs���^�O���u�t�@�C����(�s�ԍ�)�v�Ȃ�^�O���ňړ�
		if (tag_line_jump(Document.GetLine(line))) {
			return;
		}
		Selection.SelectWord();
		keyword = Selection.Text;
		Selection.Collapse();
		Selection.SetActivePoint(ePosLogical, column, line, false);
	} else {
		keyword = Selection.Text;
	}
	
	//�V�F�����s�ɕs��������镶�����󔒂ɒu��������
	keyword = keyword.replace(/["'\\\r\n]/g, " ");

	var cmd = "tagsym";
	var commandline;
	
	commandline = '"' + exe + '"' + " " + "-o " + '"' + tmpfile + '"' + " " + cmd + " " + '"' + file + '"' + " "
					 + line + " " + column + " " + '"' + keyword + '"';
	
	//alert(commandline);
	
	sh.Run(commandline, RUN_HIDE, RUN_WAIT);
	
	try {
		var resultfile = fs.OpenTextFile(tmpfile, 1, false, -1);
		var text = resultfile.ReadLine();
		
		//alert(text);
		if (text == "LIST") {
			make_list(resultfile);
			open_file(listfile);
		} else {
			var result = text.split('\t');
			open_file_with_pos(result[0], result[1], result[2]);
		}
	}
	catch (e) {
		//Nothing
	}
	
	
	try {
		if (resultfile != null) {
			resultfile.Close();
		}
		fs.DeleteFile(tmpfile, false);
	}
	catch (e) {
	}
}

function tag_line_jump(line_string)
{
	result = parse_tag_line(line_string);
	if (result == null) {
		return false;
	}
	var jump_file = result[0];
	var jump_line = result[1];
	var jump_column = "1";

	//�����̕ۑ�
	var cmd = "push";
	var commandline;
	commandline = '"' + exe + '"' + " " + "-o " + '"' + tmpfile + '"' + " " + cmd + " " + '"' + file + '"' + " "
					 + line + " " + column + " " + '"' + jump_file + '"' + " " + jump_line + " " + jump_column;

	//alert(commandline);

	sh.Run(commandline, RUN_HIDE, RUN_WAIT);

	//�t�@�C���I�[�v��
	open_file_with_pos(jump_file, jump_line, jump_column);

	return true;
}

function parse_tag_line(line)
{
	var match1;
	var match2;
	var line2
	var filename;
	
	//�s�擪����t�@�C�����ƍl�����镶����𔲂��o��
	match1 = line.match(/^([a-zA-Z]:)?[^?:*"><|]*/);
	if (match1 == null) {
		return null;
	}
	//�O��̋󔒍폜
	line2 = trim(match1[0])
	// (�s�ԍ�)�����Ă���Ȃ番������
	match2 = line2.match(/(.*)\((\d+)\)$/);
	if (match2 != null) {
		filename = match2[1];
		fileline = match2[2];
	} else {
		filename = line2;
		fileline = "1";
	}
	//�t�@�C���̑��݊m�F
	if (!fs.FileExists(filename)) {
		return null;
	}
	
	return [ filename, fileline ];
}

function trim(str)
{
	return str.replace(/^\s+|\s+$/g, "");
}

function make_list(resultfilehandle)
{
	var listfilehandle = fs.OpenTextFile(listfile, 2, true, -1);
	
	while (!resultfilehandle.AtEndOfStream) {
		var text = resultfilehandle.ReadLine();
		var result = text.split('\t');
		var description = "";
		var i;
		for (i = 3; i < result.length; i++) {
			description += "\t" + result[i];
		}
		listfilehandle.WriteLine(result[0] + "(" + result[1] + ")" + "\t\t:" + description);
	}
	
	listfilehandle.Close();
}

function open_file(filename)
{
	Editor.OpenFile(filename);
}

function open_file_with_pos(filename, line, column)
{
	Editor.OpenFile(filename);
	retry_count = 10;
	while (Editor.ActiveDocument.FullName != filename) {
		Sleep(100);
		retry_count--;
		if (retry_count == 0) {
			throw "Open Error";
		}
	}
	var jump_line = parseInt(line, 10);
	var jump_column = parseInt(column, 10);
	retry_count = 10;
	do {
		Editor.ActiveDocument.Selection.SetActivePoint(ePosLogical, jump_column, jump_line, false);
		if (Editor.ActiveDocument.Selection.GetActivePointY() == jump_line && Editor.ActiveDocument.Selection.GetActivePointX() == jump_column) {
			break;
		}
		Sleep(100);
		retry_count--;
	} while (retry_count > 0);
}

