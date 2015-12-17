======================================================
=  GTAGS Utility for Windows Text Editor Ver 1.03    =
======================================================

■概要
　WindowsのテキストエディタからGNU GLOBALを使用してタグファイルを検索するツールです。
　サクラエディタ(http://sakura-editor.sourceforge.net/)とgPad(http://mfactory.me/)に対応しています。
　タグファイルの作成機能はありません。

■本ツール以外に必要なソフト
　GNU GLOBAL Ver 6.3.3以降
　　6.3.2以前のバージョンは動作未確認です。
　　また古いバージョンで作成したタグファイルは使用できない場合がありますのでVer6.3.3以降で再作成してください。

　　GNU GLOBAL source code tagging system
　　http://www.gnu.org/software/global/global.html
　　Getting GLOBAL
　　http://www.gnu.org/software/global/download.html

■インストール方法
□サクラエディタの場合
　１．実行ファイルのコピー
　　エディタのマクロフォルダに本ツールの実行ファイルをコピーする。
　　マクロフォルダは「設定」→「共通設定」→「マクロ」で確認できる。
　　
　　コピーするファイル
　　gtags-util.exe
　　gtags-util-ctrl.exe

　２．マクロファイルのコピー
　　エディタのマクロフォルダにサクラエディタ用マクロファイルをコピーする。
　　
　　コピーするファイル
　　macro\sakura\内の*.jsファイル
　　　tagjump.js
　　　tagjumpback.js
　　　tagjumpforward.js
　　　refer.js
　　　symbol.js
　　　clipboard.js

　３．GNU GLOBALのコピー
　　Windows用のGNU GLOBALをエディタのマクロフォルダにコピーする。
　　GNU GLOBALは別途ダウンロードしてください。
　　
　　コピーするファイル
　　global.exe
　　gtags.conf

　※上記手順後は以下のようになります。
　　\マクロフォルダ
　　　gtags-util.exe
　　　gtags-util-ctrl.exe
　　　global.exe
　　　gtags.conf
　　　tagjump.js
　　　tagjumpback.js
　　　tagjumpforward.js
　　　refer.js
　　　symbol.js
　　　clipboard.js

　４．マクロをキーに割り当てる
　　「設定」→「共通設定」→「キー割り当て」で自由に割り当てる。

　　tagjump.js          カーソル位置の単語の定義位置に移動
　　refer.js            カーソル位置の単語の参照位置に移動
　　symbol.js           カーソル位置のシンボルの定義位置に移動
　　clipboard.js        クリップボード内の文字列の定義位置に移動
　　tagjumpback.js      移動履歴の前の位置の戻る
　　tagjumpforward.js   移動履歴の次の位置に進む
　　

□gPadの場合
　１．実行ファイルのコピー
　　エディタのマクロフォルダに本ツールの実行ファイルをコピーする。
　　マクロフォルダは「ツール」→「マクロフォルダを開く」で確認できる。
　　
　　マクロフォルダ内に本ツール用のフォルダを作成してもよい。
　　このドキュメントでは「タグジャンプ」フォルダを作成したとして説明する。
　　
　　コピーするファイル
　　gtags-util.exe
　　gtags-util-ctrl.exe

　２．マクロファイルのコピー
　　エディタのマクロフォルダにサクラエディタ用マクロファイルをコピーする。
　　
　　コピーするファイル
　　macro\gPad\内の*.jsファイル
　　　tagjump.js
　　　tagjumpback.js
　　　tagjumpforward.js
　　　refer.js
　　　symbol.js
　　　clipboard.js

　３．GNU GLOBALのコピー
　　Windows用のGNU GLOBALをエディタのマクロフォルダにコピーする。
　　GNU GLOBALは別途ダウンロードしてください。
　　
　　コピーするファイル
　　global.exe
　　gtags.conf

　※上記手順後は以下のようになります。
　　\マクロフォルダ
　　　\タグジャンプ
　　　　gtags-util.exe
　　　　gtags-util-ctrl.exe
　　　　global.exe
　　　　gtags.conf
　　　　tagjump.js
　　　　tagjumpback.js
　　　　tagjumpforward.js
　　　　refer.js
　　　　symbol.js
　　　　clipboard.js

　４．マクロをキーに割り当てる
　　「ツール」→「オプション」→「キーボード」→「分類」「マクロ」で自由に割り当てる。

　　tagjump.js          カーソル位置の単語の定義位置に移動
　　refer.js            カーソル位置の単語の参照位置に移動
　　symbol.js           カーソル位置のシンボルの定義位置に移動
　　clipboard.js        クリップボード内の文字列の定義位置に移動
　　tagjumpback.js      移動履歴の前の位置の戻る
　　tagjumpforward.js   移動履歴の次の位置に進む
　　
■使用方法
　キーボードに割り当てたマクロを実行する。
　初回起動時に検索ツールが起動して常駐します。
　※検索ツールの起動のため初回の実行は時間がかかります。
　
　常駐ツールを終了する場合は、タスクトレイのアイコンを右クリックして終了を選択してください。

■アンインストール方法
　コピーしたファイルを削除してください。
　レジストリは使用しません。

