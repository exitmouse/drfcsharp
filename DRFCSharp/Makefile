release:
	dmcs DRFCSharp/*.cs -pkg:dotnet -r:nunit.core -r:nunit.framework -r:DRFCSharp/bin/Release/MathNet.Numerics.dll -r:DRFCSharp/bin/Release/zlib.net.dll -out:DRFCSharp/bin/Release/DRFCSharp.exe

debug:
	dmcs DRFCSharp/*.cs -debug -pkg:dotnet -r:nunit.core -r:nunit.framework -r:DRFCSharp/bin/Debug/MathNet.Numerics.dll -r:DRFCSharp/bin/Debug/zlib.net.dll -out:DRFCSharp/bin/Debug/DRFCSharp.exe

debug-tests:
	dmcs DRFCSharp/*.cs DRFCSharp/Tests/*.cs -debug -pkg:dotnet -r:nunit.core -r:nunit.framework -r:DRFCSharp/bin/Debug/MathNet.Numerics.dll -r:DRFCSharp/bin/Debug/zlib.net.dll -out:DRFCSharp/bin/Debug/DRFCSharp.exe
