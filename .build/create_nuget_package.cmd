set PATH=%PATH%;%WINDIR%\Microsoft.Net\Framework\v4.0.30319.

rem go to current folder
cd %~dp0

msbuild build_server_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~1"

msbuild build_server_web_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~2"

msbuild build_dot_net_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~3"

msbuild build_angular_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~4"

msbuild build_constraint_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver