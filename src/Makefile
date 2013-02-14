options:
	gmcs -target:library -debug+ -d:LINQ -r:System.Core DRFCSharp/NDesk/Options.cs

release: options
	dmcs DRFCSharp/*.cs -pkg:dotnet -r:nunit.core -r:nunit.framework -r:DRFCSharp/bin/Release/MathNet.Numerics.dll -r:DRFCSharp/bin/Release/zlib.net.dll -r:DRFCSharp/NDesk/Options.dll -out:DRFCSharp/bin/Release/DRFCSharp.exe

debug: options
	dmcs DRFCSharp/*.cs -debug -pkg:dotnet -r:nunit.core -r:nunit.framework -r:DRFCSharp/bin/Debug/MathNet.Numerics.dll -r:DRFCSharp/bin/Debug/zlib.net.dll -r:DRFCSharp/NDesk/Options.dll -out:DRFCSharp/bin/Debug/DRFCSharp.exe

debug-tests: options
	dmcs DRFCSharp/*.cs DRFCSharp/Tests/*.cs -debug -pkg:dotnet -r:nunit.core -r:nunit.framework -r:DRFCSharp/bin/Debug/MathNet.Numerics.dll -r:DRFCSharp/bin/Debug/zlib.net.dll -r:DRFCSharp/NDesk/Options.dll -out:DRFCSharp/bin/Debug/DRFCSharp.exe
