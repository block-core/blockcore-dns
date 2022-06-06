// See https://aka.ms/new-console-template for more information



using DNS.Client;
using DNS.Protocol;
using System.Net;

if (args.Length == 0)
{
    Console.WriteLine("lookup 127.0.0.1 53 blockcore.net AAAA");
    return;
}

try
{
    var iPEndPoint = new IPEndPoint(IPAddress.Parse((string)args[0]), int.Parse((string)args[1]));
    DnsClient client = new DnsClient(iPEndPoint);

    ClientRequest request = client.Create();

    var question = new Question(Domain.FromString((string)args[2]), Enum.Parse<RecordType>((string)args[3]));
    request.Questions.Add(question);
    request.RecursionDesired = true;

    Console.WriteLine($"calling server={iPEndPoint}");

    Console.WriteLine(request.ToString());

    var response = request.Resolve().Result;

    Console.WriteLine(response.ToString());
}
catch (Exception ex)
{
    Console.WriteLine(ex);

    if (ex.InnerException is DNS.Client.ResponseException exr)
    {
        Console.WriteLine(exr.Response.ToString());
    }
}

