var sh = new ActiveXObject("WScript.Shell");
var fs = new ActiveXObject("Scripting.FileSystemObject");

var RUN_HIDE = 0;
var RUN_WAIT = true;

var editorfile = Editor.FullName;
//var scriptdir = editorfile.match(/.*\\/) + "\\Macro\\タグ";
var scriptdir = sh.SpecialFolders("Appdata") + "\\gPad\\Macro\\タグ";

var exe = scriptdir + "\\gtags-util-ctrl.exe";
var tmpfile = scriptdir + "\\global.tmp";
var listfile = scriptdir + "\\result_list.txt";

main();

//---End--

function main()
{
	var cmd = "back";
	var commandline;

	commandline = '"' + exe + '"' + " " + "-o " + '"' + tmpfile + '"' + " " + cmd;
	
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

