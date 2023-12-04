using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Web
{
    public class WebSender
    {
        private static readonly HttpClient _HttpClient = new() {
            Timeout = TimeSpan.FromMilliseconds(1000)
        };
        public static async Task<WebResponse> SendWebRequest(string url, Dictionary<string, string> kvp, WebDataType dataType = WebDataType.Xml) {
            var content = new FormUrlEncodedContent(kvp);
            return await SendPostRequestAsync(url, content, dataType);
        }
        private static async Task<WebResponse> SendPostRequestAsync(string request, FormUrlEncodedContent content, WebDataType dataType = WebDataType.Xml) {
            Utils.Log("SendingAPIRequest::{0}", request);
            try {
                
                var response = await _HttpClient.PostAsync(Settings.API_ADDRESS + request, content);
                Utils.Log("HandlingResponse::DataType::{0}", dataType);
                
                var textResponse = await response.Content.ReadAsStringAsync();
                Utils.Log("HandlingResponse::{0}::{1}::{2}", response.StatusCode, response.Content, textResponse);

                if (!response.IsSuccessStatusCode) {
                    Utils.Error($"HandlingResponse::Error::{textResponse}");
                    var errorXml = StringUtils.AddErrorTag(response.StatusCode);
                    return new WebResponse(WebResult.Error, WebDataType.Xml, errorXml);
                }

                return new WebResponse(WebResult.Success, dataType, textResponse);
            }
            catch (Exception) {
                return new WebResponse(WebResult.FailedToConnect, WebDataType.Xml);
            }
        }

        
    }
    public readonly struct WebResponse
    {
        public readonly WebResult Result;
        public readonly string Reply;
        public readonly WebDataType Type;
        public WebResponse(WebResult result, WebDataType type, string reply = "")
        {
            Result = result;
            Type = type;
            Reply = reply;
        }

        public string ResultToString() {
            return
                Result switch
            {
                WebResult.Success => nameof(WebResult.Success),
                WebResult.FailedToConnect => nameof(WebResult.FailedToConnect),
                WebResult.Error => nameof(WebResult.Error),
                _ => string.Empty,
            };
        }
    }

    public enum WebResult
    {
        Success,
        Error,
        FailedToConnect,
    }

    public enum WebDataType
    {
        String,
        Xml,
        Json,
    }
}
