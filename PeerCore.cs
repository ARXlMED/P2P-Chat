using P2P_Chat.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace P2P_Chat
{
    public class PeerCore : IDisposable
    {
        public string name;
        public IPAddress myIP;
        public int tcpPort;
        public int udpPort;
        private readonly Guid instanceId = Guid.NewGuid();

        private ConcurrentDictionary<IPEndPoint, PeerInfo> peers; // adr + name

        private Socket udpListenSocket;
        private Socket tcpListenSocket;

        public bool isAlive = false;

        public event Action<ChatEvent> nowEvent;



        public PeerCore(string name, IPAddress myIP, int TCPPort = 12345, int UDPPort = 12346)
        {
            this.name = name;
            this.myIP = myIP;
            this.tcpPort = TCPPort;
            this.udpPort = UDPPort;
            peers = new ConcurrentDictionary<IPEndPoint, PeerInfo>();
        }

        public async Task StartAsync()
        {
            isAlive = true;

            udpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            udpListenSocket.Bind(new IPEndPoint(myIP, udpPort));
            _ = Task.Run(() => ListenUDPAsync());
            
            tcpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpListenSocket.Bind(new IPEndPoint(myIP, tcpPort));
            tcpListenSocket.Listen(100);
            _ = Task.Run(() => ListenTCPAsync());

            await SendBroadcastUDPAsync();
        }

        private IPAddress GetBroadcastAddress()
        {
            if (IPAddress.IsLoopback(myIP))
                return IPAddress.Parse("127.255.255.255");

            return IPAddress.Broadcast;
        }

        private async Task SendBroadcastUDPAsync()
        {
            using var udpSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            var packet = new DiscoveryPacket
            {
                Id = instanceId,
                Name = name,
                TcpPort = tcpPort
            };

            byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet));
            var broadcastPoint = new IPEndPoint(GetBroadcastAddress(), udpPort);

            await udpSendSocket.SendToAsync(data, broadcastPoint);
        }

        private async Task ListenUDPAsync()
        {
            byte[] buffer = new byte[1024];
            EndPoint anyEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (isAlive)
            {
                try
                {
                    var result = await udpListenSocket.ReceiveFromAsync(buffer, anyEndPoint);
                    int received = result.ReceivedBytes;
                    IPEndPoint remoteUdpEndPoint = (IPEndPoint)result.RemoteEndPoint;

                    var packet = JsonSerializer.Deserialize<DiscoveryPacket>(Encoding.UTF8.GetString(buffer, 0, received));

                    if (packet == null) continue;
                    if (packet.Id == instanceId) continue; 

                    var tcpEndPoint = new IPEndPoint(remoteUdpEndPoint.Address, packet.TcpPort);

                    MessageBox.Show($"UDP получен от {remoteUdpEndPoint.Address}:{remoteUdpEndPoint.Port}");

                    if (!peers.ContainsKey(tcpEndPoint))
                    {
                        _ = ConnectToPeerTCPAsync(tcpEndPoint, packet.Name);
                    }
                }
                catch (SocketException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        // слушает на TCP
        private async Task ListenTCPAsync() 
        {
            while (isAlive)
            {
                try
                {
                    var clientSocket = await tcpListenSocket.AcceptAsync();
                    IPEndPoint remoteEndpoint = (IPEndPoint)clientSocket.RemoteEndPoint;
                    _ = HandlerTCPConnectionAsync(clientSocket, remoteEndpoint, null);
                }
                catch (SocketException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        // соединяет с пиром после получения UDP
        private async Task ConnectToPeerTCPAsync(IPEndPoint EndPoint, string PeerName) 
        {
            if (peers.ContainsKey(EndPoint)) return;

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(EndPoint);
                await HandlerTCPConnectionAsync(socket, EndPoint, PeerName);
            }
            catch
            {
                socket.Close();
            }
        }

        // главный метод установки tcp соединений и обработки их для отправки в UI
        private async Task HandlerTCPConnectionAsync(Socket socket, IPEndPoint remoteEndPoint, string ExpectedPeerName)
        {
            try
            {
                await SendMessageTCPAsync(socket, 2, ConvertToByte(name));

                var (typeName, dataName) = await ReceiveMessageTCPAsync(socket);
                if (typeName != 2)
                {
                    socket.Close();
                    return;
                }
                string RealPeerName = Encoding.UTF8.GetString(dataName);

                if (ExpectedPeerName != null && ExpectedPeerName != RealPeerName)
                {
                    socket.Close();
                    return;
                }

                PeerInfo peerInfo = new PeerInfo 
                {
                    Name = RealPeerName,
                    Socket = socket
                };
                peers[remoteEndPoint] = peerInfo;
                MessageBox.Show($"Peer added: {RealPeerName} at {remoteEndPoint}");

                // отображение имени того кто зашел
                nowEvent?.Invoke(new ChatEvent
                {
                    Timestamp = DateTime.Now,
                    Type = "Name",
                    Name = RealPeerName,
                    Ip = remoteEndPoint.Address.ToString(),
                    Text = null
                });

                while (isAlive && socket.Connected)
                {
                    var (typeMessage, dataMessage) = await ReceiveMessageTCPAsync(socket);
                    if (typeMessage == 0) break;
                    switch (typeMessage)
                    {
                        case 1: 
                            string stringMessage = Encoding.UTF8.GetString(dataMessage);
                            // отображение сообщения которое пришло нам
                            nowEvent?.Invoke(new ChatEvent
                            {
                                Timestamp = DateTime.Now,
                                Type = "Message",
                                Name = RealPeerName,
                                Ip = remoteEndPoint.Address.ToString(),
                                Text = stringMessage
                            });
                            break;
                        case 3:
                            // отображение заверешния соединения с узлом
                            nowEvent?.Invoke(new ChatEvent
                            {
                                Timestamp = DateTime.Now,
                                Type = "CloseConnection",
                                Name = RealPeerName,
                                Ip = remoteEndPoint.Address.ToString(),
                                Text = null
                            });
                            return;
                    }
                }
            }
            catch (SocketException) { }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            finally
            {
                socket?.Close();
            }
        }

        // отправка сообщения всем пирам к которым есть подключение, используется во ViewModel
        public async Task BroadCastMessageTCPAsync(byte[] data, byte type = 1)
        {
            foreach (var peer in peers.Values)
            {
                try
                {
                    await SendMessageTCPAsync(peer.Socket, type, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            nowEvent?.Invoke(new ChatEvent
            {
                Timestamp = DateTime.Now,
                Type = "MyMessage",
                Name = name,
                Ip = myIP.ToString(),
                Text = Encoding.UTF8.GetString(data)
            });
        }

        private async Task ReceiveExactAsync(Socket socket, Memory<byte> buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int got = await socket.ReceiveAsync(buffer.Slice(offset), SocketFlags.None);
                if (got == 0)
                    throw new SocketException();
                offset += got;
            }
        }

        private async Task SendExactAsync(Socket socket, ReadOnlyMemory<byte> buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int sent = await socket.SendAsync(buffer.Slice(offset), SocketFlags.None);
                if (sent == 0)
                    throw new SocketException();
                offset += sent;
            }
        }

        // отправка сообщения конкретному пиру
        private async Task SendMessageTCPAsync(Socket socket, byte type, byte[] data)
        {
            if (socket == null || !socket.Connected) return;
            ushort len = (ushort)data.Length;
            byte[] header = new byte[3];
            header[0] = type;
            header[1] = (byte)(len >> 8);
            header[2] = (byte)(len & 0xFF);
            await SendExactAsync(socket, header);
            await SendExactAsync(socket, data);
        }

        private async Task<(byte type, byte[] data)> ReceiveMessageTCPAsync(Socket socket)
        {
            byte[] header = new byte[3];
            await ReceiveExactAsync(socket, header);
            byte type = header[0];
            int len = (header[1] << 8) | header[2];
            byte[] data = new byte[len];
            if (len > 0) await ReceiveExactAsync(socket, data);
            return (type, data);
        }

        // конвертация текста в байты
        private byte[] ConvertToByte(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        // очистка данных
        public void Dispose()
        {
            isAlive = false;
            foreach (PeerInfo peer in peers.Values)
            {
                try
                {
                    SendMessageTCPAsync(peer.Socket, 3, ConvertToByte("")).Wait();
                }
                catch { }
                finally
                {
                    peer.Socket?.Close();
                }
            }
            tcpListenSocket?.Close();
            udpListenSocket?.Close();
            peers.Clear();
        }
    }
}
