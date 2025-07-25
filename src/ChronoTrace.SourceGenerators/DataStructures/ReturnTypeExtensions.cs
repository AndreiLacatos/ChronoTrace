namespace ChronoTrace.SourceGenerators.DataStructures
{
    internal static class ReturnTypeExtensions
    {
        /// <summary>
        /// Shows whether the <see cref="MethodType"/> represents an asynchronous method
        /// </summary>
        /// <param name="methodType">Value</param>
        /// <returns></returns>
        internal static bool IsAsync(this MethodType methodType)
        {
            const int asyncFlag = 0b01;
            return ((int)methodType & asyncFlag) == asyncFlag;
        }

        /// <summary>
        /// Determines if the method returns a value that can be captured.
        /// Returns <c>false</c> for methods returning <c>void</c> or a non-generic <c>Task</c>.
        /// </summary>
        /// <param name="methodType">Value</param>
        /// <returns></returns>
        internal static bool IsVoidType(this MethodType methodType)
        {
            const int voidFlag = 0b10;
            return ((int)methodType & voidFlag) == voidFlag;
        }
    }
}
