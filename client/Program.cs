using System.Net.Sockets;

string ip = "127.0.0.1";
int port = 55555;
using TcpClient user = new TcpClient();
Console.Write("Your name: ");
string? userName = Console.ReadLine();
StreamReader? reader = null;
StreamWriter? writer = null;
System.Console.WriteLine();
try
{
    user.Connect(ip, port);
    reader = new StreamReader(user.GetStream());
    writer = new StreamWriter(user.GetStream());
    if (reader is null || writer is null) return;
    Task.Run(() => GetMessage(reader));
    await SendMessage(writer);

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    writer?.Close();
    reader?.Close();
}

async Task GetMessage(StreamReader reader)
{
    while (true)
    {
        try
        {
            string? message = await reader.ReadLineAsync();
            if (!string.IsNullOrEmpty(message)) Console.WriteLine(message);
        }
        catch
        {
            break;
        }
    }
}

async Task SendMessage(StreamWriter writer)
{
    await writer.WriteLineAsync(userName);
    await writer.FlushAsync();
    Console.WriteLine("<<<Press Enter to send message>>>");

    while (true)
    {
        string? message = Console.ReadLine();
        await writer.WriteLineAsync(message);
        await writer.FlushAsync();
    }
}