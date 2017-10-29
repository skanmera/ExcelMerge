@echo off

for /f "tokens=3 delims= " %%a in ('chcp') do set lang=%%a

if "%lang%" == "932" ( 
    start "" https://github.com/skanmera/ExcelMerge/blob/master/README.jp.md#readme ) else ( 
    start "" https://github.com/skanmera/ExcelMerge/blob/master/README.md#readme )
