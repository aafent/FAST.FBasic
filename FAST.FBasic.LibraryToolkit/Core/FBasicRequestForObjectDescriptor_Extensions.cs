namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Extensions to class FBasicRequestForObjectDescriptor
    /// </summary>
    public static class FBasicRequestForObjectDescriptor_Extensions
    {

        /// <summary>
        /// Get the "Context.Group.Name"
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static string Level3Request(this IFBasicRequestForObjectDescriptor descriptor)
        {
            return $"{descriptor.Context}.{descriptor.Group}.{descriptor.Name}";
        }

        /// <summary>
        /// Get the "Context.Group"
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static string Level2Request(this IFBasicRequestForObjectDescriptor descriptor)
        {
            return $"{descriptor.Context}.{descriptor.Group}";
        }
    }
}
