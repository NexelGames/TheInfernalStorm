using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Text;

namespace Game.Core {

	/// <summary>
	/// Save JSON Serializer.
	/// </summary>
	public class SaveJSONSerializer : ISaveSerializer {
		public JsonSerializerSettings SerializerSettings = new JsonSerializerSettings() {
			TypeNameHandling = TypeNameHandling.Auto
		};

		/// <summary>
		/// Serialize the specified object to stream with encoding.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="stream">Stream.</param>
		/// <param name="encoding">Encoding.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void Serialize<T>(T obj, Stream stream, Encoding encoding) {
			try {
				var json = JsonConvert.SerializeObject(obj, Formatting.Indented, SerializerSettings);
				StreamWriter sw;
				if (encoding != null) {
					sw = new StreamWriter(stream, encoding);
				}
                else {
					sw = new StreamWriter(stream);
                }
				sw.Write(json);
				sw.Dispose();
			}
			catch (Exception ex) {
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Deserialize the specified object from stream using the encoding.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="encoding">Encoding.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T Deserialize<T>(Stream stream, Encoding encoding) {
			T result = default(T);
			
			try {
				StreamReader sr;
				if (encoding != null) {
					sr = new StreamReader(stream, encoding);
				}
				else {
					sr = new StreamReader(stream);
				}
				result = JsonConvert.DeserializeObject<T>(sr.ReadToEnd(), SerializerSettings);
				sr.Dispose();
			}
			catch (Exception ex) {
				Debug.LogException(ex);
			}
			return result;
		}
	}
}
