namespace ImmotionAR.ImmotionRoom.Helpers.CrossPlatformSupport
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    // Inspired by the ideas described here:
    // http://blogs.msdn.com/b/dsplaisted/archive/2012/08/27/how-to-make-portable-class-libraries-work-for-you.aspx
    // http://log.paulbetts.org/the-bait-and-switch-pcl-trick/
    // and the implementations in Portable Class Libraries Contrib by David Kean
    // and Microsoft Rx (Reactive Extensions) project.

    // An implementation IPlatformAdapterResolver that probes for platforms-specific adapters by dynamically
    // looking for concrete types in platform-specific assemblies, such as Platform.NET45 or Platform.UWP.
    internal class PlatformAdapterResolver : IPlatformAdapterResolver
    {
        private readonly string[] m_PlatformNames;
        private readonly Func<AssemblyName, Assembly> m_AssemblyLoader;
        private readonly object m_Lock = new object();
        private readonly Dictionary<Type, object> m_Adapters = new Dictionary<Type, object>();
        private readonly Dictionary<string, Assembly> m_Assemblies = new Dictionary<string, Assembly>();

        public PlatformAdapterResolver(params string[] platformNames) : this(Assembly.Load, platformNames)
        {
        }

        public PlatformAdapterResolver(Func<AssemblyName, Assembly> assemblyLoader, params string[] platformNames)
        {
            Debug.Assert(platformNames != null);
            Debug.Assert(assemblyLoader != null);

            m_PlatformNames = platformNames;
            m_AssemblyLoader = assemblyLoader;
        }

        public object Resolve(Type type)
        {
            Debug.Assert(type != null);

            lock (m_Lock)
            {
                object instance;
                if (!m_Adapters.TryGetValue(type, out instance))
                {
#if UNITY_5
                    var assembly = GetPlatformSpecificAssembly(type.Assembly.GetName().Name);
#else
                    var assembly = GetPlatformSpecificAssembly(type.GetTypeInfo().Assembly.GetName().Name);
#endif
                    instance = ResolveAdapter(assembly, type);
                    m_Adapters.Add(type, instance);
                }

                return instance;
            }
        }

        private static object ResolveAdapter(Assembly assembly, Type interfaceType)
        {
            var typeName = MakeAdapterTypeName(interfaceType);

            var type = assembly.GetType(typeName);
            if (type != null)
                return Activator.CreateInstance(type);

            return type;
        }

        private static string MakeAdapterTypeName(Type interfaceType)
        {
#if UNITY_5
            Debug.Assert(interfaceType.IsInterface);
#else
            Debug.Assert(interfaceType.GetTypeInfo().IsInterface);
#endif
            Debug.Assert(interfaceType.DeclaringType == null);
            Debug.Assert(interfaceType.Name.StartsWith("I", StringComparison.Ordinal));

            // For example, if we're looking for an implementation of ImmotionAR.ImmotionRoom.Helpers.Interfaces.IAppVersions, 
            // then we'll look for ImmotionAR.ImmotionRoom.Helpers.AppVersions
            var name = string.Format("{0}.{1}", interfaceType.Namespace.Replace(".Interfaces", ""), interfaceType.Name.Substring(1));
            return name;
        }

        private Assembly GetPlatformSpecificAssembly(string assemblyName)
        {
            if (!m_Assemblies.ContainsKey(assemblyName))
            {
                var assembly = LookForPlatformSpecificAssembly(assemblyName);
                if (assembly == null)
                    throw new InvalidOperationException(string.Format("Assembly Not Supported: {0}", assemblyName));

                m_Assemblies[assemblyName] = assembly;
            }

            return m_Assemblies[assemblyName];
        }

        private Assembly LookForPlatformSpecificAssembly(string assemblyName)
        {
            foreach (var platformName in m_PlatformNames)
            {
                var assembly = LookForPlatformSpecificAssembly(assemblyName, platformName);
                if (assembly != null)
                    return assembly;
            }

            return null;
        }

        private Assembly LookForPlatformSpecificAssembly(string pclAssemblyName, string platformName)
        {
            var pclAssembly = new AssemblyName(pclAssemblyName);
            //var pclAssemblyName= pclAssembly.GetName().Name;
            var assemblyName = new AssemblyName(pclAssembly.FullName);
            assemblyName.Name = string.Format("{0}.Platform.{1}", pclAssemblyName, platformName);

            try
            {
                return m_AssemblyLoader(assemblyName);
            }
            catch (FileNotFoundException)
            {
            }

            return null;
        }
    }
}
