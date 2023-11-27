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

                var response = await _HttpClient.PostAsync(Settings.IP_ADDRESS + request, content);
                Utils.Log($"HandlingResponse::DataType::{dataType}");
                var textResponse = await response.Content.ReadAsStringAsync();

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
