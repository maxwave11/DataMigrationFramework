using System.Linq;
using System.Text;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Utils;

namespace SampleMigrationApp.PipelineCommands
{
    /// <summary>
    /// Transition description: Column `Tarkistuskk` will contain integers (between 1 - 12), 
    /// separated by comma (if more than one). Create a string that is initially 12 chars of zeros, i.e. > `000000000000`. 
    /// Then change that string so that for each unique number in `Tarkistuskk` source string turn 
    /// the 0 in the string to 1 of the position determined by the source string. 
    /// For example if source string is "11", > turn the 11th char in the string to 1 (i.e. `000000000010`). 
    /// If the source string is "5,11" > turn the 5th and 11th char to 1 (i.e. `000010000010`). 
    /// If the source string is null > set the string to `100000000000` (and we should record comment to
    ///  data transfer log that "lease indexation information was missing")
    /// </summary>
    public class ToRecurrancePattern : CommandBase
    {
        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            //suppose that default value is 1 (january)
            var value = ctx.TransitValue?.ToString().IsNotEmpty() == true ? ctx.TransitValue.ToString() : "1";
            var months = value.Replace(".",",").Split(',').Where(i=>i.IsNotEmpty()).Select(int.Parse).ToList();

            StringBuilder sb = new StringBuilder("000000000000");

            foreach (var month in months)
            {
                sb[month - 1] = '1';
            }

            ctx.SetCurrentValue(sb.ToString());
        }
    }
}
