using System;

namespace Optima.Utilities.Pagination
{
    /// <summary>
    /// Non-enumerable version of the PagedList class.
    /// </summary>
    [Serializable]
    public class PagedListMetaData : Optima.Utilities.Pagination.IPagedList
    {
        /// <summary>
        /// Protected constructor that allows for instantiation without passing in a separate list.
        /// </summary>
        protected PagedListMetaData()
        {
        }

        /// <summary>
        /// Non-enumerable version of the PagedList class.
        /// </summary>
        /// <param name="pagedList">A PagedList (likely enumerable) to copy metadata from.</param>
        public PagedListMetaData(Optima.Utilities.Pagination.IPagedList pagedList)
        {
            PageCount = pagedList.PageCount;
            TotalItemCount = pagedList.TotalItemCount;
            PageNumber = pagedList.PageNumber;
            PageSize = pagedList.PageSize;
        }

        #region IPagedList Members

        /// <summary>
        /// Total number of subsets within the superset.
        /// </summary>
        /// <value>Total number of subsets within the superset.</value>
        public int PageCount { get; protected set; }

        /// <summary>
        /// Total number of objects contained within the superset.
        /// </summary>
        /// <value>Total number of objects contained within the superset.</value>
        public int TotalItemCount { get; protected set; }

        /// <summary>
        /// One-based index of this subset within the superset.
        /// </summary>
        /// <value>One-based index of this subset within the superset.</value>
        public int PageNumber { get; protected set; }

        /// <summary>
        /// Maximum size any individual subset.
        /// </summary>
        /// <value>Maximum size any individual subset.</value>
        public int PageSize { get; protected set; }

        /// <summary>
        /// Returns true if this is NOT the first subset within the superset.
        /// </summary>
        /// <value>Returns true if this is NOT the first subset within the superset.</value>
        public bool HasPreviousPage { get; protected set; }

        /// <summary>
        /// Returns true if this is NOT the last subset within the superset.
        /// </summary>
        /// <value>Returns true if this is NOT the last subset within the superset.</value>
        public bool HasNextPage { get; protected set; }

        #endregion IPagedList Members
    }
}