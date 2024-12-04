// ************************************************************************
// Practica 07
// Joselyn Martinez
// Fecha de realización: 27/11/2024
// Fecha de entrega: 04/12/2024
// ********************************
// Conclusiones:
// * El uso de herramientas como Visual Studio y GitHub refuerza la integración entre el entorno de desarrollo y el control
//   de versiones. La práctica muestra cómo administrar el flujo de trabajo de desarrollo de forma estructurada, especialmente
//   cuando se trabaja con métodos de integración continua y colaboración remota, elementos que son fundamentales en proyectos
//   de software reales.
// * Se puede denotar que en el servidor se usa hilos para manejar conexiones simultáneas de los clientes. Aunque esto permite que
//   el servidor responda de manera concurrente a múltiples clientes, este enfoque no es el más eficiente, especialmente si el número
//   de conexiones aumenta significativamente. 
// Recomendaciones:
// * Se recomienda documentar adecuadamente los cambios realizados en el código, como los comentarios y encabezados solicitados en la
//   práctica. Asegurarse de que cada cambio esté claramente explicado ayuda a otros a entender la lógica detrás de las modificaciones. 
// * Es esencial mejorar la seguridad del sistema, particularmente en la autenticación de usuarios y la transmisión de datos. Se
//   recomienda implementar mecanismos de autenticación más robustos para tener myor seguridad que la información sensible no pueda
//   ser fácilmente obtenida por atacantes.
// ************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>();
         

        static void Main(string[] args)
        {
            try
            {
                // Se crea un TcpListener en el puerto 8080 y se inicia la escucha de conexiones
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor inició en el puerto 5000...");

                // Se entra en un ciclo infinito para aceptar conexiones de los clientes.
                while (true)
                {
                    // Acepta una nueva conexión de un cliente
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());

                    // Se crea un nuevo hilo para manejar la conexión de este cliente
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al iniciar el servidor: " + ex.Message);
            }
            finally
            {
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;
            try
            {
                // Se obtiene el flujo de datos del cliente para leer y escribir mensajes
                flujo = cliente.GetStream();
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                // Instanciamos un objeto Protocolo
                // Protocolo protocolo = new Protocolo();
                Protocolo.Protocolo protocolo = new Protocolo.Protocolo();

                // Obtener la dirección del cliente
                string direccionCliente = cliente.Client.RemoteEndPoint.ToString();

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    // Pedido pedido = Pedido.Procesar(mensajeRx);
                    Pedido pedido = protocolo.CrearPedido1(mensajeRx); // Usar el método de la clase Protocolo
                    Console.WriteLine("Se recibio: " + pedido);
                    // Console.WriteLine("Se recibió: " + mensajeRx);


                    // Respuesta respuesta = ResolverPedido(pedido, direccionCliente);

                    Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(pedido, direccionCliente);
                    Console.WriteLine("Se envió: " + respuesta);

                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }


        /*
        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;
            try
            {
                flujo = cliente.GetStream();
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    Pedido pedido = Pedido.Procesar(mensajeRx);
                    Console.WriteLine("Se recibio: " + pedido);

                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();
                    Respuesta respuesta = ResolverPedido(pedido, direccionCliente);
                    Console.WriteLine("Se envió: " + respuesta);

                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }
        */
        /*
        private static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 && pedido.Parametros[0] == "root" && pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Random().Next(2) == 0
                            ? new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = "ACCESO_CONCEDIDO"
                            }
                            : new Respuesta
                            {
                                Estado = "NOK",
                                Mensaje = "ACCESO_NEGADO"
                            };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        if (ValidarPlaca(placa))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };
                            ContadorCliente(direccionCliente);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = listadoClientes[direccionCliente].ToString()
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }

            return respuesta;
        }

        */
        /*

        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }

        private static void ContadorCliente(string direccionCliente)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++;
            }
            else
            {
                listadoClientes[direccionCliente] = 1;
            }
        }
        */

    }
}