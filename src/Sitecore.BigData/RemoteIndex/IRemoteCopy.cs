// -----------------------------------------------------------------------
// <copyright file="IRemoteCopy.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sitecore.ItemBuckets.BigData.RemoteIndex
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IRemoteCopy
    {
        void ReIndex();

        void Optimise();

        void Fetch();
    }
}
