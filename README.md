![](https://github.com/skanmera/ExcelMerge/blob/media/media/logo.png)

### GUI Diff Tool for Excel

![Demo](https://github.com/skanmera/ExcelMerge/blob/media/media/demo.gif)

![](https://github.com/skanmera/ExcelMerge/blob/media/media/cell_diff.png)

## Language

- [English](https://github.com/skanmera/ExcelMerge/blob/master/README.md)
- [日本語](https://github.com/skanmera/ExcelMerge/blob/master/README.jp.md)

## Description

ExcelMerge is a graphical display tool for Excel or CSV Diff.
The current feature is limited only to the display of Diff, but the goal is to implement the merge feature.
It can also be used as a diff tool for Git or Mercurial.

## System Requirements

- Windows 7 or later

## Supported files

- .xls
- .xlsx
- .csv

## Installation

Download ExcelMergeSetup.msi from [here](https://github.com/skanmera/ExcelMerge/releases/) and Run.

## Usage

### From shortcut

![](https://github.com/skanmera/ExcelMerge/blob/media/media/shortcut.png)

### From exproler context menu

![](https://github.com/skanmera/ExcelMerge/blob/media/media/context.png)

### From command line

```
ExcelMerge.GUI diff [Options]
```

|Option|Description|Type|Default|
|------|-----------|----|-------|
|```-s``` ```--src-path```|Source file path.|string|
|```-d``` ```--dst-path``` |Dest file path.| string
|```-c``` ```--external-cmd```|It is used to activate other tools for unsupported file types and occured any exception.| string
|```-i``` ```--immediately-execute-external-cmd```|Execute external cmd without error dialog.| bool | false
|```-w``` ```--wait-external-cmd```|Wait for the external process to finish.|bool|false
|```-v``` ```--validate-extension```|Validate extension before open file.|bool|false
|```-e``` ```--empty-file-name```|Empty file name.|string
|```-k``` ```--keep-file-history```|Don't add recent files.|bool|false

### From Git diff tool

.gitconfig
```
[diff]
tool = ExcelMerge

[difftool "ExcelMerge"]
cmd = \"C:/Program Files (x86)/ExcelMerge/ExcelMerge.GUI.exe\" diff -s \"$LOCAL\" -d \"$REMOTE\" -c WinMerge -i -w -v -k 

[alias]
windiff = difftool -g -y -t ExcelMerge
```

### From Mercurial diff tool

mercurial.ini
```
[merge-tools]
excelmerge.executable = C:\Program Files (x86)\ExcelMerge\ExcelMerge.GUI.exe
excelmerge.diffargs = diff -s $parent1 -d $child -c WinMerge -i -w -v -e empty -k

[tortoisehg]
vdiff = excelmerge
```

## Register External Command
Register the external command specified by the command line argument --external-cmd.

![](https://github.com/skanmera/ExcelMerge/blob/media/media/ext_cmd_win.png)

### Available Variables
|Value|Description|
|------|----------|
|```${SRC}```|Source file path|
|```${DST}```|Dest file path|  
  
  
Can also be executed from within the tool.

![](https://github.com/skanmera/ExcelMerge/blob/media/media/ext_cmd.png)

## File Settings

For each file you can specify a line header or a column header.

![](https://github.com/skanmera/ExcelMerge/blob/media/media/file_settings.png)

## Color Settings

You can customize background colors.

![](https://github.com/skanmera/ExcelMerge/blob/media/media/settings.png)


## Known problems

- <h4>If there are column deletions or additions, they may not be displayed at the expected position.</h4>
If the currently displayed header is not what you expect, you may resolve it by specifying the appropriate header and extract diff.
Follow these steps.
1. Select appropriate header cell.
2. Right click to display the context menu.
3. Select "Extract diff with this row as header"


## LICENSE

#### MIT Licence

Copyright (c)2017 skanmera

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
