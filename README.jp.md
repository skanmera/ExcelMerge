![](https://github.com/skanmera/ExcelMerge/blob/media/media/logo.png)

### エクセルのGUI差分ツール

![Demo](https://github.com/skanmera/ExcelMerge/blob/media/media/demo.gif)

![](https://github.com/skanmera/ExcelMerge/blob/media/media/cell_diff.png)

## 説明

ExcelMerge はエクセルやCSVファイルの差分をグラフィカルに表示するためのツールです。
現在は差分表示にしか対応していませんが、マージ機能を実装することを目標としています。

## システム要件

- Windows 7 以降

## 対象ファイル

- .xls
- .xlsx
- .csv

## インストール

[ここ](https://github.com/skanmera/ExcelMerge/releases/)から ExcelMergeSetup.msiをダウンロードして実行して下さい。

## 使い方

### ショートカットから起動

![](https://github.com/skanmera/ExcelMerge/blob/media/media/shortcut.png)

### エクスプローラーのコンテキストメニューから起動

![](https://github.com/skanmera/ExcelMerge/blob/media/media/context.png)

### コマンドラインから起動

```
ExcelMerge.GUI diff [オプション]
```

|オプション|説明|型|デフォルト値|
|------|-----------|----|-------|
|```-s``` ```--src-path```|左側のファイルパス|string|
|```-d``` ```--dst-path``` |右側のファイルパス| string
|```-c``` ```--external-cmd```|対象ファイル以外の差分を表示したいときに使用する外部コマンドを指定します| string
|```-i``` ```--immediately-execute-external-cmd```|エラーが発生したときにダイアログを表示せずに外部コマンドを実行します| bool | false
|```-w``` ```--wait-external-cmd```|外部コマンドの完了を待機します|bool|false
|```-v``` ```--validate-extension```|対象ファイルかどうかの検証を行います。falseの場合、対象外ファイルでもオープンを試みます|bool|false
|```-e``` ```--empty-file-name```|空のファイル名を指定します。|string
|```-k``` ```--keep-file-history```|最近使用したファイルに追加しません|bool|false

### Gitのdiffツールとして起動

.gitconfig
```
[diff]
tool = ExcelMerge

[difftool "ExcelMerge"]
cmd = \"C:/Program Files (x86)/ExcelMerge/ExcelMerge.GUI.exe\" diff -s \"$LOCAL\" -d \"$REMOTE\" -c WinMerge -i -w -v -k 

[alias]
windiff = difftool -g -y -t ExcelMerge
```

### Mercurialのdiffツールとして起動

mercurial.ini
```
[merge-tools]
excelmerge.executable = C:\Program Files (x86)\ExcelMerge\ExcelMerge.GUI.exe
excelmerge.diffargs = diff -s $parent1 -d $child -c WinMerge -i -w -v -e empty -k

[tortoisehg]
vdiff = excelmerge
```

## 外部コマンドの登録
コマンドラインのオプション"--external-cmd"で指定した名前の外部コマンドを追加します。

![](https://github.com/skanmera/ExcelMerge/blob/media/media/ext_cmd_win.png)

### 利用可能な変数
|変数|説明|
|------|----------|
|```${SRC}```|右側のファイルパス|
|```${DST}```|左側のファイルパス|  
  
  
コマンドラインからの指定だけでなく、ツール内からも外部コマンドを実行できます。

![](https://github.com/skanmera/ExcelMerge/blob/media/media/ext_cmd.png)

## 既知の問題点

- <h4>列の追加や削除がある場合に、その位置が期待している位置に表示されないことがある.</h4>
この問題は適切なヘッダを指定して差分を抽出しなおすことで解決することがあります。
具体的には以下の手順を行ってください。

1. 適切なヘッダのセルを選択する
2. 右クリックでコンテキストメニューを表示する
3. "この行をヘッダとして差分を抽出する"を選択


## ライセンス

#### MIT Licence

Copyright (c)2017 skanmera

以下に定める条件に従い、本ソフトウェアおよび関連文書のファイル（以下「ソフトウェア」）の複製を取得するすべての人に対し、ソフトウェアを無制限に扱うことを無償で許可します。これには、ソフトウェアの複製を使用、複写、変更、結合、掲載、頒布、サブライセンス、および/または販売する権利、およびソフトウェアを提供する相手に同じことを許可する権利も無制限に含まれます。

上記の著作権表示および本許諾表示を、ソフトウェアのすべての複製または重要な部分に記載するものとします。

ソフトウェアは「現状のまま」で、明示であるか暗黙であるかを問わず、何らの保証もなく提供されます。ここでいう保証とは、商品性、特定の目的への適合性、および権利非侵害についての保証も含みますが、それに限定されるものではありません。 作者または著作権者は、契約行為、不法行為、またはそれ以外であろうと、ソフトウェアに起因または関連し、あるいはソフトウェアの使用またはその他の扱いによって生じる一切の請求、損害、その他の義務について何らの責任も負わないものとします。
