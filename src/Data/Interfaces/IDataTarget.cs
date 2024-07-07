using System.Threading;
using System.Threading.Tasks;

namespace DataMigrationCore.Data.Interfaces
{
    public interface IDataTarget
    {
        void CommitChanges(PipelineResults results);
        Task CommitChangesAsync(PipelineResults results, CancellationToken cancellationToken);
    }
}
