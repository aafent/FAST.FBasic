using FAST.FBasic.InteractiveConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("FBasicSettings.settings", optional: true, reloadOnChange: true);
    });
var host = builder.Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
FBasicIC FBASIC = new(config);

FBASIC.welcome();
string iCommand = "R"; //R
while (true)
{
    Console.WriteLine("==========================================[(v) TEST====]\n");

    try
    {
        FBASIC.run(iCommand);
    }
    catch (Exception ex)
    {
        Console.WriteLine("EXCEPTION:");
        Console.WriteLine(ex);
        Console.WriteLine();
    }

    Console.WriteLine("==========END========[enter R=to rerun, Q=Exit, H=Help....]");
    iCommand = Console.ReadLine();
    iCommand = iCommand.ToUpper();
    if (iCommand == "R") continue;
    if (iCommand == "X" | iCommand == "Q") break;

}
