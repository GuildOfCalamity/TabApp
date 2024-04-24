using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    }
}
