@echo off
set module_dir=%~dp0

set target_dir=%module_dir%%~1

set copy_source_files_data=%module_dir%\module_copy_sources.txt
set search_folders_data=%module_dir%\module_copy_search_folders.txt
rem echo %~2

for /f "delims=" %%a in (%copy_source_files_data%) do (
	call :do_copy_file %%a %~2\ 
)
goto :EOF

:do_copy_file
set elm=%1
for /f "delims=" %%b in (%search_folders_data%) do (
	if exist %%b%elm% (
		echo will copy %%b%elm% to %target_dir%
		copy /Y %%b%elm% /B %target_dir% /B
		goto :EOF
	)
	
	if exist %%b%~2%elm% (
		echo will copy %%b%~2%elm% to %target_dir%
		copy /Y %%b%~2%elm% /B %target_dir% /B
		goto :EOF
	)
)
goto :EOF


@echo on

