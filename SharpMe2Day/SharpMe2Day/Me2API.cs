using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using SharpMe2Day.Util;
//using System.Web;
using SharpMe2Day.Model;
using Windows.Data.Xml.Dom;
using System.Net.Http;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace SharpMe2Day
{
    public class Me2API
    {
        /// <summary>
        /// 미투데이 사용자 이름
        /// </summary>
        public String UserID { get; set; }
        /// <summary>
        /// 미투데이 사용자 API Key
        /// </summary>
        public String ApiKey { get; set; }

        /// <summary>
        /// 미투데이 APP Key
        /// </summary>
        public String AppKey { get; set; }

        /// <summary>
        /// 미투데이 Full_Auth_Token
        /// </summary>
        public String Password { get; set; }

        public enum FRIENDS_SCOPE
        {
            ALL,
            CLOSE,
            FAMILY,
            MYTAG
        }
        public enum FRIENDSHIP_SCOPE
        {
            friend,           // 친구신청 관련 내용 처리
            friend_request,   // 친구신청 수락 관련 내용 처리
            friend_sms        // 관심친구 설정과 관련된 내용을 처리
        }

        protected string GetParamString(IDictionary<string, string> param)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> item in param)
            {
                if (sb.Length != 0)
                    sb.Append("&");

                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(WebUtility.UrlEncode(item.Value));
            }

            return sb.ToString();

        }

        protected async Task<XmlDocument> request(Uri url, bool authRequest, IDictionary<string, string> param)
        {
            try
            {
                XmlDocument doc = null;
                WebRequest req = WebRequest.Create(url);
//                HttpClient req = new HttpClient();
                //Cache-Control: 
//                client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
//                Password=me2
                if (authRequest)
                {
                    req.Credentials = new NetworkCredential(UserID, Me2Util.GetAuthPassword(ApiKey));
 //                   req.Credentials = new NetworkCredential(UserID, "full_auth_token " + Password);
                }

                if (!String.IsNullOrEmpty(AppKey))
                {
                    req.Headers["me2_application_key"]= AppKey;
                }

                req.ContentType = "application/x-www-form-urlencoded";
                //req.KeepAlive = false;

                if (param != null)
                {
                    req.Method = "POST";
                    Stream output = await req.GetRequestStreamAsync();
                    StreamWriter output_writer = new StreamWriter(output);
                    output_writer.Write(GetParamString(param));
                    output_writer.Flush();
                    output_writer.Dispose();
                    output.Dispose();
                }

                WebResponse rep = await req.GetResponseAsync();
                Stream input = rep.GetResponseStream();
                StreamReader input_reader = new StreamReader(input, Encoding.UTF8);
                
                doc = new XmlDocument();
                doc.LoadXml(input_reader.ReadToEnd());

                input_reader.Dispose();
                input.Dispose();

                return doc;


            }
            catch (WebException we)
            {
                if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.InternalServerError)
                {
                    Stream input = we.Response.GetResponseStream();
                    StreamReader input_reader = new StreamReader(input, Encoding.UTF8);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(input_reader.ReadToEnd());

                    input_reader.Dispose();
                    input.Dispose();

                    //MessageBox.Show(Me2Util.ParseError(doc).Description);
                    return doc;
                    //throw new Me2Exception(Me2Util.ParseError(doc));
                }
                else
                {
                  //  MessageBox.Show("에러 :: " + we.Message);
                    return new XmlDocument();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        protected async Task<XmlDocument> simpleRequest(Uri url, bool authRequest)
        {
            try
            {
                XmlDocument doc = null;
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);

                if (authRequest)
                {
                    req.Credentials = new NetworkCredential(UserID, "full_auth_token " + Password);
                }

                if (!String.IsNullOrEmpty(AppKey))
                {
                    req.Headers["me2_application_key"] = AppKey;
                }

                req.ContentType = "application/x-www-form-urlencoded";
                //req.KeepAlive = false;

                WebResponse rep = await req.GetResponseAsync();
                Stream input = rep.GetResponseStream();
                StreamReader input_reader = new StreamReader(input, Encoding.UTF8);

                doc = new XmlDocument();
                doc.LoadXml(input_reader.ReadToEnd());

                input_reader.Dispose();
                input.Dispose();

                return doc;


            }
            catch (WebException we)
            {
                //MessageBox.Show(we.Message);
                if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.InternalServerError)
                {
                    Stream input = we.Response.GetResponseStream();
                    StreamReader input_reader = new StreamReader(input, Encoding.UTF8);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(input_reader.ReadToEnd());

                    input_reader.Dispose();
                    input.Dispose();

                    throw new Me2Exception(Me2Util.ParseError(doc));
                }
                else
                {
                    throw we;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 인증 테스트용
        /// </summary>
        /// <returns></returns>
        public async Task<bool> noop()
        {
            Uri url = Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_NOOP);
            try
            {
                XmlDocument doc = await request(url, true, null);

                return Me2Util.ParseError(doc).Code == 0;
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    return false;
                }

                throw e;
            }

        }
        /// <summary>
        /// 해당 글에 미투합니다
        /// </summary>
        /// <param name="post_id"></param>
        /// <returns>성공여부</returns>
        public async Task<bool> metoo(string post_id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("post_id", post_id);

            Me2Error result = Me2Util.ParseError(await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_METOO), true, param));
            return result.Code == 0;
        }

        /// <summary>
        /// 친구 신청, 친구신청 수락, 관심친구 설정, 관심친구 해제를 처리합니다.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="value">true = 친구신청, 관심친구 설정, 친구신청 수락, false = 친구신청 해제, 관심친구 해제, 친구신청 무시</param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public async Task<bool> friendship(FRIENDSHIP_SCOPE scope, bool value, string user_id)
        {
            string real_value;
            if (value == true) { real_value = "on"; }
            else { real_value = "off"; }

            Dictionary<string, string> param = new Dictionary<string, string>();
            if (scope == FRIENDSHIP_SCOPE.friend)
            {
                param.Add("scope", "friend");
            }
            else if (scope == FRIENDSHIP_SCOPE.friend_request)
            {
                param.Add("scope", "friend_request");
            }
            else
            {
                param.Add("scope", "friend_sms");
            }
            param.Add("value", real_value);
            param.Add("user_id", user_id);

            Me2Error result = Me2Util.ParseError(await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_FRIENDSHIP), true, param));
            return result.Code == 0;
        }
        /// <summary>
        /// 친구 신청용 테스트 메소드입니다.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="value"></param>
        /// <param name="user_id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> friendship(FRIENDSHIP_SCOPE scope, bool value, string user_id, string message)
        {
            string real_value;
            if (value == true) { real_value = "on"; }
            else { real_value = "off"; }

            Dictionary<string, string> param = new Dictionary<string, string>();
            if (scope == FRIENDSHIP_SCOPE.friend)
            {
                param.Add("scope", "friend");
            }
            else if (scope == FRIENDSHIP_SCOPE.friend_request)
            {
                param.Add("scope", "friend_request");
            }
            else
            {
                param.Add("scope", "friend_sms");
            }
            param.Add("value", real_value);
            param.Add("user_id", user_id);
            param.Add("message", message);

            Me2Error result = Me2Util.ParseError(await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_FRIENDSHIP), true, param));
            return result.Code == 0;
        }
        /// <summary>
        /// 내게 온 친구 신청을 수락합니다.
        /// </summary>
        /// <param name="friendship_request_id">수락할 친구 신청 id</param>
        /// <param name="message">친구 신청 수락시 요청자에게 보여질 메세지</param>
        /// <returns></returns>
        public async Task<bool> acceptFriendshipRequest(string friendship_request_id, string message)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("friendship_request_id", friendship_request_id);
            param.Add("message", message);

            Me2Error result = Me2Util.ParseError(await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_ACCEPT_FRIENDSHIP_REQUEST), true, param));
            return result.Code == 0;
        }
        /// <summary>
        /// 지정한 사용자의 친구들 목록을 가져 옵니다. ( mytag: 인증 필요. 본인의 것만 가능 )
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scope"></param>
        /// <param name="mytag"></param>
        /// <returns></returns>
        public async Task<List<Person>> GetFriends(string id, FRIENDS_SCOPE scope, params object[] mytag)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            bool bMustAuth = false;

            if (scope == FRIENDS_SCOPE.ALL)
                param = null;
            else if (scope != FRIENDS_SCOPE.MYTAG)
            {
                param.Add("scope", Enum.GetName(typeof(FRIENDS_SCOPE), scope).ToLower());
            }
            else
            {
                param.Add("scope", "mytag[" + mytag[0].ToString() + "]");
                bMustAuth = true;
            }

            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_FRIENDS, id), bMustAuth, param);

            List<Person> result = new List<Person>();

            XmlNodeList friends = ret.SelectNodes("//person");
            foreach (IXmlNode personNode in friends)
            {
                Person person = Me2Util.FromXml<Person>(personNode);
                result.Add(person);
            }

            return result;

        }
        /// <summary>
        /// 나에게 온 친구 요청 리스트를 반환합니다.
        /// </summary>
        /// <param name="id">미투데이 아이디</param>
        /// <returns></returns>
        public async Task<List<Person>> getFriendshipRequests(string id)
        {
            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_FRIENDSHIP_REQUESTS, id), true, null);

            List<Person> result = new List<Person>();

            XmlNodeList persons = ret.SelectNodes("//from//person");
            XmlNodeList reqMessage = ret.GetElementsByTagName("message");
            XmlNodeList reqID = ret.SelectNodes("//id");
            int count = 0;
            foreach (IXmlNode postNode in persons)
            {
                Person person = Me2Util.FromXml<Person>(postNode);
                person.request_message = reqMessage[count].InnerText;
                person.request_id = reqID[count].InnerText;
                result.Add(person);
                ++count;
            }

            return result;
        }

        /// <summary>
        /// 해당 사용자의 최근글을 가져옵니다.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Post>> getLatest(string id)
        {
            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_LATESTS, id), false, null);

            List<Post> result = new List<Post>();

            XmlNodeList posts = ret.SelectNodes("//post");
            foreach (IXmlNode postNode in posts)
            {
                Post post = Me2Util.FromXml<Post>(postNode);

                if (postNode.SelectNodes("tags").Count > 0)
                {
                    foreach (IXmlNode tagNode in postNode.SelectNodes("tags//tag"))
                    {
                        Tag tag = Me2Util.FromXml<Tag>(tagNode);
                        post.Tags.Add(tag);
                    }
                }

                Person author = Me2Util.FromXml<Person>(postNode.SelectSingleNode("author"));
                post.Author = author;
                result.Add(post);
            }

            return result;

        }
        /// <summary>
        /// 현재 사용자의 글 리스트를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Post>> getPosts(string id)
        {
            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_POSTS, id), false, null);

            List<Post> result = new List<Post>();

            XmlNodeList posts = ret.SelectNodes("//post");
            foreach (IXmlNode postNode in posts)
            {
                Post post = Me2Util.FromXml<Post>(postNode);

                if (postNode.SelectNodes("tags").Count > 0)
                {
                    foreach (IXmlNode tagNode in postNode.SelectNodes("tags//tag"))
                    {
                        Tag tag = Me2Util.FromXml<Tag>(tagNode);
                        post.Tags.Add(tag);
                    }
                }

                Person author = Me2Util.FromXml<Person>(postNode.SelectSingleNode("author"));
                post.Author = author;
                result.Add(post);
            }

            return result;
        }
        /// <summary>
        /// 모든 친구의 글을 가져옵니다
        /// </summary>
        /// <returns></returns>
        public async Task<List<Post>> getAllFriendsPosts(string id)
        {
            XmlDocument ret = await simpleRequest(new Uri("http://me2day.net/api/get_posts/" + id + ".xml?scope=friend[all]"), true);

            List<Post> result = new List<Post>();

            XmlNodeList posts = ret.SelectNodes("//post");
            foreach (IXmlNode postNode in posts)
            {
                Post post = Me2Util.FromXml<Post>(postNode);

                if (postNode.SelectNodes("tags").Count > 0)
                {
                    foreach (IXmlNode tagNode in postNode.SelectNodes("tags//tag"))
                    {
                        Tag tag = Me2Util.FromXml<Tag>(tagNode);
                        post.Tags.Add(tag);
                    }
                }

                Person author = Me2Util.FromXml<Person>(postNode.SelectSingleNode("author"));
                post.Author = author;
                result.Add(post);
            }

            return result;
        }

        /// <summary>
        /// 지정된 글의 퍼머링크에 달려 있는 댓글을 가져 옵니다.
        /// </summary>
        /// <param name="permalink"></param>
        /// <returns></returns>
        public async Task<List<Comment>> getComments(string permalink)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("post_id", permalink);

            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_COMMENTS), false, param);
            List<Comment> result = new List<Comment>();

            XmlNodeList comments = ret.SelectNodes("//comment");

            foreach (IXmlNode commentNode in comments)
            {
                Comment comment = Me2Util.FromXml<Comment>(commentNode);
                Person author = Me2Util.FromXml<Person>(commentNode.SelectSingleNode("author"));
                comment.Author = author;
                result.Add(comment);
            }

            return result;
        }
        /// <summary>
        /// 지정된 post_id 글의 댓글 리스트를 가져옵니다
        /// </summary>
        /// <param name="post_id">각 글의 고유 post_id</param>
        /// <returns></returns>
        public async Task<List<Comment>> getCommentsByPostID(string post_id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("post_id", post_id);

            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_COMMENTS), false, param);
            List<Comment> result = new List<Comment>();

            XmlNodeList comments = ret.SelectNodes("//comment");

            foreach (IXmlNode commentNode in comments)
            {
                Comment comment = Me2Util.FromXml<Comment>(commentNode);
                Person author = Me2Util.FromXml<Person>(commentNode.SelectSingleNode("author"));
                comment.Author = author;
                result.Add(comment);
            }

            return result;
        }

        /// <summary>
        /// 해당 글에 미투한 사람의 리스트를 반환합니다
        /// </summary>
        /// <param name="post_id"></param>
        /// <returns></returns>
        public async Task<List<Post>> getMetoos(string post_id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("post_id", post_id);

            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_METOOS), false, param);
            List<Post> result = new List<Post>();

            XmlNodeList metoos = ret.SelectNodes("//metoo");

            foreach (IXmlNode metooNode in metoos)
            {
                Post post = Me2Util.FromXml<Post>(metooNode);
                Person author = Me2Util.FromXml<Person>(metooNode.SelectSingleNode("author"));
                post.Author = author;
                result.Add(post);
            }
            return result;
        }

        /// <summary>
        /// 미투데이 가입자 정보를 가져옵니다.
        /// </summary>
        /// <param name="id">아이디 or post_id or 퍼머링크/param>
        /// <returns></returns>
        public async Task<List<Person>> getPerson(string id)
        {
            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_GET_PERSON, id), false, null);

            List<Person> result = new List<Person>();

            XmlNodeList person = ret.SelectNodes("//person");
            foreach (IXmlNode postNode in person)
            {
                Person p = Me2Util.FromXml<Person>(postNode);
                result.Add(p);
            }

            return result;
        }

        /// <summary>
        /// 글을 작성합니다.
        /// </summary>
        /// <param name="body">본문</param>
        /// <param name="tags">태그</param>
        /// <param name="icon">아이콘 번호(1~12)</param>
        /// <returns></returns>
        public async Task<Post> createPost(string id, string body, List<string> tags, int icon)
        {

            if (icon > 12 || icon <= 0)
                throw new ArgumentOutOfRangeException("아이콘 번호는 1-12 사이여야 합니다.");

            if (String.IsNullOrEmpty(body))
                throw new ArgumentNullException("본문을 입력하셔야 합니다.");

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("post[body]", body);
            param.Add("post[tags]", String.Join(" ", tags.ToArray()));
            param.Add("post[icon_index]", icon.ToString());

            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_CREATE_POST, id), true, param);
            return Me2Util.FromXml<Post>(ret.SelectSingleNode("//post"));

        }
        public async Task<Post> createPost(string id, string body)
        {
            if (String.IsNullOrEmpty(body))
                throw new ArgumentNullException("본문을 입력하셔야 합니다.");

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("post[body]", body);

            XmlDocument ret = await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_CREATE_POST, id), true, param);
            return Me2Util.FromXml<Post>(ret.SelectSingleNode("//post"));
        }

        /// <summary>
        /// 댓글을 기록합니다. 성공/실패 여부만을 리턴합니다.
        /// </summary>
        /// <param name="post_id"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public async Task<bool> createComment(string post_id, string comment)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("body", comment);
            param.Add("post_id", post_id);

            Me2Error result = Me2Util.ParseError(await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_CREATE_COMMENT), true, param));
            return result.Code == 0;

        }
        public async Task<bool> deletePost(string post_id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("post_id", post_id);

            Me2Error result = Me2Util.ParseError(await request(Me2Util.GetAPIUrl(Me2Util.API_METHOD_TYPE.ME2DAY_API_DELETE_POST), true, param));
            return result.Code == 0;
        }

        /// 사용자 인증 요청
        /// 반환 값 XML

        public async Task<XmlDocument> getAuthResult()
        {
            return await request(new Uri("http://me2day.net/api/get_auth_url.xml?akey=" + AppKey), false, null);
        }

        /// <summary>
        /// 토큰으로 세션키를 받아오는 메소드
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<XmlDocument> getSessionKey(string token)
        {
            return await request(new Uri("http://me2day.net/api/get_full_auth_token.xml?token=" + token), false, null);
        }
        public async Task<XmlDocument> Authenticate(string username, string password, string appKey)
        {
            return await request(new Uri("http://me2day.net/api/noop?uid=" + username + "&ukey=full_auth_token " + password + "&akey=" + appKey), false, null);
        }
    }
}

