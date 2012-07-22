using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Xml;
using System.Reflection;
using Windows.Data.Xml.Dom;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Xml.Linq;

namespace SharpMe2Day.Util
{
    public class Me2Util
    {
        /// <summary>
        /// API METHOD 종류
        /// </summary>
        public enum API_METHOD_TYPE
        {
            ME2DAY_API_NOOP,
            ME2DAY_API_METOO,
            ME2DAY_API_FRIENDSHIP,
            ME2DAY_API_GET_FRIENDS,
            ME2DAY_API_GET_LATESTS,
            ME2DAY_API_GET_POSTS,
            ME2DAY_API_GET_COMMENTS,
            ME2DAY_API_GET_METOOS,
            ME2DAY_API_GET_FRIENDSHIP_REQUESTS,
            ME2DAY_API_GET_PERSON,
            ME2DAY_API_ACCEPT_FRIENDSHIP_REQUEST,
            ME2DAY_API_CREATE_POST,
            ME2DAY_API_CREATE_COMMENT,
            ME2DAY_API_DELETE_POST
        }

        private static IDictionary<API_METHOD_TYPE, string> m_url_dict;
        private static Random rand;
        private static string API_BASE;
        private static char[] NONCE_ARRAY;

        static Me2Util()
        {
            rand = new Random(DateTime.Now.Millisecond);
            API_BASE = "http://me2day.net/api/";
            NONCE_ARRAY = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

            m_url_dict = new Dictionary<API_METHOD_TYPE, string>();

            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_NOOP, "noop");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_METOO, "metoo");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_FRIENDSHIP, "friendship");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_GET_FRIENDS, "get_friends");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_GET_LATESTS, "get_latests");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_GET_POSTS, "get_posts");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_GET_COMMENTS, "get_comments");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_GET_METOOS, "get_metoos");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_GET_FRIENDSHIP_REQUESTS, "get_friendship_requests");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_GET_PERSON, "get_person");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_ACCEPT_FRIENDSHIP_REQUEST, "accept_friendship_request");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_CREATE_POST, "create_post");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_CREATE_COMMENT, "create_comment");
            m_url_dict.Add(API_METHOD_TYPE.ME2DAY_API_DELETE_POST, "delete_post");
        }


        /// <summary>
        /// 8자리 임이의 문자열을 만들어 냅니다.
        /// </summary>
        /// <returns></returns>
        private static String GetNonce()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 8; i++)
            {
                sb.Append(NONCE_ARRAY[rand.Next(0, NONCE_ARRAY.Length)]);
            }

            return sb.ToString();
        }

        public static string MD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }

        /// <summary>
        /// ME2 API 인증 암호를 생성 합니다.
        /// </summary>
        /// <param name="key">사용자 API KEY</param>
        /// <returns></returns>
        public static String GetAuthPassword(string key)
        {
            StringBuilder sb = new StringBuilder();
            string nonce = GetNonce();
            

            sb.Append(nonce);
            sb.Append(MD5(String.Format("{0}{1}", nonce, key)));
            return sb.ToString();
        }

        /// <summary>
        /// me2DAY API 주소를 구해 옵니다
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param">사용자 id 등의 url 파라메터</param>
        /// <returns></returns>
        public static Uri GetAPIUrl(API_METHOD_TYPE type, params object[] param)
        {
            string url_base = String.Format("{0}{1}", API_BASE, m_url_dict[type]);

            if (param == null)
            {
                return new Uri(url_base);
            }
            if (param.Length > 0)
            {
                foreach (object o in param)
                {
                    url_base += "/" + o.ToString();
                }
            }

            return new Uri(url_base);
        }

        /// <summary>
        /// me2 Error XML문서를 분석 합니다
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static Me2Error ParseError(XmlDocument xml)
        {
            IXmlNode elem_error = xml.SelectSingleNode("//error");
            if (elem_error != null)
            {
                Me2Error error = new Me2Error();
                error.Code = Convert.ToInt32(elem_error.SelectSingleNode("code").InnerText);
                error.Message = elem_error.SelectSingleNode("message").InnerText;
                error.Description = elem_error.SelectSingleNode("description").InnerText;

                return error;
            }
            else
                return null;

        }

        /// <summary>
        /// XML 형태로 떨어지는 문서에서 모델 객체를 생성 합니다.
        /// </summary>
        /// <typeparam name="T">can be from Comment,Person,Post,Tag</typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static T FromXml<T>(IXmlNode root)
        {
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();

            foreach (XNode elem in root.ChildNodes)
            {
                PropertyInfo pi = type.GetRuntimeProperty(elem.ToString());
                if (pi != null)
                {
                    object setValue = elem.ToString();
                    if (pi.PropertyType == typeof(Int32))
                    {
                        setValue = Convert.ToInt32(elem.ToString());

                    }
                    else if (pi.PropertyType == typeof(DateTime))
                    {
                        setValue = Convert.ToDateTime(elem.ToString());
                    }

                    pi.SetMethod.Invoke(model, new object[] { setValue });
                }
            }

            return model;

        }
    }
}
