using Networking.Web;
using Static;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace Account
{
    public class AccountData
    {
        public const string EMAIL_KEY = "email";
        public const string PASSWORD_KEY = "password";

        public static int CurrentCharId;
        public static int AccountId;
        public static int MaxCharacters;
        public static int NextCharId;

        private static string _email;
        private static string _password;

        private static readonly Dictionary<int, ClassStats> _ClassStats = new();
        public static readonly ReadOnlyDictionary<int, ClassStats> ClassStats = new(_ClassStats);
        private static readonly List<CharacterStats> _Characters = new();
        public static ReadOnlyCollection<CharacterStats> Characters = new(_Characters);
        private static List<NewsInfos> _News = new();
        public static ReadOnlyCollection<NewsInfos> News = new(_News);

        public static string GetEmail() => _email;
        public static string GetPassword() => EncryptionUtils.ToSHA256(_password);
        /// <summary>
        /// Try login with user data when we are not logged in and login button is clicked
        /// </summary>
        public static void LoginWithUserData(string email, string pass)
        {

        }
        /// <summary>
        /// Try login on startup with saved data
        /// </summary>
        public static void TryLoginWithSavedData()
        {
            var email = PlayerPrefs.GetString(EMAIL_KEY);
            var pass = PlayerPrefs.GetString(PASSWORD_KEY);
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass)) {
                Utils.Log("NoEmailOrPassSaved");
                return;
            }

            var req = new VerifyHandler(email, pass);
            WebController.WorkQueue.Enqueue(new WebWork(req));
        }
        /// <summary>
        /// Called when we get a success response from server
        /// </summary>
        public static void OnSuccessfulLogin(string email, string pass)
        {
            Reset();

            _email = email;
            _password = pass;

            PlayerPrefs.SetString(EMAIL_KEY, email);
            PlayerPrefs.SetString(PASSWORD_KEY, GetPassword());
        }
        public static void LoadFromCharList(XElement data)
        {
            _Characters.Clear();


        }
        public static void Reset()
        {
            _email  = "";
            _password = "";
            AccountId = -1;
            MaxCharacters = 1;
            CurrentCharId = 0;
            _ClassStats.Clear();
            _Characters.Clear();
            PlayerPrefs.DeleteKey(EMAIL_KEY);
            PlayerPrefs.DeleteKey(PASSWORD_KEY);
        }
    }
}
