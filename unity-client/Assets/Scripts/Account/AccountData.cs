using Networking.Web;
using Static;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace Account
{
    public class AccountData
    {
        public const string EMAIL_KEY = "email";
        public const string PASSWORD_KEY = "password";
        private const string UndefinedName = "Undefined";

        public static int CurrentCharId;
        public static int AccountId;
        public static int MaxCharacters;
        public static int NextCharId;

        public static bool SignedIn;
        public static string PlayerName = UndefinedName;
        private static string _email;
        private static string _password;

        private static readonly Dictionary<int, ClassStats> _ClassStats = new();
        public static readonly ReadOnlyDictionary<int, ClassStats> ClassStats = new(_ClassStats);
        private static readonly List<CharacterStats> _Characters = new();
        public static ReadOnlyCollection<CharacterStats> Characters = new(_Characters);
        private static readonly List<NewsInfos> _News = new();
        public static ReadOnlyCollection<NewsInfos> News = new(_News);

        public static string GetEmail() => _email;
        public static string GetPassword() => EncryptionUtils.ToSHA256(_password);
        /// <summary>
        /// Try login on startup with saved data
        /// </summary>
        public static void TryLoginWithSavedData() {
            var email = PlayerPrefs.GetString(EMAIL_KEY);
            var pass = PlayerPrefs.GetString(PASSWORD_KEY);

            Requests.TryLogin(email, pass);
        }
        /// <summary>
        /// Called when we get a success response from server
        /// </summary>
        public static void OnSuccessfulLogin(string email, string pass) {
            Reset();

            SignedIn = true;

            _email = email;
            _password = pass;

            PlayerPrefs.SetString(EMAIL_KEY, email);
            PlayerPrefs.SetString(PASSWORD_KEY, GetPassword());
        }
        public static void LoadFromCharList(XElement data) {
            _Characters.Clear();
            MaxCharacters = data.ParseInt("@maxNumChars");
            NextCharId = data.ParseInt("@nextCharId");
            ParseAccountXml(data.Element("Account"));
            _Characters.Clear();
            foreach (var charXml in data.Elements("Char")) {
                _Characters.Add(new CharacterStats(charXml));
            }
            _News.Clear();
            XElement news = data.Element("News");
            foreach (var newsItem in news.Elements("Item")) {
                _News.Add(new NewsInfos(newsItem));
            }
        }

        private static void ParseAccountXml(XElement xml) {
            PlayerName = xml.ParseString("Name");
            AccountId = xml.ParseInt("AccountId", -1);

            //if (xml.Element("Guild") != null) {
            //    Guild = new GuildInfo(xml.Element("Guild"));
            //}
            //else Guild = GuildInfo.None;

            ParseStatsXml(xml.Element("Stats"));
        }
        private static void ParseStatsXml(XElement xml) {
            Currencies.CurrentFame = xml.ParseInt("Fame");
            Currencies.CurrentGold = xml.ParseInt("Credits");
            Currencies.TotalFame = xml.ParseInt("TotalFame");
            Currencies.TotalGold = xml.ParseInt("TotalCredits");

            _ClassStats.Clear();
            foreach (var classStatXml in xml.Elements("ClassStats")) {
                var classType = classStatXml.ParseInt("@objectType");
                _ClassStats[classType] = new ClassStats(classStatXml);
            }
        }

        public static void Reset() {
            SignedIn = false;
            PlayerName = UndefinedName;
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
