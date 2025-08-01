using AzureSemantic.Process;


// await HotelProcess.Process().ConfigureAwait(false);

if (false)
{
    await LoadBalancerProcess.ProcessCreateIndex().ConfigureAwait(false);
}
await LoadBalancerProcess.ProcessSearch().ConfigureAwait(false);
// await QdrantProcess.ProcessSearch().ConfigureAwait(false);


Console.WriteLine("Hello, World!");





