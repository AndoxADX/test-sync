using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Models;
using TodoApi.ViewModels;
using WENShared.Lib.IProvider;
using WENShared;
using System.Threading;
using WENShared.Lib.Model;
using Microsoft.Extensions.DependencyInjection;

namespace TodoApi.Store
{

    public class TrxScheduler
    {
        // public readonly TodoContext _context;
        private readonly IServiceProvider _provider;

        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContext;

        public readonly HttpClient _client;

        public TrxScheduler(
            // TodoContext context,
            IServiceProvider provider,
            ILogger<TrxScheduler> logger,
            IHttpContextAccessor httpContext
        // HttpClient client
        )
        {
            // _context = context;
            _provider = provider;
            _logger = logger;
            _httpContext = httpContext;
            _client = new HttpClient();
        }

        public async Task RunScheduledTask(CancellationToken cancellationToken)
        {

            int code = -1;
            var startTimeUTC = DateTime.UtcNow;
            int successCount = 0;
            do
            {
                if (cancellationToken.IsCancellationRequested || successCount >= 100)
                    break;
                code = await ProcessTransaction(cancellationToken);
                if (code == 0)
                    successCount++;
            } while (code == 0);

            var timespan = DateTime.UtcNow - startTimeUTC;
            if (successCount > 0)
                _logger.LogInformation("[{datetime}] Completed: {count}, in {timespan} seconds. Exit code {code}",
                DateTime.Now.ToShortTimeString(),
                successCount.ToString().PadLeft(4, ' '),
                timespan.TotalSeconds.ToString("#.00").PadLeft(6, ' '), code);
        }

        public async Task<ReqTransaction> GetUnprocessedReqTrx(TodoContext _context)
        {
            var reqTrx = await _context.reqtransactions
                .FirstOrDefaultAsync(x => x.IsProcess == false);
            return reqTrx;
        }

