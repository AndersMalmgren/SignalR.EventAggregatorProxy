set PATH=%PATH%;%WINDIR%\Microsoft.Net\Framework\v4.0.30319.

rem go to current folder
cd %~dp0

rem msbuild build_server_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~1"

rem msbuild build_dot_net_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~2"

msbuild build_angular_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~3"

rem msbuild build_constraint_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver