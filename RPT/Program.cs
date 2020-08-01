using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using static RPT.Entidades;

namespace RPT
{
    class Program
    {
        enum Verbs
        {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }
        enum Options
        {
            SGA = 3
        }
        static int TimeOutMs = 1000;
        static void Main(string[] args)
        {
            while (true)
            {
                Mensajes Msj = new Mensajes();

                Console.WriteLine(Msj.MensajeEncabezado());
                Console.WriteLine(Msj.MenuInicial());

                string IpRecolectada = Console.ReadLine();
                IpRecolectada = ConfirmarIp(IpRecolectada, Msj);
                try
                {
                    TcpClient tc = new TcpClient(IpRecolectada, 23);

                    var c = Login("rsegovia", "BDF5DCN>=", 1000, tc);

                    if (c.Split('\n')[2].Contains("rsegovia"))
                    {
                        Console.WriteLine("Conexion Establecida con exito!.");
                        WriteLine("onu status", tc);
                        System.Threading.Thread.Sleep(1000);
                        WriteLine("yes", tc);
                        Console.WriteLine("Recopilando posiciones.");
                        List<string> Posiciones = new List<string>();
                        System.Threading.Thread.Sleep(1000);
                        bool TerminoDeCarga = false;
                        string PrimeraLista = "";

                        while (!TerminoDeCarga)
                        {
                            PrimeraLista = Read(tc);
                            if (PrimeraLista.Contains("dBm") || PrimeraLista.Contains("====="))
                            {
                                TerminoDeCarga = true;
                                break;
                            }
                            else
                            {
                                if (PrimeraLista != "") 
                                {
                                    Console.WriteLine(PrimeraLista);
                                }
                                System.Threading.Thread.Sleep(10000);
                            }
                        }

                        List<string> ListPrimeraLista = PrimeraLista.Split('\n').ToList();
                        foreach (string po in ListPrimeraLista)
                        {
                            if (po.Contains("dBm"))
                            {
                                Posiciones.Add(po);
                            }
                        }

                        WriteLine("a", tc);
                        System.Threading.Thread.Sleep(5000);
                        string SegundaLista = Read(tc);
                        List<string> ListSegundaLista = SegundaLista.Split('\n').ToList();
                        foreach (string po in ListSegundaLista)
                        {
                            if (po.Contains("dBm"))
                            {
                                Posiciones.Add(po);
                            }
                        }
                        Console.WriteLine("Posiciones Recopiladas con exito - Total  = " + Posiciones.Count.ToString());
                        List<ObjetoPosiciones> objpo = new List<ObjetoPosiciones>();
                        foreach (string Po in Posiciones)
                        {
                            string line = Po.Replace("\r", "").Replace("dBm", "");
                            var cline = line.Split(' ').ToList().Where(x => x != "").ToList();
                            objpo.Add(new ObjetoPosiciones { ID = cline[0], Onu = cline[1], OperStatus = cline[2], ConfigState = cline[3], DownloadState = cline[4], Tx = cline[5], Rx = cline[6], KM = cline[7], OnuStatus = cline[8], State = cline[9] });
                        }

                        List<ObjScaner> objScan = new List<ObjScaner>();
                        Console.WriteLine("Inicia Iteracion de posiciones (on status xxxxx port eth 1)");
                        int PosicionNumber = 0;
                        foreach (ObjetoPosiciones Po in objpo)
                        {
                            Console.WriteLine("Posicion " + Po.Onu + " - Numero : " + (PosicionNumber + 1) + " De : " + Posiciones.Count.ToString());
                            WriteLine("onu status " + Po.Onu + " port eth 1", tc);
                            System.Threading.Thread.Sleep(1000);
                            string RespuestaComandoFinal = Read(tc).Replace("\r", "").Replace("\t", "");

                            var ParametrosNecesarios = RespuestaComandoFinal.Split('\n').Where(x => x.Contains("Configured Auto-Detection") || x.Contains("Administrative State") || x.Contains("Operational State") || x.Contains("Connection Type")).ToList();

                            objScan.Add(new ObjScaner
                            {
                                Onu = Po.Onu
                                ,
                                ConfiguredAutoDetection = (ParametrosNecesarios.Count > 0 ? ParametrosNecesarios[0].Replace("Configured Auto-Detection", "").Replace(" ", "") : "Data no disponible")
                                ,
                                AdministrativeState = (ParametrosNecesarios.Count > 0 ? ParametrosNecesarios[1].Replace("Administrative State", "").Replace(" ", "") : "Data no disponible")
                                ,
                                OperationalState = (ParametrosNecesarios.Count > 0 ? ParametrosNecesarios[2].Replace("Operational State", "").Replace(" ", "") : "Data no disponible")
                                ,
                                ConnectionType = (ParametrosNecesarios.Count > 0 ? ParametrosNecesarios[3].Replace("Connection Type", "").Replace(" ", "") : "Data no disponible")
                            });
                            PosicionNumber++;
                        }

                        StringBuilder sbOutput = new StringBuilder();
                        sbOutput.AppendLine("ID,Onu,OperStatus,ConfigState,DownLoadState,TX,RX,KM,OnuState,State,ConfiguredAutoDetection,AdministrativeState,OperationalState,ConnectionType");
                        foreach (ObjetoPosiciones Po in objpo)
                        {
                            ObjScaner scan = objScan.Find(x => x.Onu == Po.Onu);
                            sbOutput.AppendLine(Po.ID + "," + Po.Onu + "," + Po.OperStatus + "," + Po.ConfigState + "," + Po.DownloadState + "," + Po.Tx + "," + Po.Rx + "," + Po.KM + "," + Po.OnuStatus + "," + Po.State + "," + scan.ConfiguredAutoDetection + "," + scan.AdministrativeState + "," + scan.OperationalState + "," + scan.ConnectionType);
                        }
                        File.WriteAllText(@"C:\Archivo_" + IpRecolectada.Replace(".", "_") + ".csv", sbOutput.ToString());
                        Console.WriteLine("Archivo guardado en " + @"C:\Archivo_" + IpRecolectada.Replace(".", "_") + ".csv");
                        Console.WriteLine("Terminado - Precione Enter para continuar  ... ");
                        Console.ReadLine();
                    }
                    Console.Clear();
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("- Ocurrio un error Message : "+ex.Message+"\n - Inner : "+ex.InnerException);
                    Console.WriteLine("Precione Enter para continuar ingresar una nueva IP");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }
        public static string Login(string Username, string Password, int LoginTimeOutMs, TcpClient tc)
        {
            Console.WriteLine("Estableciendo Conexion...");

            int oldTimeOutMs = TimeOutMs;
            TimeOutMs = LoginTimeOutMs;
            string s = Read(tc);
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no login prompt");
            WriteLine(Username, tc);

            s += Read(tc);
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no password prompt");
            WriteLine(Password,tc);

            s += Read(tc);
            TimeOutMs = oldTimeOutMs;
            return s;
        }
        public static string Read(TcpClient tc)
        {
            if (!tc.Client.Connected) return null;
            StringBuilder sb = new StringBuilder();
            do
            {
                ParseTelnet(sb, tc);
                System.Threading.Thread.Sleep(TimeOutMs);
            } while (tc.Client.Available > 0);
            return sb.ToString();
        }
        public static void WriteLine(string cmd, TcpClient tc)
        {
            Write(cmd + "\n", tc);
        }
        public static void Write(string cmd, TcpClient tc)
        {
            if (!tc.Client.Connected) return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
            tc.GetStream().Write(buf, 0, buf.Length);
        }
        public static void ParseTelnet(StringBuilder sb, TcpClient tcpSocket)
        {
            while (tcpSocket.Client.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }
        public static string ConfirmarIp(string ipreco, Mensajes Msj)
        {
            bool ConfirmarIp = false;
            while (!ConfirmarIp) 
            {
                Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                MatchCollection result = ip.Matches(ipreco);

                if (result.Count <= 0)
                {
                    Console.WriteLine(Msj.IpSinFormato());
                    ipreco = Console.ReadLine();
                }
                else 
                {
                    ConfirmarIp = true;
                }
            }
            return ipreco;
        }
    }
}
