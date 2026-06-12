namespace EmbeddingPlayground.Console.Data;

public sealed class DocGenerator
{
    private static readonly string[] Domains =
    [
        "Distributed Systems",
        "Databases",
        "Caching",
        "Authentication",
        "Logging & Observability",
        "Networking",
        "Storage Systems",
        "Message Queues",
        "Search Systems",
        "Infrastructure"
    ];

    private static readonly string[] Systems =
    [
        "Redis Cluster",
        "PostgreSQL",
        "Kafka",
        "ElasticSearch",
        "API Gateway",
        "Service Mesh",
        "Load Balancer",
        "Object Storage",
        "Authentication Service",
        "Monitoring Stack"
    ];

    private static readonly string[] Concepts =
    [
        "latency spikes",
        "connection pooling",
        "replication lag",
        "cache eviction",
        "token expiration",
        "rate limiting",
        "backpressure handling",
        "data consistency",
        "failover behavior",
        "request retries",
        "circuit breaking",
        "hot partitions"
    ];

    private static readonly string[] Actions =
    [
        "debug",
        "investigate",
        "optimize",
        "troubleshoot",
        "configure",
        "scale",
        "stabilize",
        "diagnose",
        "monitor",
        "repair"
    ];

    public static List<Doc> Generate(int count = 500)
    {
        var rnd = new Random();
        var docs = new List<Doc>();

        for (int i = 0; i < count; i++)
        {
            var domain = Domains[rnd.Next(Domains.Length)];
            var system = Systems[rnd.Next(Systems.Length)];
            var concept = Concepts[rnd.Next(Concepts.Length)];
            var action = Actions[rnd.Next(Actions.Length)];

            var id = $"{domain}-{i}-{Guid.NewGuid():N}";

            var title = $"{action} {concept} in {system}";

            var content =
$"""
# {title}

## Overview
This document describes how to {action} {concept} when operating {system} in large-scale production environments.

These issues typically appear under high throughput conditions or during partial infrastructure degradation.

## Symptoms
- Increased request latency under load
- Intermittent timeouts across services
- Uneven resource utilization
- Elevated error rates in telemetry dashboards

## Root Causes
In most cases, {concept} in {system} is caused by one or more of the following:
- Improper configuration of internal buffers
- Inefficient connection reuse patterns
- Uneven load distribution across nodes
- Delayed cleanup of expired state
- Network congestion or packet loss

## Investigation Steps
1. Check system metrics (CPU, memory, IO saturation)
2. Inspect logs for repeated retry patterns
3. Validate configuration consistency across nodes
4. Analyze request traces for bottlenecks
5. Compare healthy vs degraded cluster behavior

## Mitigation Strategies
- Adjust timeout and retry policies
- Enable adaptive load balancing
- Tune cache or queue parameters
- Scale horizontally when saturation exceeds thresholds
- Enable backpressure controls where applicable

## Long-term Fix
To permanently resolve {concept}, teams should:
- Introduce better observability tooling
- Improve system resilience under partial failure
- Reduce hot-spot dependencies
- Implement smarter routing and partitioning strategies
""";

            docs.Add(new Doc(id, domain, title, content));
        }

        return docs;
    }
}