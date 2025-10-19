using FAST.FBasicInteractiveConsole.TestCode;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.Design;

namespace FAST.FBasicInteractiveConsole
{
    /// <summary>
    /// Do not pay attention on this class
    /// </summary>
    internal class TPRunner
    {
        public void Run()
        {
            string lastCommand = "?";
            while (true)
            {
                Console.Write("Test programs [enter command: Q=Exit] :");
                string iCommand = Console.ReadLine()!;
                iCommand = iCommand.ToUpper();
                if (iCommand == "X" | iCommand == "Q") break;
                try
                {
                    switchAgain:
                    switch (iCommand)
                    {
                        case "?":
                            help();
                            break;
                        case "R":
                            iCommand = lastCommand;
                            if (iCommand=="R") iCommand="?"; // prevent looping
                            goto switchAgain;

                        case "J":
                            Console.WriteLine("Jagged Array Test");
                            new JaggedArray_Test().Run();
                            break;

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION:");
                    Console.WriteLine(ex);
                    Console.WriteLine();
                }

                if (!string.IsNullOrWhiteSpace(lastCommand)) lastCommand = iCommand;
            }

        }

        public void help()
        {
            Console.WriteLine();
            Console.WriteLine("?      : This help lines");
            Console.WriteLine("R      : Repeat last command");
            Console.WriteLine("Q | X  : Return to interactive console");
            Console.WriteLine();
            Console.WriteLine("J      : Jagged Array Testing code");
           
            Console.WriteLine();

        }
    }
}
