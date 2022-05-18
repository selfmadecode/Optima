using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Optima.Utilities.Pagination
{
    /// <summary>
    /// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata
    /// about the superset collection of objects this subset was created from.
    /// </summary>
    /// <typeparam name="T">The type of object the collection should contain.</typeparam>
    /// <seealso cref="IPagedList{T}" />
    /// <seealso cref="BasePagedList{T}" />
    /// <seealso cref="StaticPagedList{T}" />
    /// <seealso cref="List{T}" />
    /// <remarks>Represents a subset of a collection of objects that can be individually accessed by index and containing metadata
    /// about the superset collection of objects this subset was created from.</remarks>
    [JsonObject]
    public class PagedList<T> : BasePagedList<T>
    {
        //Use this contructor to create a new paginated data
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalCount">The total count.</param>
        public PagedList(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
        {
            TotalItemCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Subset.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}" /> class that divides the supplied superset into subsets
        /// the size of the supplied pageSize. The instance then only containes the objects contained in the subset specified
        /// by index.
        /// </summary>
        /// <param name="superset">The collection of objects to be divided into subsets. If the collection implements
        /// <see cref="IQueryable{T}" />, it will be treated as such.</param>
        /// <param name="pageNumber">The one-based index of the subset of objects to be contained by this instance.</param>
        /// <param name="pageSize">The maximum size of any individual subset.</param>
        /// <exception cref="ArgumentOutOfRangeException">pageNumber - PageNumber cannot be below 1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">pageSize - PageSize cannot be less than 1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified index cannot be less than zero.</exception>
        public PagedList(IQueryable<T> superset, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "PageNumber cannot be below 1.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, "PageSize cannot be less than 1.");

            // set source to blank list if superset is null to prevent exceptions
            TotalItemCount = superset == null ? 0 : superset.Count();
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = TotalItemCount > 0
                ? (int) Math.Ceiling(TotalItemCount / (double) PageSize)
                : 0;

            // add items to internal list
            if (superset != null && TotalItemCount > 0)
                Subset.AddRange(pageNumber == 1
                    ? superset.Skip(0).Take(pageSize).ToList()
                    : superset.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
                );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}" /> class that divides the supplied superset into subsets
        /// the size of the supplied pageSize. The instance then only containes the objects contained in the subset specified
        /// by index.
        /// </summary>
        /// <param name="superset">The collection of objects to be divided into subsets. If the collection implements
        /// <see cref="IQueryable{T}" />, it will be treated as such.</param>
        /// <param name="pageNumber">The one-based index of the subset of objects to be contained by this instance.</param>
        /// <param name="pageSize">The maximum size of any individual subset.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified index cannot be less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified page size cannot be less than one.</exception>
        public PagedList(IEnumerable<T> superset, int pageNumber, int pageSize)
            : this(superset.AsQueryable(), pageNumber, pageSize)
        {
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        public override IEnumerable<T> Items => Subset;
    }
}