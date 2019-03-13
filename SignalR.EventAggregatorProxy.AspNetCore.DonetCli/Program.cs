using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using SignalR.EventAggregatorProxy.Event;

namespace dotnet_signalreventproxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting SignalR.EventAggregatorProxy generation of event contracts!");

            if(args.Length < 2) throw new ArgumentException("dotnet signalreventproxy [Path_to_event_type_finder_dll] [Path_to_output_js] [Event_type_finder_type(optional)]");

            var starPath = Directory.GetCurrentDirectory();
            var eventTypeFinderAssembly = AssemblyLoader.LoadFromAssemblyPath($"{starPath}\\{args[0]}");

            var lookup = typeof(IEventTypeFinder);
            var typeFinderType = args.Length > 2 ? eventTypeFinderAssembly.GetType(args[2]) : eventTypeFinderAssembly.GetExportedTypes().First(t => !t.IsAbstract && lookup.IsAssignableFrom(t));

            var finder = Activator.CreateInstance(typeFinderType) as IEventTypeFinder;
            var eventTypes = finder.ListEventsTypes();

            var parser = new Parser(eventTypes);
            var result = parser.Parse();
            File.WriteAllText(args[1], result);
        }

        public static class AssemblyLoader
        {
            public static Assembly LoadFromAssemblyPath(string assemblyFullPath)
            {
                var fileNameWithOutExtension = Path.GetFileNameWithoutExtension(assemblyFullPath);
                var fileName = Path.GetFileName(assemblyFullPath);
                var directory = Path.GetDirectoryName(assemblyFullPath);

                var inCompileLibraries = DependencyContext.Default.CompileLibraries.Any(l => l.Name.Equals(fileNameWithOutExtension, StringComparison.OrdinalIgnoreCase));
                var inRuntimeLibraries = DependencyContext.Default.RuntimeLibraries.Any(l => l.Name.Equals(fileNameWithOutExtension, StringComparison.OrdinalIgnoreCase));

                var assembly = (inCompileLibraries || inRuntimeLibraries)
                    ? Assembly.Load(new AssemblyName(fileNameWithOutExtension))
                    : AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFullPath);

                if (assembly != null)
                    LoadReferencedAssemblies(assembly, fileName, directory);

                return assembly;
            }

            private static void LoadReferencedAssemblies(Assembly assembly, string fileName, string directory)
            {
                var filesInDirectory = Directory.GetFiles(directory).Where(x => x != fileName).Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
                var references = assembly.GetReferencedAssemblies();

                foreach (var reference in references)
                {
                    if (filesInDirectory.Contains(reference.Name))
                    {
                        try
                        {
                            var loadFileName = reference.Name + ".dll";
                            var path = Path.Combine(directory, loadFileName);
                            var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                            if (loadedAssembly != null)
                                LoadReferencedAssemblies(loadedAssembly, loadFileName, directory);
                        }
                        catch 
                        {

                        }
                    }
                }

            }

        }
    }
}
