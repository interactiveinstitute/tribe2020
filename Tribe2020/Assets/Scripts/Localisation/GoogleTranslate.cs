using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp.Net;

public class GoogleTranslate : MonoBehaviour {
	public string ErrorMessage { get; private set; }

	// Use this for initialization
	void Start () {
		string result = TranslateGoogle("The pig is not big, but it's home!", "en-us", "fr-ca");
		Debug.Log("result:" +  result);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Translates a string into another language using Google's translate API JSON calls.
	/// <seealso>Class TranslationServices</seealso>
	/// </summary>
	/// <param name="Text">Text to translate. Should be a single word or sentence.</param>
	/// <param name="FromCulture">
	/// Two letter culture (en of en-us, fr of fr-ca, de of de-ch)
	/// </param>
	/// <param name="ToCulture">
	/// Two letter culture (as for FromCulture)
	/// </param>
	public string TranslateGoogle(string text, string fromCulture, string toCulture) {
		fromCulture = fromCulture.ToLower();
		toCulture = toCulture.ToLower();

		// normalize the culture in case something like en-us was passed 
		// retrieve only en since Google doesn't support sub-locales
		string[] tokens = fromCulture.Split('-');
		if(tokens.Length > 1) {
			fromCulture = tokens[0];
		}

		// normalize ToCulture
		tokens = toCulture.Split('-');
		if(tokens.Length > 1) {
			toCulture = tokens[0];
		}

		string url = string.Format(@"http://translate.google.com/translate_a/t?client=j&text={0}&hl=en&sl={1}&tl={2}",
								   HttpUtility.UrlEncode(text), fromCulture, toCulture);

		// Retrieve Translation with HTTP GET call
		string html = null;
		try {
			WebClient web = new WebClient();

			// MUST add a known browser user agent or else response encoding doen't return UTF-8 (WTF Google?)
			web.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
			web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

			// Make sure we have response encoding to UTF-8
			web.Encoding = Encoding.UTF8;
			html = web.DownloadString(url);

			return html;
		} catch(Exception ex) {
			Debug.Log(ex);

			return null;
		}

		//return "" + html;

		// Extract out trans":"...[Extracted]...","from the JSON string
		//string result = Regex.Match(html, "trans\":(\".*?\"),\"", RegexOptions.IgnoreCase).Groups[1].Value;

		//if(string.IsNullOrEmpty(result)) {
		//	//this.ErrorMessage = Westwind.Globalization.Resources.Resources.InvalidSearchResult;
		//	return null;
		//}

		////return WebUtils.DecodeJsString(result);

		//Debug.Log(result);
		//return result;

		// Result is a JavaScript string so we need to deserialize it properly
		//JavaScriptSerializer ser = new JavaScriptSerializer();
		//return ser.Deserialize(result, typeof(string)) as string;
	}
}
