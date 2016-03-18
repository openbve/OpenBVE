#!/usr/bin/env bash
MajorVersion=1
MinorVersion=4

# cd to correct directory
cd -P -- "$(dirname -- "$0")"

# determine revision and build numbers
if [[ "$OSTYPE" == "darwin"* ]]; then
#OSX
Revision=$(((($(date +%s) - $(date -jf "%Y-%m-%d" "2016-03-08" $B +%s))/86400 )+40 ))
Minutes=$(( $(date "+10#%H * 60 + 10#%M") ))
else
#Linux & Cygwin
Revision=$(( ( ($(date "+%s") - $(date --date="2016-03-08" +%s))/(60*60*24) )+40 ))
Minutes=$(( ( $(date "+%s") - $(date -d "today 0" +%s))/60 ))
fi


cat > AssemblyInfo.cs << EOF

 // This code was generated by a tool. Any changes made manually will be lost
 // the next time this code is regenerated.
 // 
 
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("openBVE")]
[assembly: AssemblyProduct("openBVE")]
[assembly: AssemblyCopyright("The openBVE Project")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("$MajorVersion.$MinorVersion.$Revision.$Minutes")]
[assembly: AssemblyInformationalVersion("$MajorVersion.$MinorVersion.$Revision.$Minutes-$USER")]
[assembly: AssemblyFileVersion("$MajorVersion.$MinorVersion.$Revision.$Minutes")]
[assembly: CLSCompliant(true)]

namespace OpenBve {
	internal static partial class Program {
		internal const bool IsDevelopmentVersion = false;
		internal const string VersionSuffix = "";
	}
}
EOF