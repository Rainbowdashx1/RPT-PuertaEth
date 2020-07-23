
namespace RPT
{
    public class Mensajes
    {
        public string MensajeEncabezado() 
        {
            return ""
                  +"██████╗░██████╗░████████╗\n"
                  + "██╔══██╗██╔══██╗╚══██╔══╝\n"
                  + "██████╔╝██████╔╝░░░██║░░░\n"
                  + "██╔══██╗██╔═══╝░░░░██║░░░\n"
                  + "██║░░██║██║░░░░░░░░██║░░░\n"
                  + "╚═╝░░╚═╝╚═╝░░░░░░░░╚═╝░░░\n";
        }

        public string MenuInicial()
        {
            return "**************************\n"
                   + "1.- Ingresar Ip para extracción\n"
                   + "**************************";
        }

        public string IpSinFormato()
        {
            return "**************************\n"
                   + "Ip no tiene formato correcto ingrese otra \n"
                   + "**************************";
        }

    }
}
