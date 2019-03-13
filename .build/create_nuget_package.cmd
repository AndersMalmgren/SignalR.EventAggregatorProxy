set PATH=%PATH%;%WINDIR%\Microsoft.Net\Framework\v4.0.30319.

rem go to current folder
cd %~dp0

dotnet msbuild build_server_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~1"

dotnet msbuild build_server_web_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~2"

dotnet msbuild build_dot_net_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~3"

msbuild build_angular_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~4"

dotnet msbuild build_constraint_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver


msbuild build_js_client.proj /property:NugetFolder=..\NugetOutput 

msbuild build_vue_client.proj /property:NugetFolder=..\NugetOutput 

dotnet msbuild build_dotnet_cli.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~5"

msbuild build_react_client.proj /property:NugetFolder=..\NugetOutput 

