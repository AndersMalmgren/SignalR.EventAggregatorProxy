set PATH=%PATH%;%WINDIR%\Microsoft.Net\Framework\v4.0.30319.

rem go to current folder
cd %~dp0

rem dotnet msbuild build_server_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~1"
rem 
rem dotnet msbuild build_server_web_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~2"
rem 
dotnet msbuild build_dot_net_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~3"
rem 
rem msbuild build_angular_client_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~4"
rem 
rem dotnet msbuild build_constraint_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver
rem 
rem 
rem msbuild build_js_client.proj /property:NugetFolder=..\NugetOutput 
rem 
rem msbuild build_vue_client.proj /property:NugetFolder=..\NugetOutput 
rem 
rem dotnet msbuild build_dotnet_global_tool.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput /property:DeliverFolder=..\Deliver /property:ReleaseNotes="%~5"
rem 
rem msbuild build_react_client.proj /property:NugetFolder=..\NugetOutput 

