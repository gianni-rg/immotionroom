namespace ImmotionAR.ImmotionRoom.Helpers.CrossPlatformSupport
{
    using System;

    // Inspired by the ideas described here:
    // http://blogs.msdn.com/b/dsplaisted/archive/2012/08/27/how-to-make-portable-class-libraries-work-for-you.aspx
    // http://log.paulbetts.org/the-bait-and-switch-pcl-trick/
    // and the implementations in Portable Class Libraries Contrib by David Kean
    // and Microsoft Rx (Reactive Extensions) project.

    internal interface IPlatformAdapterResolver
    {
        object Resolve(Type type);
    }
}
