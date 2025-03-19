.PHONY: build clean restore run publish

clean:
		dotnet clean ./AspNetCore.Profiler.sln -nologo -consoleLoggerParameters:NoSummary -verbosity:quiet

restore:
		dotnet restore ./AspNetCore.Profiler.sln

build:
		# dotnet build --verbosity minimal
		dotnet build ./AspNetCore.Profiler.sln --no-restore /clp:ErrorsOnly

build_mvc:
		# dotnet build --verbosity minimal
		dotnet build ./AspNetCore.Profiler.Mvc/AspNetCore.Profiler.Mvc.csproj --no-restore /clp:ErrorsOnly

build_gw:
		# dotnet build --verbosity minimal
		dotnet build ./AspNetCore.Profiler.Gateway/AspNetCore.Profiler.Gateway.csproj --no-restore /clp:ErrorsOnly
        
# start AspNetCore.Profiler.Mvc
run_mvc: build_mvc
		dotnet run --project ./AspNetCore.Profiler.Mvc/AspNetCore.Profiler.Mvc.csproj --launch-profile "Dev" --property:Configuration=Debug

# start AspNetCore.Profiler.Gateway
run_gw: build_gw
		dotnet run --project ./AspNetCore.Profiler.Gateway/AspNetCore.Profiler.Gateway.csproj --launch-profile "Dev" --property:Configuration=Debug

publish:
		# MSBuild.exe -clp:ErrorsOnly -nologo -m /property:Configuration=Release
		dotnet build --no-restore --configuration Release /clp:ErrorsOnly
		dotnet publish -c release -r centos.7-x64 --self-contained
		# dotnet publish -c release -r ubuntu.16.04-x64 --self-contained
		# The binary will be in "bin/Release/netcoreapp3.1/ubuntu.16.04-x64/publish"
