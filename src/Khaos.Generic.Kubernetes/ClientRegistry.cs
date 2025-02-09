using System.Collections.Concurrent;
using k8s;
using Microsoft.Extensions.Options;

namespace Khaos.Generic.Kubernetes;

public interface IClientRegistry
{
    IKubernetes GetClient(string clusterName);
}

internal class ClientRegistry(IOptions<Options> options) : IClientRegistry
{
    private readonly ConcurrentDictionary<string, IKubernetes> _clients = new();

    public IKubernetes GetClient(string clusterName)
    {
        return _clients.GetOrAdd(clusterName, cluster =>
        {
            if (!options.Value.Clusters.TryGetValue(cluster, out var clusterOptions))
            {
                throw new ArgumentException($"Cluster {cluster} not found");
            }
            
            return new k8s.Kubernetes(clusterOptions);
        });
    }
}