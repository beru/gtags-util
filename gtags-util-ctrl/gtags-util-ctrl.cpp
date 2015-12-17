#include "stdafx.h"
#include <shlwapi.h>

/*
����: [OPTION] COMMAND ARGS...
OPTION:
-e
COMMAND: [tag|refer|back|forward]
ARGS
tag current_filename current_line current_column keyword
refer current_filename current_line current_column keyword
back
forward
*/

using namespace std;

static HANDLE open_connection(void);
static void exec_util(wchar_t *dir);

#define WAIT_TIME	200						//�N���҂��̃E�F�C�g
#define WAIT_COUNT	50						//�N���҂��̃��g���C��
#define COMMAND_LINE_MAX_LEN	4096		//�R�}���h���C���ő啶����
#define READ_BUFFER_MAX_LEN		(32*1024)	//��M�o�b�t�@�ő啶����

int _tmain(int argc, _TCHAR* argv[])
{
	static wchar_t	command[COMMAND_LINE_MAX_LEN];		//�T�C�Y���傫���̂�static
	static wchar_t	result[READ_BUFFER_MAX_LEN];
	static wchar_t	*exe_file_name;
	HANDLE	hFile;
	DWORD	writesize;
	DWORD	readsize;
	wchar_t	*output_file = NULL;
	FILE	*output_fp = NULL;

	//�����`�F�b�N
	if (argc < 2) {
		return 0;
	}

	//���s�t�@�C�����A�h���X�ۑ�
	exe_file_name = argv[0];

	//�I�v�V�������
	argv++;
	argc--;
	while (argc > 0) {
		if (wcscmp(*argv, L"-o") == 0) {
			if (argc < 2) {
				return 0;
			}
			output_file = argv[1];
			argv += 2;
			argc -= 2;
		}
		else {
			break;
		}
	}

	//�R�}���h�����񐶐�
	if (wcscmp(argv[0], L"tag") == 0) {
		if (argc < 5) {
			return 0;
		}
		swprintf_s(command, sizeof(command) / sizeof(wchar_t),
			TEXT("tag\t%s\t%s\t%s\t%s\n"),
			argv[1],
			argv[2],
			argv[3],
			argv[4]
			);
	}
	else if (wcscmp(argv[0], L"tagsym") == 0) {
		if (argc < 5) {
			return 0;
		}
		swprintf_s(command, sizeof(command) / sizeof(wchar_t),
			TEXT("tagsym\t%s\t%s\t%s\t%s\n"),
			argv[1],
			argv[2],
			argv[3],
			argv[4]
			);
	}
	else if (wcscmp(argv[0], L"refer") == 0) {
		if (argc < 5) {
			return 0;
		}
		swprintf_s(command, sizeof(command) / sizeof(wchar_t),
			TEXT("refer\t%s\t%s\t%s\t%s\n"),
			argv[1],
			argv[2],
			argv[3],
			argv[4]
			);
	}
	else if (wcscmp(argv[0], L"symbol") == 0) {
		if (argc < 5) {
			return 0;
		}
		swprintf_s(command, sizeof(command) / sizeof(wchar_t),
			TEXT("symbol\t%s\t%s\t%s\t%s\n"),
			argv[1],
			argv[2],
			argv[3],
			argv[4]
			);
	}
	else if (wcscmp(argv[0], L"back") == 0) {
		swprintf_s(command, sizeof(command) / sizeof(wchar_t),
			TEXT("back\n")
			);
	}
	else if (wcscmp(argv[0], L"forward") == 0) {
		swprintf_s(command, sizeof(command) / sizeof(wchar_t),
			TEXT("forward\n")
			);
	}
	else if (wcscmp(argv[0], L"push") == 0) {
		if (argc < 7) {
			return 0;
		}
		swprintf_s(command, sizeof(command) / sizeof(wchar_t),
			TEXT("push\t%s\t%s\t%s\t%s\t%s\t%s\n"),
			argv[1],
			argv[2],
			argv[3],
			argv[4],
			argv[5],
			argv[6]
			);
	}
	
	//�o�̓t�@�C���I�[�v��
	if (output_file != NULL) {
		if (_wfopen_s(&output_fp, output_file, L"wt,ccs=UTF-16LE") != 0) {
			return 0;
		}
	}

	//�ʐM�p�C�v�I�[�v��
	hFile = open_connection();
	if (hFile == INVALID_HANDLE_VALUE) {
		static wchar_t dir[MAX_PATH];
		wcscpy_s(dir, MAX_PATH, exe_file_name);
		::PathRemoveFileSpec(dir);
		//�풓�v���O�����̎��s
		exec_util(dir);
		//���g���C
		for (int i = 0; i < WAIT_COUNT; i++) {
			hFile = open_connection();
			if (hFile != INVALID_HANDLE_VALUE) {
				break;
			}
			::Sleep(WAIT_TIME);
		}
		if (hFile == INVALID_HANDLE_VALUE) {
			return 0;
		}
	}

	//searchgtags�����E�C���h�E���t�H�A�O���E���h�ɂł���悤�ɂ���
	::AllowSetForegroundWindow(ASFW_ANY);

	//�R�}���h���M
	::WriteFile(hFile, command, sizeof(wchar_t) * (wcslen(command) + 1), &writesize, NULL);

	while (1) {
		//���ʎ�M
		readsize = 0;
		::ReadFile(hFile, result, sizeof(result) - sizeof(wchar_t), &readsize, NULL);
		if (readsize == 0) {
			break;
		}
		result[readsize / sizeof(wchar_t)] = 0;

		//���ʂ��o��
		if (output_fp != NULL) {
			//�t�@�C���o��
			fwprintf(output_fp, TEXT("%s"), result);
		}
		else {
			//�W���o��
			wprintf(TEXT("%s"), result);
		}
	}

	//�t�@�C���N���[�Y
	if (output_fp != NULL) {
		fclose(output_fp);
	}
	::CloseHandle(hFile);

	return 0;
}

static HANDLE open_connection(void)
{
	HANDLE hFile;

	hFile = ::CreateFile(
		TEXT("\\\\.\\pipe\\gtags-util-pipe"),
		GENERIC_WRITE | GENERIC_READ,
		0,
		NULL,
		OPEN_EXISTING,
		0,
		NULL);

	return hFile;
}

static void exec_util(wchar_t *dir)
{
	wchar_t exefile[MAX_PATH];

	_sntprintf_s(exefile, MAX_PATH, _TRUNCATE, TEXT("%s\\gtags-util.exe"), dir);
	::ShellExecute(NULL, NULL, exefile, NULL, dir, SW_SHOW);
}
