using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TabApp.Helpers
{
    /// <summary>
    /// <see cref="Stream"/> base class extensions.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads all lines in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <returns>All lines in the stream.</returns>
        public static string[] ReadAllLines(this Stream stream) => ReadAllLines(stream, Encoding.UTF8);

        /// <summary>
        /// Writes all lines using <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write with.</param>
        /// <param name="lines">The <see cref="List{T}"/> of data to write.</param>
        /// <returns>true if successful, false otherwise</returns>
        public static bool WriteAllLines(this Stream stream, List<string> lines) => WriteAllLines(stream, lines, Encoding.UTF8);

        /// <summary>
        /// Reads all lines in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>All lines in the stream.</returns>
        public static string[] ReadAllLines(this Stream stream, Encoding encoding)
        {
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                return ReadAllLines(reader).ToArray();
            }
        }

        /// <summary>
        /// Reads all lines in the <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read from.</param>
        /// <returns>All lines in the stream.</returns>
        public static IEnumerable<string> ReadAllLines(this TextReader reader)
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Reads all lines in the <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read from.</param>
        /// <returns>All lines in the stream.</returns>
        public static async IAsyncEnumerable<string> ReadAllLinesAsync(this TextReader reader)
        {
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Writes <paramref name="lines"/> using a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write with.</param>
        /// <returns>true if successful, false otherwise</returns>
        public static bool WriteAllLines(this TextWriter writer, List<string> lines)
        {
            try
            {
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes <paramref name="lines"/> using a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write with.</param>
        /// <returns>true if successful, false otherwise</returns>
        public static async Task<bool> WriteAllLinesAsync(this TextWriter writer, List<string> lines)
        {
            try
            {
                foreach (string line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes <paramref name="lines"/> using a <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write with.</param>
        /// <param name="lines">The <see cref="List{T}"/> of data to write.</param>
        /// <returns>true if successful, false otherwise</returns>
        public static bool WriteAllLines(this Stream stream, List<string> lines, Encoding encoding)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(stream, encoding))
                {
                    foreach (string line in lines)
                    {
                        writer.WriteLine(line);
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes <paramref name="lines"/> using a <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write with.</param>
        /// <param name="lines">The <see cref="List{T}"/> of data to write.</param>
        /// <returns>true if successful, false otherwise</returns>
        public static async Task<bool> WriteAllLinesAsync(this Stream stream, List<string> lines, Encoding encoding)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(stream, encoding))
                {
                    foreach (string line in lines)
                    {
                        await writer.WriteLineAsync(line);
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes all <paramref name="data"/> using a <see cref="TextWriter"/>.
        /// Will work with any object that returns a <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="data"><see cref="char"/> array</param>   
        /// <param name="writer"><see cref="TextWriter"/></param>
        public static async Task<bool> WriteAllData(this TextWriter writer, char[] data)
        {
            if (data.Length > 0)
            {
                try
                {
                    await writer.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// Adds the ability to write <see cref="string"/> array using <see cref="TextWriter.WriteAsync"/>.
        /// Writes all <paramref name="data"/> using a <see cref="TextWriter"/>.
        /// Will work with any object that returns a <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="data"><see cref="string"/> array</param>   
        /// <param name="writer"><see cref="TextWriter"/></param>
        /// <param name="addCRLF">if true, appends 0x0D and 0x0A to each element in the array during</param>
        public static async Task<bool> WriteAllData(this TextWriter writer, string[] data, bool addCRLF = false)
        {
            if (data.Length > 0)
            {
                List<char> charList = new List<char>();
                foreach (string str in data)
                {
                    charList.AddRange(str.ToCharArray());
                    if (addCRLF) { charList.Add('\x0D'); charList.Add('\x0A'); }
                }
                char[] converted = charList.ToArray();
                try
                {
                    await writer.WriteAsync(converted, 0, converted.Length).ConfigureAwait(false);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Nothing to do.");
            }
            return false;
        }


        public static Stream ToStream(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            return new MemoryStream(byteArray);
        }

        public static string ToString(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static Stream StreamFromString(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return new MemoryStream(); // return empty stream

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(str);
                    writer.Flush();
                    stream.Position = 0;
                    return stream;
                }
            }
        }

        public static string StringFromStream(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Copy from one stream to another.
        /// <example><code>
        /// using(var stream = response.GetResponseStream())
        /// using(var ms = new MemoryStream())
        /// {
        ///     stream.CopyTo(ms);
        ///     /* Do something with copied data */
        /// }
        /// </code></example>
        /// </summary>
        /// <param name="fromStream">from <see cref="Stream"/></param>
        /// <param name="toStream">to <see cref="Stream"/></param>
        public static void CopyTo(this Stream fromStream, Stream toStream)
        {
            if (fromStream == null)
            {
                throw new ArgumentNullException("fromStream is null");
            }

            if (toStream == null)
            {
                throw new ArgumentNullException("toStream is null");
            }

            byte[] bytes = new byte[8092];
            int dataRead;
            while ((dataRead = fromStream.Read(bytes, 0, bytes.Length)) > 0)
            {
                toStream.Write(bytes, 0, dataRead);
            }
        }

        public static Stream ToMemoryStream(this System.Xml.XmlDocument doc)
        {
            MemoryStream xmlStream = new MemoryStream();
            doc.Save(xmlStream);
            xmlStream.Flush(); // Adjust this if you want to read your data 
            xmlStream.Position = 0;
            return xmlStream;
        }

        public static void Append(this MemoryStream stream, byte value)
        {
            stream.Append(new[] { value });
        }

        public static void Append(this MemoryStream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }
    }
}
