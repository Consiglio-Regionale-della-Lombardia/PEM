namespace Scheduler.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>
    ///     Exception to manage path not found
    /// </summary>
    public class PathNotFoundException : Exception
    {
        #region Props / Indexers

        /// <summary>
        ///     The invalid path
        /// </summary>
        public string Path { get; set; }

        #endregion Props / Indexers

        #region Ctors

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="path">The invalid config path</param>
        public PathNotFoundException(string path) : base("No file found with the specified path (" + path + ")")
        {
            Path = path;
        }

        #endregion Ctors
    }
}