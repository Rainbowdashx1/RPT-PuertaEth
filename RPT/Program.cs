using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace RPT
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain myDomain = Thread.GetDomain();
            myDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal myPrincipal = (WindowsPrincipal)Thread.CurrentPrincipal;
            myPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

            Mensajes Msj = new Mensajes();
            Console.WriteLine(Msj.MensajeEncabezado());
            Console.WriteLine(Msj.MenuInicial());
            Thread[] workerThreads;
                
            string IpRecolectada = Console.ReadLine();
            //IpRecolectada = ConfirmarIp(IpRecolectada, Msj);

            List<string> ListadoDeIps = IpRecolectada.Split(',').ToList();
            workerThreads = new Thread[ListadoDeIps.Count];
            int i = 0;

            foreach (string ip in ListadoDeIps) 
            {
                Work w = new Work(ip,i);
                ThreadStart st = new ThreadStart( w.TrabajoTelnet );
                workerThreads[i] = new Thread(st);
                workerThreads[i].Start();
                i++;
            }
        }
    }
}
