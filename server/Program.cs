using System.Net;
using System.Net.Sockets;

Server server = new Server();
await server.Listening();
class Server
{
    TcpListener server = new TcpListener(IPAddress.Any, 55555);
    List<Users> users = new List<Users>();

    protected internal void DropConnection(string id)
    {
        Users? user = users.FirstOrDefault(c => c.Id == id);
        if (user != null)
        {
            users.Remove(user);
            user.CloseAll();
        }

    }
    protected internal async Task Listening()
    {
        try
        {
            server.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                TcpClient tcpClient = await server.AcceptTcpClientAsync();
                Console.WriteLine($"Incoming connection: {tcpClient.Client.RemoteEndPoint}");
                Users user = new Users(tcpClient, this);
                users.Add(user);
                Task.Run(user.StartChat);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            DisconnectUsers();
        }
    }

    protected internal async Task WorkingMessager(string message, string id)
    {
        foreach (var user in users)
        {
            if (user.Id != id)
            {
                await user.writer.WriteLineAsync(message);
                await user.writer.FlushAsync();
            }
        }
    }
    protected internal void DisconnectUsers()
    {
        foreach (var user in users)
        {
            user.CloseAll();
        }
        server.Stop();
    }

}
class Users
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter writer { get; }
    protected internal StreamReader reader { get; }
    TcpClient user;
    Server server;

    public Users(TcpClient user, Server server)
    {
        this.user = user;
        this.server = server;
        var stream = user.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);
    }

    public async Task StartChat()
    {
        try
        {
            string? userName = await reader.ReadLineAsync();
            string? message = "";
            while (true)
            {
                try
                {

                    if (message != null)
                    {
                        message = await reader.ReadLineAsync();
                        message = $"{userName}: {message}";
                        Console.WriteLine(message);
                        await server.WorkingMessager(message, Id);
                    }

                }
                catch
                {
                    message = $"{userName} покинул чат";
                    Console.WriteLine(message);
                    await server.WorkingMessager(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

        }
        finally
        {
            server.DropConnection(Id);
        }
    }

    protected internal void CloseAll()
    {
        writer.Close();
        reader.Close();
        user.Close();
    }

}