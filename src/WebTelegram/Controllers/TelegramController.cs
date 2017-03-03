using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Extensions;
using Microsoft.AspNetCore.Mvc;
using ServiceReference_RussianPost;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Update = WebTelegram.Models.Update;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTelegram.Controllers
{
    [Route("/[controller]")]
    public class TelegramController : Controller
    {
        // GET: api/values
        [HttpGet]
        public string Get()
        {
	        return "Служба запущена!";
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] Update request)
        {
			GetRequestFromTelegram(request);
			
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

		public async void GetRequestFromTelegram(Update request)
		{
			if (request.message == null) { return; }

			var bot = new TelegramBotClient("352425198:AAGm-foe_-29YU2067-MblBWwod3PFjF8q0");
			switch (request.message.text)
			{
				case "/start":
					string caption = "Вас приветствует Бот отслеживания почтовых отправлений!\nВведите номер почтового отправления:";
					await bot.SendTextMessageAsync(request.message.chat.id, caption);
					break;
				default:
					//логика обработки трека
					try
					{
						Task<getOperationHistoryResponse> response = GetRussianPost(request.message.text);
						SendMessage(bot, request, response.Result);
					}
					catch (Exception ex)
					{
						caption = ex.Message;
						await bot.SendTextMessageAsync(request.message.chat.id, caption);
					}
					break;

			}

			

			
		}

	    public async Task<getOperationHistoryResponse> GetRussianPost(string barcode)
	    {
			OperationHistoryRequest ohr = new OperationHistoryRequest();
			ohr.Barcode = barcode;
			ohr.MessageType = 0;

			AuthorizationHeader ah = new AuthorizationHeader();
			ah.login = "xOmeLwsajuASmo";
			ah.password = "R6K4avbste01";


			getOperationHistoryRequest req = new getOperationHistoryRequest(ohr, ah);

			getOperationHistoryResponse resp = new getOperationHistoryResponse(new OperationHistoryRecord[100]);
			OperationHistory12 history = new OperationHistory12Client(OperationHistory12Client.EndpointConfiguration.OperationHistory12Port);

			return resp = await history.getOperationHistoryAsync(req);

		}

	    public async  void SendMessage(TelegramBotClient bot, Update request, getOperationHistoryResponse response)
	    {
			long idChat = request.message.chat.id;

			List<OperationHistoryRecord> operationList = new List<OperationHistoryRecord>(response.OperationHistoryData);
		    //operationList.Reverse();

			foreach (var r in operationList)
		    {
				string dateTime = r.OperationParameters.OperDate.ToShortDateString() + " - " +
			                      r.OperationParameters.OperDate.ToShortTimeString();
			   
			    string typeName = r.OperationParameters.OperType.Name;
				string attrName = r.OperationParameters.OperAttr.Name;

				string operStatus = string.IsNullOrEmpty(attrName)? typeName : typeName + "-" + attrName;


			    string index = r.AddressParameters.OperationAddress.Index;
			    string discr = r.AddressParameters.OperationAddress.Description;

			    string operLocation = string.IsNullOrEmpty(index) ? discr : index + " " + discr;

				await bot.SendTextMessageAsync(idChat,
					"<b>" + dateTime + "</b>" + "\n" +
					"<pre>" + operStatus + "</pre>" + "\n" + 
					"<code>" + operLocation + "</code>", 
					true, 
					false, 
					0, 
					null, 
					ParseMode.Html);
			}

			//await bot.SendTextMessageAsync(idChat, seperator);
			string messageText = request.message.text;
			string caption = "<b>Дополнительная информация по адресу:\n" +
			                 "</b><a href=\"https://www.pochta.ru/tracking#" +
							 messageText + "\">" + messageText + "</a>";

			await bot.SendTextMessageAsync(idChat, caption, true, false, 0, null,ParseMode.Html);

			caption = "Введите номер почтового отправления:";

			await bot.SendTextMessageAsync(idChat, caption, true, false, 0, null, ParseMode.Html);
		}
	    
		
	}
	
			
	
}