        public async Task<string> CreateTrade(ReqTransaction rtrx)
        {
            try
            {
                var baseUrl = "http://localhost:5100/api";
                var outTradeId = GenerateNonce();

                var isTrade = rtrx.Type == "PTrade";

                if (isTrade)
                {
                    var model = new TradeCreateViewModel
                    {
                        OutTradeId = outTradeId,
                        CoinCode = "ETH_LLC",
                        TotalAmount = rtrx.Amount,
                        Subject = rtrx.Remark,
                        SubjectDetail = rtrx.Id,
                        NotifyUrl = "",
                        AccountId = rtrx.Type == "PTrade" ? int.Parse(rtrx.AccountFrom) : int.Parse(rtrx.AccountTo)
                    };
                    var httpRequestMessage = await GenerateWalletPostRequest(baseUrl, "/v2/Gateway/Trade/CreateAndConfirmV3", model, null);

                    using (var response = await _client.SendAsync(httpRequestMessage))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            var errorstring = await response.Content.ReadAsStringAsync();
                            _logger.LogDebug(errorstring);
                            var error = await response.Content.ReadAsStringAsync();
                            throw new Exception(error);
                        }
                    }

                }
                else
                    return "";

            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Gateway Trade creation failed for User:");
                throw new Exception("Deposit error");
            }
        }

        public async Task<string> CreateGift(ReqTransaction rtrx)
        {
            try
            {
                var baseUrl = "http://localhost:5100/api";
                var outGiftId = GenerateNonce();

                var isTrade = rtrx.Type == "PTrade";
                var isGift = rtrx.Type == "PGift";

                var isError = !isTrade && !isGift || isTrade && isGift;

                if (isError)
                    throw new Exception("Unknown Trade Entry Type");

                if (isGift)
                {
                    var model = new GiftCreateViewModel
                    {
                        OutGiftId = outGiftId,
                        CoinCode = "ETH_LLC",
                        TotalAmount = rtrx.Amount,
                        Subject = rtrx.Remark,
                        SubjectDetail = rtrx.Id,
                        NotifyUrl = null,
                        UserId = null,
                        AccountId = rtrx.Type == "PGift" ? int.Parse(rtrx.AccountFrom) : int.Parse(rtrx.AccountTo)
                    };
                    var httpRequestMessage = await GenerateWalletPostRequest(baseUrl, "/v2/Gateway/Gift/CreateAndConfirmV3", model, null);

                    using (var response = await _client.SendAsync(httpRequestMessage))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            var errorstring = await response.Content.ReadAsStringAsync();
                            _logger.LogDebug(errorstring);
                            var error = await response.Content.ReadAsStringAsync();
                            throw new Exception(error);
                        }
                    }

                }
                return "";

            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Gateway Gift creation failed for User:");
                throw new Exception("Deposit error");
            }
        }

        // user to system only.
        public async Task CreateTransaction(TodoContext _context, ReqTransaction rtrx)
        {
            switch (rtrx.EntryType)
            {
                case "credit":
                    // Create Gateway Trade

                    try
                    {
                        var trade = await CreateTrade(rtrx);
                        // create trx
                        // _context.AddAsync(new Transaction
                        // {
                        //     Id = rtrx.Id,
                        //     Amount = rtrx.Amount,
                        //     AccountFrom = rtrx.AccountFrom,
                        //     AccountTo = rtrx.AccountTo,
                        //     Date = DateTime.UtcNow,
                        //     Remark = rtrx.Remark,
                        //     EntryType = rtrx.EntryType,
                        //     Type = rtrx.Type
                        // });
                        // if confirm trade at wallet.
                        // then

                        rtrx.IsSuccess = true;
                    }
                    catch (Exception e)
                    {
                        rtrx.IsSuccess = false;
                    }
                    break;

                case "debit":
                    try
                    {
                        var gift = await CreateGift(rtrx);
                        rtrx.IsSuccess = true;
                    }
                    catch (Exception e)
                    {
                        rtrx.IsSuccess = false;
                    }
                    // create trx
                    // _context.AddAsync(new Transaction
                    // {
                    //     Id = rtrx.Id,
                    //     Amount = rtrx.Amount,
                    //     AccountFrom = rtrx.AccountFrom,
                    //     AccountTo = rtrx.AccountTo,
                    //     Date = DateTime.UtcNow,
                    //     Remark = rtrx.Remark,
                    //     EntryType = rtrx.EntryType
                    // });

                    // if confirm trade at wallet.
                    // then

                    break;
                default:
                    throw new Exception("Trx Type not found!");
            }

            rtrx.IsProcess = true;
            _context.reqtransactions.Update(rtrx);

            await _context.SaveChangesAsync();

        }


        public async Task<HttpRequestMessage> GenerateWalletPostRequest(string baseUrl, string path, object model, IHttpContextAccessor context)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, baseUrl + path)
            {
                Content = new ObjectContent<object>
                   (model, new JsonMediaTypeFormatter(), (MediaTypeHeaderValue)null),
            };
            // var token = await context.HttpContext.GetTokenAsync("Bearer", "access_token");
            // httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            if (context != null)
            {

            }
            httpRequestMessage.Headers.Add("Accept", "application/json");
            return httpRequestMessage;
        }

        public async Task<int> ProcessTransaction(CancellationToken cancellationToken)
        {
            using (var scope = _provider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<TodoContext>();
                var trx = await GetUnprocessedReqTrx(_context);
                if (trx == null)
                    return -1;
                try
                {

                await CreateTransaction(_context, trx);
                }catch (Exception ex)
                {
                    throw new Exception("Died");
                }
                return 1;
            }
        }
        public async Task<int> TrxV3(TodoContext _context, CreateTrxModel model)
        {
            await _context.AddAsync(new ReqTransaction
            {
                Id = model.Id,
                Amount = model.Amount,
                Date = DateTime.UtcNow,
                EntryType = model.Type,
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
