namespace Sitecore.ItemBucket.Kernel.Kernel.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Return all instances of type T.
        /// </summary>
        /// <returns>Anonymous T</returns>
        IEnumerable<T> All();
       
        /// <summary>Returns the single entity matching the expression.
        /// Throws an exception if there is not exactly one such entity.</summary>
        /// <param name="exp">The Expression</param>
        /// <returns>Type T</returns>
        T Single(Func<T, bool> exp);

        /// <summary>Returns the first element satisfying the condition.</summary>
        /// <param name="exp">The Expression</param><returns>Type T</returns>
        T First(Func<T, bool> exp);
    } 
}
