using System.Reflection;

[assembly: AssemblyProduct("DarksProtector")]
[assembly: AssemblyCompany("darkshoz")]
[assembly: AssemblyCopyright("Copyright (C) darkshoz 2019")]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else

[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
[assembly: AssemblyInformationalVersion("2.0")]