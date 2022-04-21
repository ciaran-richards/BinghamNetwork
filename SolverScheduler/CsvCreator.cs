using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using MainSolver;

namespace SolverScheduler
{
    class CsvCreator
    {

        public bool CreateCsv(List<IsoResultStruct> results, string name)
        {

            string filePath = Paths.Results + "\\" + name + ".csv"; 
            try
            {
                using (TextWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
                    csv.WriteRecords(results); // where values implements IEnumerable
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        public bool CreateCsv(List<FlowResultStruct> results, string name)
        {

            string filePath = Paths.Results + "\\" + name + ".csv";
            try
            {
                using (TextWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
                    csv.WriteRecords(results); // where values implements IEnumerable
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

    }
}
