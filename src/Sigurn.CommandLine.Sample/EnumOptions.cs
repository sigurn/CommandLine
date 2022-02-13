using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigurn.CommandLine.Sample
{
    enum EnumValue
    {
        None,
        Yes,
        No,
        Maybe,
        NotSure
    }

    [Flags]
    enum EnumFlags
    {
        None = 0,
        Flag1 = 1,
        Flag2 = 2,
        Flag4 = 4
    }

    internal class EnumOptions
    {
        [Option(IsRequired = true)]
        [HelpText("Enum value of your choice")]
        public EnumValue Choice { get; set; }

        [Option]
        [HelpText(
            "Enum flags. Sevral flags can be used.",
            "Use plus sign to join several flags or separate them by space")]
        public EnumFlags Flags { get; set; } = EnumFlags.Flag1 | EnumFlags.Flag4;

        [CommandName("enum")]
        [HelpText(
            "Test enum values.",
            "See help and choose different enum values to check them.",
            "It also supports enum flags. You can use them as array values or use plus sign to join several flags."
            )]
        public static void TestEnum(EnumOptions options)
        {
            Console.WriteLine($"Your choice: {options.Choice}");
            Console.WriteLine($"Flags: {options.Flags}");
        }
    }
}
