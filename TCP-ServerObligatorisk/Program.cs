using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using ClassLibrary;

namespace TCP_ServerObligatorisk
{
    class Program
    {
        private static List<FootballPlayer> _players = new List<FootballPlayer>();
        private static int _nextId = 1;

        static void Main(string[] args)
        {
            Console.WriteLine("Obligatorisk TCP Server");

            TcpListener listener = new TcpListener(2121);

            listener.Start();

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Task.Run(() => NewClient(listener, socket));
            }
        }

        public static void NewClient(TcpListener listener, TcpClient socket)
        {
            Console.WriteLine("New client connected");

            NetworkStream ns = socket.GetStream();

            StreamReader reader = new StreamReader(ns);

            StreamWriter writer = new StreamWriter(ns);

            while (true)
            {
                try
                {
                    Console.WriteLine("HentAlle, Hent, Gem, Slut");

                    string message = reader.ReadLine();

                    if (message.StartsWith("HentAlle", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (_players?.Any() != true)
                        {
                            Console.WriteLine("Gem en spiller først");
                        }
                        else
                        {
                            foreach (FootballPlayer player in _players)
                            {
                                Console.WriteLine($"ID: {player.Id}, Navn: {player.Name}, Nummer: {player.ShirtNumber}, Pris: {player.Price}");
                            }

                            foreach (FootballPlayer player in _players)
                            {
                                writer.WriteLine(JsonSerializer.Serialize<FootballPlayer>(player));
                            }
                            writer.WriteLine("Alle spillere hentet");
                        }
                    }
                    
                    if (message.EndsWith("Hent", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("Skriv ID på den spiller du vil finde");
                        int getId = Convert.ToInt32(reader.ReadLine());

                        foreach (FootballPlayer player in _players)
                        {
                            if (player.Id == getId)
                            {
                                Console.WriteLine($"ID: {player.Id}, Navn: {player.Name}, Nummer: {player.ShirtNumber}, Pris: {player.Price}");
                                writer.WriteLine(JsonSerializer.Serialize<FootballPlayer>(player));
                            }
                        }
                        writer.WriteLine($"Spiller med ID: {getId} hentet");
                    }

                    if (message.StartsWith("Gem", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("Skriv JSON på den spiller du vil gemme");

                        string playerToSave = reader.ReadLine();

                        FootballPlayer player = JsonSerializer.Deserialize<FootballPlayer>(playerToSave);

                        player.Id = _nextId++;

                        _players.Add(player);

                        Console.WriteLine($"ID: {player.Id}, Navn: {player.Name}, Nummer: {player.ShirtNumber}, Pris: {player.Price} = GEMT");

                        writer.WriteLine("Spiller gemt");
                    }

                    writer.Flush();

                    if (message.StartsWith("slut", StringComparison.InvariantCultureIgnoreCase) && message.EndsWith("slut", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("Forbindelsen er sluttet");
                        socket.Close();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e + " - Forbindelsen er sluttet");
                    socket.Close();
                    break;
                }
            }
        }
    }
}
