using System.Reflection;
using System.Runtime.CompilerServices;

// TODO: Werte der Assemblyattribute überprüfen

// .NET framework version constants are defined in TargetFrameworkVersionConstants.prop
#if NET_45
[assembly: AssemblyTitle("OSCI 1.2 Transport-Bibliothek .NET 4.5")]
#elif NET_40
[assembly: AssemblyTitle("OSCI 1.2 Transport-Bibliothek .NET 4.0")]
#elif NET_35
[assembly: AssemblyTitle("OSCI 1.2 Transport-Bibliothek .NET 3.5")]
#endif

[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("Governikus GmbH & Co. KG")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyDelaySign(false)]

[assembly:InternalsVisibleTo("osci-bibliothek-net-test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100fda6cb064a5a8e4158a5811fb9a9c7717841bc950f23d8827c50dba6dafede6a01369a4acf8e37ada4bc1db74074f56caf6b65732e5b30aaa5c7bffaec72bbbad5fe9644bb0e490b027df7a98f4eb6cb02dff2409472f1706b7b23c061cbe84eb8b437179233c81d38540e484981ed01c9b33fa5c81f373f57af895a14f017b5")]