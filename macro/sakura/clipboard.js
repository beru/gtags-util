var sh = new ActiveXObject("WScript.Shell");
var fs = new ActiveXObject("Scripting.FileSystemObject");

var RUN_HIDE = 0;
var RUN_WAIT = true;

var editorfile = Editor.ExpandParameter("$S");
var scriptfile = Editor.ExpandParameter("$M");
var scriptdir = scriptfile.match(/.*\\/);

var exe = scriptdir + "\\gtags-util-ctrl.exe";
var tmpfile = scriptdir + "\\global.tmp";
var listfile = scriptdir + "\\result_list.txt";

var file = Editor.ExpandParameter("$F");
var line = Editor.ExpandParameter("$y");
var column = Editor.ExpandParameter("$x");

main();

//---End--

function main()
{
	var keyword = Editor.GetClipboard(0);

	//シェル実行に不具合が生じる文字を空白に置き換える
	keyword = keyword.replace(/["'\\\r\n]/g, " ");

	var cmd = "refer";
	var commandline;
	
	commandline = '"' + exe + '"' + " " + "-o " + '"' + tmpfile + '"' + " " + cmd + " " + '"' + file + '"' + " "
					 + line + " " + column + " " + '"' + keyword + '"';
	
	//Editor.InsText(commandline);
	
	sh.Run(commandline, RUN_HIDE, RUN_WAIT);
	
	try {
		var resultfile = fs.OpenTextFile(tmpfile, 1, false, -1);
		var text = resultfile.ReadLine();
		
		//Editor.InsText(text);
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
	Editor.FileOpen(filename);
}

function open_file_with_pos(filename, line, column)
{
	sh.Run('"' + editorfile + '"' + " -X=" + column + " -Y=" + line + " " + '"' + filename + '"');
}

