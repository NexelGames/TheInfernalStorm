using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class that provides email related functions
/// </summary>
public static class EmailHelper
{
	private const string COMPANY_EMAIL = "okiddygames@gmail.com";

	/// <summary>
	/// Opens email send by url with subject and body ready
	/// </summary>
	/// <param name="email">Email message reciever</param>
	/// <param name="subject">Subject of email message</param>
	/// <param name="body">Body of email message</param>
	public static void SendMailExternal(string email, string subject, string body) {
		string url = string.Format("mailto:{0}?subject={1}&body={2}", email, MyEscapeURL(subject), MyEscapeURL(body));
		Application.OpenURL(url);
	}

	/// <summary>
	/// Opens email send by url with subject and body ready to <see cref="COMPANY_EMAIL"/>
	/// </summary>
	/// <param name="subject">Subject of email message</param>
	/// <param name="body">Body of email message</param>
	public static void SendMailExternal(string subject, string body) {
		SendMailExternal(COMPANY_EMAIL, subject, body);
	}

	private static string MyEscapeURL(string url) {
		return UnityWebRequest.EscapeURL(url).Replace("+", "%20"); // to avoid "+" in mail send text instead of spaces
	}
}
