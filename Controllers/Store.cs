using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoApi.Models;
using TodoApi.ViewModels;

namespace TodoApi.Store
{

    public class TestStore
    {
        public readonly TodoContext _context;
        private readonly ILogger _logger;
        private static readonly Random Global = new Random();
        public TestStore(TodoContext context,
            ILogger<TestStore> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        // public async Task<int> TestTrx(decimal amount, string remark)
        // {
        //     var id = await GetId();
        //     var sum = _context.Transactions.Sum(x => x.Amount);
        //     await _context.AddAsync(new Transaction
        //     {
        //         Id = id,
        //         Amount = amount,
        //         Remark = remark,
        //     });
        //     await _context.SaveChangesAsync();
        //     _logger.LogDebug("Insufficient: {id},{amount},{sum}.", id, amount, sum);

        //     return 1;
        // }

        // public async Task<int> TestFail(string id, decimal amount, string remark)
        // {
        //     var a = 0;
        //     try
        //     {

        //         if (amount < 0)
        //         {
        //             var sum = _context.Transactions.Sum(x => x.Amount);
        //             // if (amount < sum)
        //             //     throw new Exception("Insufficient amount !");
        //             a = 1;
        //             if (amount < sum)
        //             {
        //                 a = -1;
        //                 _logger.LogDebug("Insufficient: {id},{amount},{sum}.", id, amount, sum);
        //             }
        //         }
        //         else
        //         {
        //             a = 1;
        //         }
        //         await _context.AddAsync(new Transaction
        //         {
        //             Id = id,
        //             Amount = amount,
        //             Remark = remark,
        //             IsComplete = false
        //         });
        //         await _context.SaveChangesAsync();
        //         var sum2 = _context.Transactions.Sum(x => x.Amount);
        //         _logger.LogDebug("Trx Created: {id}. Sum: {sum}.", id, sum2);

        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogDebug("Error: {id}, msg: {ex}.", id, ex);

        //     }
        //     return a;
        // }
        // public async Task<int> Trx2(decimal amount, string remark)
        // {
        //     // var id = await GetId();
        //     var sum = _context.Transactions.Sum(x => x.Amount);
        //     if (sum < amount)
        //     {
        //         _logger.LogDebug("Insufficient: {amount},{sum}.", amount, sum);
        //         throw new Exception("Insufficient amount!");
        //     }

        //     await _context.AddAsync(new Transaction
        //     {
        //         // Id = id,
        //         Amount = amount,
        //         Remark = remark,
        //         IsComplete = false
        //     });
        //     await _context.SaveChangesAsync();

        //     _logger.LogDebug("Insufficient: {amount},{sum}.", amount, sum);

        //     return 1;
        // }

        // //Function to get a random number 
        // private static readonly Random random = new Random();
        // private static readonly object syncLock = new object();
        // public static int RandomNumber(int min, int max)
        // {
        //     lock (syncLock)
        //     { // synchronize
        //         return random.Next(min, max);
        //     }
        // }

        // private static rnd = new Random();
        public static int GetRandom()
        {
            return Global.Next(1, 5);
        }

        public async Task<int> TrxV3(CreateTrxModel model)
        {
            // var userIds = new[] {
            //     (1,"1110"),
            //     (2,"1113"),
            //     (3,"1243"),
            //     (4,"1340"),
            //     (5,"1360")
            // };

            // var num = GetRandom();
            // var user = userIds[num - 1].Item2;
            await _context.AddAsync(new ReqTransaction
            {
                Id = model.Id,
                Amount = model.Amount,
                AccountFrom = model.AccountFrom,
                AccountTo = "1379",
                Date = DateTime.UtcNow,
                EntryType = model.EntryType,
                Type = model.Type,
                Remark = model.Remark,
                IsProcess = false,
                IsSuccess = false

            });
            await _context.SaveChangesAsync();

            _logger.LogDebug("Trx created:{id} {amount}.", model.Id, model.Amount);

            return 1;
        }
        public async Task<string> GetId()
        {
            long elapsedTicks = DateTime.Now.Ticks - new DateTime(2015, 1, 1).Ticks; // unique time lapsed;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

            string hexValue = elapsedSpan.Ticks.ToString("X2");
            string rand = GenerateNonce();
            hexValue = hexValue.Insert(5, "-" + rand + "-");

            return "T" + hexValue;

        }

        private static string GenerateNonce()
        {
            //Allocate a buffer
            var ByteArray = new byte[2];
            //Generate a cryptographically random set of bytes
            using (var Rnd = RandomNumberGenerator.Create())
            {
                Rnd.GetBytes(ByteArray);
            }
            return BitConverter.ToString(ByteArray).Replace("-", "");
        }

    }
}
