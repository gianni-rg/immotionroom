namespace ImmotionAR.ImmotionRoom.Helpers.CrossPlatformSupport
{
    using System;

    // Inspired by the ideas described here:
    // http://blogs.msdn.com/b/dsplaisted/archive/2012/08/27/how-to-make-portable-class-libraries-work-for-you.aspx
    // http://log.paulbetts.org/the-bait-and-switch-pcl-trick/
    // and the implementations in Portable Class Libraries Contrib by David Kean
    // and Microsoft Rx (Reactive Extensions) project.

    // Enables types within PCL libraries to use platform-specific features in a platform-agnostic way
    public static class PlatformAdapter
    {
        private static readonly string[] KnownPlatformNames = {"NET45", "UWP" }; //, "Phone", "Store"};
        private static IPlatformAdapterResolver m_Resolver = new PlatformAdapterResolver(KnownPlatformNames);

        public static T Resolve<T>()
        {
            var value = (T) m_Resolver.Resolve(typeof (T));

            if (value == null)
                throw new PlatformNotSupportedException("Adapter Not Supported");

            return value;
        }

        // Unit testing helper
        internal static void SetResolver(IPlatformAdapterResolver resolver)
        {
            m_Resolver = resolver;
        }
    }
}
