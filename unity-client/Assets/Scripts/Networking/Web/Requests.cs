using Account;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Web
{
    public static class Requests
    {
        public static Action<bool> OnLoginResult;

        /// <summary>
        /// Send Verify Request, Automatically sends CharListRequest if Successful
        /// </summary>
        public static void TryLogin(string email, string pass) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass)) {
                Utils.Error("NoEmailOrPassSaved");
                return;
            }

            new VerifyHandler(email, pass).Enqueue();
        }
        /// <summary>
        /// Send CharList Request
        /// </summary>
        public static void TryCharList() {
            //Only send request if our Verify request was successful
            if (!AccountData.SignedIn) {
                Utils.Error("NotSignedIn");
                return;
            }

            new CharListHandler().Enqueue();
        }
        /// <summary>
        /// Send register request, Automatically sends char list request if successful
        /// </summary>
        public static void TryRegister(string email, string pass, string name) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass)) {
                Utils.Error("NoEmailOrPassSaved");
                return;
            }

            new RegisterHandler(email, pass, name).Enqueue();
        }
    }
}
