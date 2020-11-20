/*
 * Module:          Program.cs
 * Author:          Jaceey Tuck
 * Date:            March 28, 2019
 * Description:     
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using HangmanLibrary;

namespace HangmanService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;
            try
            {
                // Address
                servHost = new ServiceHost(typeof(Words));

                // Manage the service’s life cycle
                servHost.Open();
                Console.WriteLine("Service started. Press a key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
