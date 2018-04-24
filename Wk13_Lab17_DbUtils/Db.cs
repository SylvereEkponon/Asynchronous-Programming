using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wk13_Lab17_DbUtils
{
    public static class Db
    {
        static Random rand = new Random();
        static List<string> columns;
        static List<int> values;

        public static void InitDb(List<string> columns)
        {
            Db.columns = columns;
            values = new List<int>();
            foreach (var col in columns)
                values.Add(rand.Next(80, 120));
        }

        //this method returns promptly
        public static int Get(string column)
        {
            int index = columns.FindIndex(x => x == column);
            int result = values[index];
            values[index] += rand.Next(-10, 10);
            return result;
        }

        //this method returns after 5 seconds
        public static int GetWithDelay(string column)
        {
            Thread.Sleep(5000);
            return Get(column);
        }

        //this method returns after 5 seconds, but it does not block the current thread
        async public static Task<int> GetAsync(string column)
        {
            await Task.Delay(5000);
            return await Task.Run<int>(() => Get(column));
        }

        //this method returns after 5 second, and it provides a mechanism to cancel the operation
        //before the 5 seconds. It checks the cancellation token 20 times every 300 ms
        async public static Task<int> GetAsync(string column, CancellationToken ct)
        {
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(250);                      //pauses for quarter seconds
                ct.ThrowIfCancellationRequested();          //
            }
            return await Task.Run<int>(() => Get(column));
        }

        //this method returns after 5 second, and it provides a mechanism to report on the state of 
        //completion within the 5 seconds. It reports on  progress 20 times every 300 ms
        async public static Task<int> GetAsync(string column, IProgress<int> progress)
        {
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(250);
                progress.Report((i + 1) * 5);
            }
            return await Task.Run<int>(() => Get(column));
        }
    }
}
